using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Bridge.Host.Services
{
  // Tracks how many clients currently have a live StreamServiceEvents call open (every
  // Helios.Client connection keeps exactly one such call open for its whole lifetime -- see
  // HeliosClient.PumpServiceEventsAsync), and fires a shutdown callback once that count has gone
  // from >0 back to 0 and stayed there for the idle delay. Never arms on startup with zero
  // clients -- only ConnectionClosed can transition the count to 0, and that can't happen without
  // a prior ConnectionOpened, so "no one has connected yet" and "everyone disconnected" are
  // naturally distinguished without an extra flag.
  //
  // The idle countdown is cancelled and restarted from scratch if a new connection arrives before
  // it elapses, so a quick app restart (e.g. relaunching ScanSpy during testing) doesn't tear down
  // a real instrument connection just to reconnect a few seconds later.
  internal sealed class ConnectionWatchdog
  {
    private readonly TimeSpan _idleShutdownDelay;
    private readonly Action _shutdown;
    private readonly object _sync = new();
    private int _activeConnections;
    private CancellationTokenSource? _idleCts;

    public ConnectionWatchdog(TimeSpan idleShutdownDelay, Action shutdown)
    {
      _idleShutdownDelay = idleShutdownDelay;
      _shutdown = shutdown;
    }

    public void ConnectionOpened()
    {
      lock (_sync)
      {
        _activeConnections++;
        _idleCts?.Cancel();
        _idleCts = null;
      }
    }

    public void ConnectionClosed()
    {
      lock (_sync)
      {
        _activeConnections = Math.Max(0, _activeConnections - 1);
        if (_activeConnections == 0) ArmIdleTimer();
      }
    }

    // Timeout.InfiniteTimeSpan disables auto-shutdown entirely (IdleShutdownSeconds <= 0 in
    // App.config) -- the host then only stops on Ctrl+C, same as before this feature existed.
    private void ArmIdleTimer()
    {
      if (_idleShutdownDelay == Timeout.InfiniteTimeSpan) return;

      var cts = new CancellationTokenSource();
      _idleCts = cts;
      _ = Task.Delay(_idleShutdownDelay, cts.Token).ContinueWith(t =>
      {
        if (t.IsCanceled) return;
        lock (_sync)
        {
          if (_activeConnections == 0) _shutdown();
        }
      }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }
  }
}
