using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  // Entry point: the .NET 8 equivalent of Helios's own
  // Helios.Interfaces.InstrumentAccessContainerFactory.Create(), except the "instrument" being
  // reached is a Helios.Bridge.Host process over gRPC rather than IAPI in-process.
  public static class HeliosClient
  {
    private static readonly TimeSpan ListeningProbeTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan LaunchWaitTimeout = TimeSpan.FromSeconds(15);

    static HeliosClient()
    {
      // Helios.Bridge.Host (Grpc.Core on .NET Framework 4.8) speaks plaintext HTTP/2 -- there's no
      // TLS handshake to pay for on what's meant to be a same-machine loopback call, but .NET's
      // SocketsHttpHandler refuses unencrypted HTTP/2 by default. Easy to forget when extending
      // this pattern elsewhere; without it every call fails at the transport layer.
      AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    // Auto-launches Helios.Bridge.Host if it isn't already reachable at host:port -- checked
    // first, since another app may already have one running (and connected to a real instrument)
    // that this call should just share rather than start a redundant instance of. hostExecutablePath
    // is only consulted if a launch turns out to be necessary; see BridgeHostLocator for the full
    // discovery chain (explicit path, env var, registry, PATH) used when it's null. If two callers
    // race and both decide to launch, only one host process ends up actually bound to the port
    // (Helios.Bridge.Host's own Server.Ports.Add failure check) -- the other exits immediately, and
    // both callers' wait-for-listening loop converges on whichever one won.
    public static async Task<IInstrumentAccess> ConnectAsync(string host = "127.0.0.1", int port = 50100, int instrumentIndex = 1, string? hostExecutablePath = null, CancellationToken cancellationToken = default)
    {
      if (IsLoopback(host) && !await IsHostListeningAsync(host, port, cancellationToken).ConfigureAwait(false))
      {
        string exePath = BridgeHostLocator.Locate(hostExecutablePath);
        LaunchHost(exePath, port);
        await WaitForHostListeningAsync(host, port, cancellationToken).ConfigureAwait(false);
      }

      var access = new GrpcInstrumentAccess(host, port);
      await access.ConnectAsync(instrumentIndex, cancellationToken).ConfigureAwait(false);
      return access;
    }

    private static bool IsLoopback(string host) =>
      host is "127.0.0.1" or "localhost" or "::1";

    private static async Task<bool> IsHostListeningAsync(string host, int port, CancellationToken cancellationToken)
    {
      using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      timeoutCts.CancelAfter(ListeningProbeTimeout);
      try
      {
        using var probe = new TcpClient();
        await probe.ConnectAsync(host, port, timeoutCts.Token).ConfigureAwait(false);
        return true;
      }
      catch (Exception) when (!cancellationToken.IsCancellationRequested)
      {
        return false; // not listening yet, or refused/timed out -- all mean "not up", not an error here.
      }
    }

    private static void LaunchHost(string exePath, int port)
    {
      // Not tracked/awaited beyond this -- Helios.Bridge.Host manages its own lifetime (idle
      // auto-shutdown once every client disconnects; see ConnectionWatchdog on the host side), so
      // there's nothing for the launching client to own or clean up.
      //
      // Run through cmd.exe's own '>' redirection rather than Process.Start's
      // RedirectStandardOutput -- that would need this client to keep draining the pipe for as
      // long as the host runs, but the host is designed to outlive whichever client happened to
      // launch it. A .NET-side pipe would back up and hang the host's own Console.WriteLine calls
      // the moment this client exits and stops reading it; cmd's redirection hands the host a
      // plain file handle instead, so no reader is needed. This also means no console window is
      // ever created for the host process itself -- CreateNoWindow below only has to hide cmd.exe.
      string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SchweppeLab", "Helios");
      Directory.CreateDirectory(logDirectory);
      string logPath = Path.Combine(logDirectory, $"Helios.Bridge.Host.{port}.log");

      Process.Start(new ProcessStartInfo
      {
        FileName = "cmd.exe",
        Arguments = $"/c \"\"{exePath}\" > \"{logPath}\" 2>&1\"",
        WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
        UseShellExecute = false,
        CreateNoWindow = true,
      });
    }

    private static async Task WaitForHostListeningAsync(string host, int port, CancellationToken cancellationToken)
    {
      var deadline = DateTime.UtcNow + LaunchWaitTimeout;
      while (DateTime.UtcNow < deadline)
      {
        if (await IsHostListeningAsync(host, port, cancellationToken).ConfigureAwait(false)) return;
        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
      }
      throw new TimeoutException($"Launched Helios.Bridge.Host but it did not start listening on {host}:{port} within {LaunchWaitTimeout.TotalSeconds:F0}s.");
    }

    // Fire-and-forget commands (SetMode, StartAcquisition, syringe pump Start/Stop/...) still need
    // their eventual failure to go somewhere instead of becoming an unobserved task exception --
    // this is that somewhere, until/unless a caller wants a real error-reporting hook.
    internal static void FireAndForget(Task task) =>
      task.ContinueWith(t => Debug.WriteLine($"Helios.Client: fire-and-forget call failed: {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
  }

  internal sealed class GrpcInstrumentAccess : IInstrumentAccess
  {
    private readonly GrpcChannel _channel;
    private readonly Contracts.InstrumentService.InstrumentServiceClient _instrument;
    private readonly CancellationTokenSource _lifetime = new();
    private Task? _serviceEventsPump;

    private readonly Dictionary<int, GrpcMsScanContainer> _msScanContainers = new();
    private readonly Dictionary<int, GrpcAnalogTraceContainer> _analogTraceContainers = new();

    public GrpcInstrumentAccess(string host, int port)
    {
      var handler = new SocketsHttpHandler
      {
        EnableMultipleHttp2Connections = true,
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
        UseProxy = false,
      };

      // One long-lived channel, reused for every call and stream -- HTTP/2 multiplexes them all
      // over the same connection, so there's no per-call connection setup cost to pay.
      _channel = GrpcChannel.ForAddress($"http://{host}:{port}", new GrpcChannelOptions { HttpHandler = handler });

      _instrument = new Contracts.InstrumentService.InstrumentServiceClient(_channel);
      _controlClient = new Contracts.InstrumentControlService.InstrumentControlServiceClient(_channel);
      _scanStreamClient = new Contracts.ScanStreamService.ScanStreamServiceClient(_channel);
      _syringeClient = new Contracts.SyringePumpService.SyringePumpServiceClient(_channel);
      _analogTraceClient = new Contracts.AnalogTraceService.AnalogTraceServiceClient(_channel);

      // Control isn't built until ConnectAsync completes -- SyringePumpControl needs
      // StatusResponse.HasSyringePump (only known post-connect) to decide whether it's null,
      // mirroring Helios.dll's own IControl.SyringePumpControl exactly (null for Exploris) rather
      // than always-non-null-plus-a-separate-flag-to-check.
      Control = null!;
    }

    private readonly Contracts.InstrumentControlService.InstrumentControlServiceClient _controlClient;
    private readonly Contracts.ScanStreamService.ScanStreamServiceClient _scanStreamClient;
    private readonly Contracts.SyringePumpService.SyringePumpServiceClient _syringeClient;
    private readonly Contracts.AnalogTraceService.AnalogTraceServiceClient _analogTraceClient;

    public IControl Control { get; private set; }

    public bool Connected { get; private set; }
    public int InstrumentId { get; private set; }
    public string InstrumentName { get; private set; } = string.Empty;
    public string[] DetectorClasses { get; private set; } = Array.Empty<string>();
    public int CountMsDetectors { get; private set; }
    public int CountAnalogChannels { get; private set; }
    public InstrumentFamily Family { get; private set; }

    public event EventHandler<EventArgs>? ConnectionChanged;
    public event EventHandler<ContactClosureEventArgs>? ContactClosureChanged;
    public event EventHandler<MessagesArrivedEventArgs>? MessagesArrived;

    public async Task ConnectAsync(int instrumentIndex, CancellationToken cancellationToken)
    {
      var response = await _instrument.ConnectAsync(new Contracts.ConnectRequest { InstrumentIndex = instrumentIndex }, cancellationToken: cancellationToken).ConfigureAwait(false);
      ApplyStatus(response);
      Control = new GrpcControl(_controlClient, _syringeClient, response.HasSyringePump, _lifetime.Token);

      _serviceEventsPump = PumpServiceEventsAsync(_lifetime.Token);
    }

    private void ApplyStatus(Contracts.StatusResponse response)
    {
      Connected = response.InstrumentConnected;
      InstrumentId = response.InstrumentId;
      InstrumentName = response.InstrumentName;
      CountMsDetectors = response.CountMsDetectors;
      CountAnalogChannels = response.CountAnalogChannels;
      Family = Mapping.ToClient(response.Family);

      var detectors = new string[response.DetectorClasses.Count];
      for (int i = 0; i < response.DetectorClasses.Count; i++) detectors[i] = response.DetectorClasses[i];
      DetectorClasses = detectors;
    }

    private async Task PumpServiceEventsAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _instrument.StreamServiceEvents(new Contracts.Empty(), cancellationToken: cancellationToken);
        await foreach (var e in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          switch (e.EventCase)
          {
            case Contracts.ServiceEvent.EventOneofCase.InstrumentConnectionChanged:
              Connected = e.InstrumentConnectionChanged.Connected;
              ConnectionChanged?.Invoke(this, EventArgs.Empty);
              break;
            case Contracts.ServiceEvent.EventOneofCase.ContactClosureChanged:
              ContactClosureChanged?.Invoke(this, new ContactClosureEventArgs { RisingEdges = e.ContactClosureChanged.RisingEdges, FallingEdges = e.ContactClosureChanged.FallingEdges });
              break;
            case Contracts.ServiceEvent.EventOneofCase.MessagesArrived:
              MessagesArrived?.Invoke(this, new MessagesArrivedEventArgs(Mapping.ToClient(e.MessagesArrived.Messages)));
              break;
          }
        }
      }
      catch (OperationCanceledException)
      {
        // Expected on Dispose.
      }
    }

    public IMsScanContainer GetMsScanContainer(int msDetectorSet)
    {
      if (!_msScanContainers.TryGetValue(msDetectorSet, out var container))
      {
        string detectorClass = msDetectorSet >= 0 && msDetectorSet < DetectorClasses.Length ? DetectorClasses[msDetectorSet] : string.Empty;
        container = new GrpcMsScanContainer(_scanStreamClient, msDetectorSet, detectorClass, _lifetime.Token);
        _msScanContainers[msDetectorSet] = container;
      }
      return container;
    }

    // CountAnalogChannels comes from the connect-time StatusResponse, so this is a cheap local
    // check rather than a round trip -- consistent with keeping IInstrumentAccess's synchronous
    // members backed by connect-time/streamed state, not blocking calls.
    public IAnalogTraceContainer? GetAnalogTraceContainer(int analogDetectorSet)
    {
      if (analogDetectorSet < 0 || analogDetectorSet >= CountAnalogChannels) return null;

      if (!_analogTraceContainers.TryGetValue(analogDetectorSet, out var container))
      {
        container = new GrpcAnalogTraceContainer(_analogTraceClient, analogDetectorSet, _lifetime.Token);
        _analogTraceContainers[analogDetectorSet] = container;
      }
      return container;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetInstrumentValuesAsync(IReadOnlyList<string>? names = null, CancellationToken cancellationToken = default)
    {
      var request = new Contracts.GetInstrumentValuesRequest();
      if (names is not null) request.Names.Add(names);

      var response = await _controlClient.GetInstrumentValuesAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
      var result = new Dictionary<string, string>(response.Values.Count);
      foreach (var kv in response.Values) result[kv.Key] = kv.Value;
      return result;
    }

    public async ValueTask DisposeAsync()
    {
      _lifetime.Cancel();
      if (_serviceEventsPump is not null)
      {
        try { await _serviceEventsPump.ConfigureAwait(false); } catch (OperationCanceledException) { }
      }
      await _channel.ShutdownAsync().ConfigureAwait(false);
      _channel.Dispose();
      _lifetime.Dispose();
    }
  }

  internal sealed class GrpcControl : IControl
  {
    internal Contracts.InstrumentControlService.InstrumentControlServiceClient InstrumentControl { get; }

    public GrpcControl(Contracts.InstrumentControlService.InstrumentControlServiceClient control, Contracts.SyringePumpService.SyringePumpServiceClient syringe, bool hasSyringePump, CancellationToken lifetime)
    {
      InstrumentControl = control;
      Acquisition = new GrpcAcquisition(control, lifetime);
      Scans = new GrpcScans(control, lifetime);
      // null for Exploris, matching Helios.dll's own IControl.SyringePumpControl exactly -- a host
      // connected to Exploris would still technically answer SyringePumpService calls (with
      // StatusCode.Unimplemented), but there's no reason to hand callers a client for a peripheral
      // that doesn't exist just to make them check a flag before every use.
      SyringePumpControl = hasSyringePump ? new GrpcSyringePumpControl(syringe, lifetime) : null;
    }

    public IAcquisition Acquisition { get; }
    public IScans Scans { get; }
    public ISyringePumpControl? SyringePumpControl { get; }
  }
}
