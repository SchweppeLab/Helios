extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Acquisition
{
    /// <summary>
    /// Wrapper around IAPI IState. From IAPI Docs:<br/>
    /// This interface describes the current system and acquisition state.<br/>
    /// A missing connection can be detected by examining Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState.SystemState.NotConnected 
    /// informs about a missing link between driver and instrument, ServerFailed about a missing link between the current software and the driver.
    /// </summary>
    public interface IUState
    {
        /// <summary>
        /// From IAPI Docs:<br/>
        /// The system state returns the instrument state with respect to data acquisition. 
        /// Another state information is Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState.SystemMode.
        /// </summary>
        /// <remarks>
        /// Rather than wrap this in a new object, just reuse an existing object in IAPI, and assume
        /// it will remain the same for both Fusion and Exploris
        /// </remarks>
        Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.InstrumentState SystemState { get; }
        
        /// <summary>
        /// From IAPI Docs:<br/>
        /// The system mode returns the processing mode the instrument is currently in. 
        /// Another state information is Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState.SystemState.
        /// </summary>
        /// <remarks>
        /// Rather than wrap this in a new object, just reuse an existing object in IAPI, and assume
        /// it will remain the same for both Fusion and Exploris
        /// </remarks>
        Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode SystemMode { get; }
    }

  internal class UStateExploris : IUState
  {
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.InstrumentState SystemState { get; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode SystemMode { get; }
    public UStateExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState s)
    {
      SystemState = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.InstrumentState)s.SystemState;
      SystemMode = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode)s.SystemMode;
    }
  }

  internal class UStateFusion : IUState
  {
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.InstrumentState SystemState { get; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode SystemMode { get; }
    public UStateFusion(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState s)
    {
      SystemState = s.SystemState;
      SystemMode = s.SystemMode;
    }
  }

  internal class UStateVMS : IUState
  {
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.InstrumentState SystemState { get; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.SystemMode SystemMode { get; }
    public UStateVMS(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState s)
    {
      SystemState = s.SystemState;
      SystemMode = s.SystemMode;
    }
  }
}
