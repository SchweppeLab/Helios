extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer;

namespace Helios.Interfaces.InstrumentAccess.AnalogTraceContainer
{
  /// <summary>
  /// Frome IAPI: This implementation of EventArgs carries an Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTracePoint.
  /// </summary>
  /// <remarks>
  ///   See Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTraceContainer for an example how this class can be used.<br/>
  ///   An instance of this class will be created by Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTraceContainer.AnalogTracePointArrived.
  /// </remarks>
  public abstract class AnalogTracePointEventArgs : EventArgs
  {
    //
    // Summary:
    //     Get access to the analog trace point that has just arrived from the instrument.
    //     It has replaced already the LastValue in the Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTraceContainer.
    public IAnalogTracePoint TracePoint { get; protected set; }

    //
    // Summary:
    //     Create a new Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs.
    protected AnalogTracePointEventArgs()
    {
    }

    protected AnalogTracePointEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      TracePoint = new HeliosAnalogTracePoint(e.TracePoint);
    }

    protected AnalogTracePointEventArgs(Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      TracePoint = new HeliosAnalogTracePoint(e.TracePoint);
    }
  }

  internal class AnalogTrancePointEventArgsExploris : AnalogTracePointEventArgs
  {
    public AnalogTrancePointEventArgsExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      TracePoint = new HeliosAnalogTracePoint(e.TracePoint);
    }
  }

  internal class AnalogTrancePointEventArgsFusion : AnalogTracePointEventArgs
  {
    public AnalogTrancePointEventArgsFusion(Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      TracePoint = new HeliosAnalogTracePoint(e.TracePoint);
    }
  }
}
