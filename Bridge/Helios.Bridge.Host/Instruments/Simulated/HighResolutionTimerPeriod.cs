using System.Runtime.InteropServices;

namespace Helios.Bridge.Host.Instruments.Simulated
{
  // Windows' default system clock tick is ~15.6ms, and System.Threading.Timer's granularity is
  // bound by it -- a Timer asked for a 5ms period (200 Hz) will fire in bursty ~15ms clumps
  // without this. winmm's timeBeginPeriod is the standard fix: it asks the OS scheduler for a
  // finer-grained tick for as long as it's held. Only relevant to the simulated backend's own
  // scan-generation timer; the real Fusion/Exploris backend (HeliosInstrumentGateway) is driven by
  // IAPI's own callbacks via Helios.dll, not a timer we control.
  internal static class HighResolutionTimerPeriod
  {
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    private static extern uint TimeBeginPeriod(uint milliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    private static extern uint TimeEndPeriod(uint milliseconds);

    public static void Begin(uint milliseconds = 1) => TimeBeginPeriod(milliseconds);

    public static void End(uint milliseconds = 1) => TimeEndPeriod(milliseconds);
  }
}
