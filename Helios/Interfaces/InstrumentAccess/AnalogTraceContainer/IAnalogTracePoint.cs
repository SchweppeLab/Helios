extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.AnalogTraceContainer
{
  public interface IAnalogTracePoint
  {
    //
    // Summary:
    //     The value of the analog detector.
    double Value { get; }

    //
    // Summary:
    //     The time difference between acquisition start and the arrival of the analog value.
    TimeSpan Occurrence { get; }
  }

  internal class HeliosAnalogTracePoint : IAnalogTracePoint
  {
    public double Value { get; } = 0;
    public TimeSpan Occurrence { get; }
    public HeliosAnalogTracePoint(exploris.Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTracePoint a)
    {
      Value = a.Value;
      Occurrence = a.Occurrence;
    }
    public HeliosAnalogTracePoint(Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTracePoint a)
    {
      Value = a.Value;
      Occurrence = a.Occurrence;
    }
  }

}
