using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
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
    static HeliosClient()
    {
      // Helios.Bridge.Host (Grpc.Core on .NET Framework 4.8) speaks plaintext HTTP/2 -- there's no
      // TLS handshake to pay for on what's meant to be a same-machine loopback call, but .NET's
      // SocketsHttpHandler refuses unencrypted HTTP/2 by default. Easy to forget when extending
      // this pattern elsewhere; without it every call fails at the transport layer.
      AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    public static async Task<IInstrumentAccess> ConnectAsync(string host = "127.0.0.1", int port = 50100, int instrumentIndex = 1, CancellationToken cancellationToken = default)
    {
      var access = new GrpcInstrumentAccess(host, port);
      await access.ConnectAsync(instrumentIndex, cancellationToken).ConfigureAwait(false);
      return access;
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
