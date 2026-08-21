using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;
using Helios.Bridge.Host.Instruments.Simulated;
using Helios.Bridge.Host.Services;
using Helios.Interfaces;
using Microsoft.Win32;

namespace Helios.Bridge.Host
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      // For zip-file installs with no installer to write the registry entry: run once with
      // --register to advertise this copy's location to Helios.Client's auto-launch discovery,
      // then exit without starting the server.
      if (args.Length > 0 && string.Equals(args[0], "--register", StringComparison.OrdinalIgnoreCase))
      {
        RegisterInstallLocation();
        Console.WriteLine("Registered this Helios.Bridge.Host location for auto-launch. You can close this window.");
        return;
      }

      string family = ConfigurationManager.AppSettings["InstrumentFamily"] ?? "Auto";
      int port = int.TryParse(ConfigurationManager.AppSettings["Port"], out var configuredPort) ? configuredPort : 50100;
      int idleShutdownSeconds = int.TryParse(ConfigurationManager.AppSettings["IdleShutdownSeconds"], out var configuredIdle) ? configuredIdle : 20;

      IInstrumentGateway gateway = CreateGateway(family);

      Console.WriteLine($"Helios.Bridge.Host starting -- backend={gateway.InstrumentFamily}, port={port}");

      var shutdownSignal = new ManualResetEventSlim();
      Console.CancelKeyPress += (_, e) =>
      {
        e.Cancel = true;
        shutdownSignal.Set();
      };

      var watchdog = new ConnectionWatchdog(
        idleShutdownSeconds > 0 ? TimeSpan.FromSeconds(idleShutdownSeconds) : Timeout.InfiniteTimeSpan,
        () =>
        {
          Console.WriteLine("Helios.Bridge.Host: idle -- no clients connected, shutting down.");
          shutdownSignal.Set();
        });

      // Loopback-only, no TLS -- this process and its .NET 8 client are expected to run on the
      // same instrument-control PC. Skipping the TLS handshake removes real latency from the
      // hottest path (scan streaming); see Helios.Client for the matching plaintext-H2C switch
      // that has to be set on the .NET 8 side for this to work.
      var server = new Server(new[]
      {
        // Idle connections must live indefinitely (HeliosClient.KeepAlivePingDelay keeps a stream
        // open with no data flowing for as long as the client stays connected). The .NET client
        // sends an HTTP/2 keepalive ping every 30s while idle; C-core's default floor for pings
        // without data is 5 minutes, so those pings would otherwise accrue "ping strikes" and the
        // server would GOAWAY (too_many_pings), killing a perfectly healthy idle connection.
        new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 20000),
        new ChannelOption("grpc.keepalive_permit_without_calls", 1),
      })
      {
        Services =
        {
          InstrumentService.BindService(new InstrumentServiceImpl(gateway, watchdog)),
          ScanStreamService.BindService(new ScanStreamServiceImpl(gateway)),
          InstrumentControlService.BindService(new InstrumentControlServiceImpl(gateway)),
          SyringePumpService.BindService(new SyringePumpServiceImpl(gateway)),
          AnalogTraceService.BindService(new AnalogTraceServiceImpl(gateway)),
        },
      };

      // Server.Ports.Add returns the actual bound port, or 0 if the bind failed -- Grpc.Core
      // doesn't throw for "address already in use" the way a raw socket bind would. That return
      // value is this process's only signal that another Helios.Bridge.Host already owns this
      // port; there's no separate single-instance mutex, since this check is already exact and
      // race-free (the OS resolves simultaneous binds atomically).
      int boundPort = server.Ports.Add(new ServerPort("127.0.0.1", port, ServerCredentials.Insecure));
      if (boundPort == 0)
      {
        Console.WriteLine($"Port {port} is already in use -- another Helios.Bridge.Host is almost certainly already running. Exiting.");
        gateway.Dispose();
        return;
      }

      server.Start();

      // Only the instance that actually won the port above registers itself -- the one an
      // auto-launching Helios.Client would actually be able to reach.
      RegisterInstallLocation();

      // Connection is client-driven: whoever calls the Connect RPC (InstrumentServiceImpl.Connect)
      // triggers gateway.ConnectAsync. Don't also connect here -- IInstrumentGateway.ConnectAsync
      // isn't safe to call twice (it would, e.g., spin up a second scan-generation clock/timer on
      // the Simulated backend, or re-subscribe every event twice against Helios.dll).
      Console.WriteLine(idleShutdownSeconds > 0
        ? $"Server started. Waiting for a client to call Connect. Shuts down {idleShutdownSeconds}s after the last client disconnects, or press Ctrl+C to stop now."
        : "Server started. Waiting for a client to call Connect. Press Ctrl+C to stop.");
      shutdownSignal.Wait();

      Console.WriteLine("Shutting down...");
      gateway.Dispose();
      server.ShutdownAsync().Wait();
    }

    // Advertises this process's own executable path in the registry so Helios.Client's
    // auto-launch discovery can find it later, regardless of where it was installed or how the
    // calling app was built (NuGet consumers included -- see BridgeHostLocator in Helios.Client).
    // Written on every successful startup, not just once, so a reinstalled/moved host keeps the
    // key accurate without needing an explicit uninstall/unregister step; a stale entry (host
    // deleted without re-registering) is instead cleaned up lazily by the client when it notices
    // the path no longer exists.
    private static void RegisterInstallLocation()
    {
      try
      {
        string exePath = Process.GetCurrentProcess().MainModule?.FileName
          ?? throw new InvalidOperationException("Could not determine this process's own executable path.");
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\SchweppeLab\Helios");
        key.SetValue("BridgeHostPath", exePath);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Helios.Bridge.Host: could not register install location in the registry -- Helios.Client's auto-launch won't find this instance until it does: {ex.Message}");
      }
    }

    // "Real" requires a real instrument or Corona (VMS) and fails outright if Create() finds
    // nothing, for testing hardware-only paths without a silent fallback masking a real problem.
    // "Simulated" always uses the synthetic generator, e.g. for the 1000Hz pipeline stress test.
    // "Auto" (the default -- an empty/unset config value also means this) restores the behavior
    // ScanSpy/Helios consumers got for free when they linked Helios.dll in-process: probe for a
    // real instrument or Corona first (InstrumentAccessContainerFactory.Create()'s own order --
    // Fusion, then Exploris, then VMS/Corona), and only fall back to the Simulated backend if
    // none of those answered. This probe (Check() on each candidate) is cheap and doesn't start
    // online access -- that still only happens once a client calls Connect, same as before.
    private static IInstrumentGateway CreateGateway(string family)
    {
      if (string.Equals(family, "Simulated", StringComparison.OrdinalIgnoreCase))
      {
        return new SimulatedInstrumentGateway(ReadSimulatedOptions());
      }

      var probed = InstrumentAccessContainerFactory.Create();
      if (probed is not null)
      {
        return new HeliosInstrumentGateway(probed);
      }

      if (string.Equals(family, "Real", StringComparison.OrdinalIgnoreCase))
      {
        throw new InvalidOperationException("InstrumentFamily=Real but no instrument or Corona (VMS) could be reached.");
      }

      Console.WriteLine("No instrument or Corona (VMS) reachable -- falling back to the Simulated backend.");
      return new SimulatedInstrumentGateway(ReadSimulatedOptions());
    }

    private static SimulatedOptions ReadSimulatedOptions()
    {
      var options = new SimulatedOptions();
      if (int.TryParse(ConfigurationManager.AppSettings["SimulatedScanIntervalMs"], out var scanIntervalMs))
      {
        options = new SimulatedOptions { ScanInterval = TimeSpan.FromMilliseconds(scanIntervalMs) };
      }
      return options;
    }
  }
}
