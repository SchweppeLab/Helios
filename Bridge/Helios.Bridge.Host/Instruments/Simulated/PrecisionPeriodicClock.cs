using System;
using System.Diagnostics;
using System.Threading;

namespace Helios.Bridge.Host.Instruments.Simulated
{
  // System.Threading.Timer is bound to the OS system-clock tick (~15.6ms by default on Windows),
  // and periods below that fire in bursty clumps rather than smoothly -- worse still, background
  // (non-foreground) processes can have that resolution throttled further regardless of
  // timeBeginPeriod. For a synthetic load generator that's supposed to hit precise rates well
  // into the hundreds of Hz, a dedicated thread doing a hybrid sleep/spin wait against a
  // Stopwatch is the standard fix (the same technique real-time audio/game engines use) --
  // it doesn't depend on OS timer resolution at all.
  internal sealed class PrecisionPeriodicClock : IDisposable
  {
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts = new();

    public PrecisionPeriodicClock(TimeSpan period, Action tick)
    {
      _thread = new Thread(() => Run(period, tick))
      {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal,
        Name = "Helios.Bridge.Simulated.ScanClock",
      };
      _thread.Start();
    }

    private void Run(TimeSpan period, Action tick)
    {
      var stopwatch = Stopwatch.StartNew();
      long periodTicks = (long)(period.TotalSeconds * Stopwatch.Frequency);
      long nextDueTicks = 0;
      var token = _cts.Token;

      while (!token.IsCancellationRequested)
      {
        long remaining = nextDueTicks - stopwatch.ElapsedTicks;
        if (remaining > 0)
        {
          // Sleep off the bulk of the wait (cheap, imprecise) and spin the last ~1-2ms (expensive,
          // precise) so this doesn't just become a busy-loop for slower configured rates.
          double remainingMs = remaining * 1000.0 / Stopwatch.Frequency;
          if (remainingMs > 2) Thread.Sleep(1);
          else Thread.SpinWait(50);
          continue;
        }

        tick();
        nextDueTicks += periodTicks;

        // If we've fallen far behind (e.g. GC pause), resync instead of firing a burst of
        // catch-up ticks.
        if (stopwatch.ElapsedTicks - nextDueTicks > periodTicks * 4) nextDueTicks = stopwatch.ElapsedTicks;
      }
    }

    public void Dispose()
    {
      _cts.Cancel();
      _thread.Join(TimeSpan.FromSeconds(2));
      _cts.Dispose();
    }
  }
}
