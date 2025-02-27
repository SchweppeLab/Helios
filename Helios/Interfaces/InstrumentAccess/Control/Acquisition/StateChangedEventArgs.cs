extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition
{
  /// <summary>
  /// Wrapper around StateChangedEventArgs in IAPI.<br/>
  /// This implementation of EventArgs carries an UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IUState.
  /// </summary>
  public abstract class StateChangedEventArgs : EventArgs
  {
    /// <summary>
    /// Get access to the current state of the instrument. 
    /// </summary>
    public IHeliosState State { get; protected set; }

    protected StateChangedEventArgs()
    {
    }

  }

  internal class ExplorisStateChangedEventArgs : StateChangedEventArgs
  {
    /// <summary>
    /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.SCEventArgs from an
    /// Exploris Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs.
    /// </summary>
    /// <param name="e">Exploris IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs</param>
    public ExplorisStateChangedEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
    {
      State = new HeliosStateExploris(e.State);
    }
  }

  internal class FusionStateChangedEventArgs : StateChangedEventArgs
  {
    /// <summary>
    /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.SCEventArgs from a
    /// Fusion Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs.
    /// </summary>
    /// <param name="e">Fusion IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs</param>
    public FusionStateChangedEventArgs(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
    {
      State = new HeliosStateFusion(e.State);
    }
  }

  internal class VMSStateChangedEventArgs : StateChangedEventArgs
  {
    /// <summary>
    /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.SCEventArgs from a
    /// Fusion Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs.
    /// </summary>
    /// <param name="e">Fusion IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs</param>
    public VMSStateChangedEventArgs()
    {
      State = new HeliosStateVMS();
    }
    public VMSStateChangedEventArgs(IHeliosState s)
    {
      State = s;
    }
  }
}
