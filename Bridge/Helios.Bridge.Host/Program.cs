using System;
using System.Configuration;
using System.Threading;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;
using Helios.Bridge.Host.Instruments.Simulated;
using Helios.Bridge.Host.Services;
using Helios.Interfaces;

namespace Helios.Bridge.Host
{
  internal static class Program
  {
    private static void Main()
    {
      string family = ConfigurationManager.AppSettings["InstrumentFamily"] ?? "Auto";
      int port = int.TryParse(ConfigurationManager.AppSettings["Port"], out var configuredPort) ? configuredPort : 50100;

      IInstrumentGateway gateway = CreateGateway(family);

      Console.WriteLine($"Helios.Bridge.Host starting -- backend={gateway.InstrumentFamily}, port={port}");

      // Loopback-only, no TLS -- this process and its .NET 8 client are expected to run on the
      // same instrument-control PC. Skipping the TLS handshake removes real latency from the
      // hottest path (scan streaming); see Helios.Client for the matching plaintext-H2C switch
      // that has to be set on the .NET 8 side for this to work.
      var server = new Server
      {
        Services =
        {
          InstrumentService.BindService(new InstrumentServiceImpl(gateway)),
          ScanStreamService.BindService(new ScanStreamServiceImpl(gateway)),
          InstrumentControlService.BindService(new InstrumentControlServiceImpl(gateway)),
          SyringePumpService.BindService(new SyringePumpServiceImpl(gateway)),
          AnalogTraceService.BindService(new AnalogTraceServiceImpl(gateway)),
        },
        Ports = { new ServerPort("127.0.0.1", port, ServerCredentials.Insecure) },
      };

      server.Start();

      // Connection is client-driven: whoever calls the Connect RPC (InstrumentServiceImpl.Connect)
      // triggers gateway.ConnectAsync. Don't also connect here -- IInstrumentGateway.ConnectAsync
      // isn't safe to call twice (it would, e.g., spin up a second scan-generation clock/timer on
      // the Simulated backend, or re-subscribe every event twice against Helios.dll).
      Console.WriteLine("Server started. Waiting for a client to call Connect. Press Ctrl+C to stop.");
      var shutdownSignal = new ManualResetEventSlim();
      Console.CancelKeyPress += (_, e) =>
      {
        e.Cancel = true;
        shutdownSignal.Set();
      };
      shutdownSignal.Wait();

      Console.WriteLine("Shutting down...");
      gateway.Dispose();
      server.ShutdownAsync().Wait();
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
