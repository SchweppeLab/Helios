using Helios.Client;

namespace Helios.Client.Demo;

// End-to-end showcase for the Helios gRPC bridge: connects to a locally running
// Helios.Bridge.Host (defaults to its Simulated backend, so this works with no hardware or
// license), streams scans while tracking round-trip latency, submits a demo custom scan, and
// toggles acquisition on/off while printing the resulting state-change events.
internal static class Program
{
  private static async Task Main()
  {
    Console.WriteLine("Helios.Client.Demo -- connecting to Helios.Bridge.Host at 127.0.0.1:50100...");

    await using var access = await HeliosClient.ConnectAsync();
    Console.WriteLine($"Connected: family={access.Family}, instrument='{access.InstrumentName}' (id={access.InstrumentId}), " +
                       $"msDetectors={access.CountMsDetectors}, analogChannels={access.CountAnalogChannels}, hasSyringePump={access.Control.SyringePumpControl is not null}");

    using var shutdown = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
      e.Cancel = true;
      shutdown.Cancel();
    };

    var scanStreamTask = StreamScansWithLatencyStatsAsync(access, shutdown.Token);
    PrintAcquisitionState(access);

    await Task.Delay(TimeSpan.FromSeconds(1), shutdown.Token).ContinueWith(_ => { });
    await SubmitDemoCustomScanAsync(access);

    await Task.Delay(TimeSpan.FromSeconds(1), shutdown.Token).ContinueWith(_ => { });
    Console.WriteLine("Switching acquisition mode ON...");
    access.Control.Acquisition.SetMode(on: true);

    Console.WriteLine("Running -- press Ctrl+C to stop.");
    try
    {
      await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
    }
    catch (OperationCanceledException)
    {
      // expected on Ctrl+C
    }

    Console.WriteLine("Switching acquisition mode OFF and shutting down...");
    access.Control.Acquisition.SetMode(on: false);

    await scanStreamTask;
  }

  private static async Task SubmitDemoCustomScanAsync(IInstrumentAccess access)
  {
    var scan = new CustomScan { RunningNumber = 1 };
    scan.Values["FirstMass"] = "300";
    scan.Values["LastMass"] = "1500";
    scan.Values["ScanType"] = "Full";
    scan.Values["Analyzer"] = "Orbitrap";
    scan.Values["OrbitrapResolution"] = "120000";

    bool accepted = await access.Control.Scans.SubmitCustomScanAsync(scan);
    Console.WriteLine($"Submitted demo custom scan -- accepted={accepted}");
  }

  // Unlike the RPC-streamed events elsewhere in this demo, IAcquisition.StateChanged is a plain
  // .NET event backed by a cache the client keeps current in the background (see
  // GrpcAcquisition) -- no await foreach needed to observe it.
  private static void PrintAcquisitionState(IInstrumentAccess access)
  {
    access.Control.Acquisition.StateChanged += (_, e) =>
      Console.WriteLine($"acquisition state -> mode={e.State.Mode}, state={e.State.State}");
    access.Control.Acquisition.AcquisitionStreamOpening += (_, _) =>
      Console.WriteLine("acquisition stream opening");
    access.Control.Acquisition.AcquisitionStreamClosing += (_, _) =>
      Console.WriteLine("acquisition stream closing");
  }

  private static async Task StreamScansWithLatencyStatsAsync(IInstrumentAccess access, CancellationToken cancellationToken)
  {
    var stats = new LatencyTracker();
    using var reportTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    var reportTask = ReportLoopAsync(stats, reportTimer, cancellationToken);

    var tcs = new TaskCompletionSource();
    using var registration = cancellationToken.Register(() => tcs.TrySetResult());

    var container = access.GetMsScanContainer(0);
    container.MsScanArrived += (_, e) =>
    {
      double latencyMs = (DateTimeOffset.UtcNow - e.Scan.ArrivalTimeUtc).TotalMilliseconds;
      stats.Record(latencyMs);
    };

    await tcs.Task;

    reportTimer.Dispose();
    await reportTask;
  }

  private static async Task ReportLoopAsync(LatencyTracker stats, PeriodicTimer timer, CancellationToken cancellationToken)
  {
    try
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        Console.WriteLine(stats.SnapshotAndReset());
      }
    }
    catch (OperationCanceledException)
    {
      // expected on shutdown
    }
    catch (ObjectDisposedException)
    {
      // timer disposed while awaiting the next tick
    }
  }

  // Rolling one-second window: count, min/avg/max latency. Plain accumulators, no LINQ -- this
  // runs once per scan on a path we're specifically trying to keep fast.
  private sealed class LatencyTracker
  {
    private int _count;
    private double _sum;
    private double _min = double.MaxValue;
    private double _max = double.MinValue;
    private readonly object _sync = new();

    public void Record(double latencyMs)
    {
      lock (_sync)
      {
        _count++;
        _sum += latencyMs;
        if (latencyMs < _min) _min = latencyMs;
        if (latencyMs > _max) _max = latencyMs;
      }
    }

    public string SnapshotAndReset()
    {
      lock (_sync)
      {
        if (_count == 0) return "scans/sec=0";

        double avg = _sum / _count;
        string result = $"scans/sec={_count}, latency(ms) min={_min:F2} avg={avg:F2} max={_max:F2}";
        _count = 0;
        _sum = 0;
        _min = double.MaxValue;
        _max = double.MinValue;
        return result;
      }
    }
  }
}
