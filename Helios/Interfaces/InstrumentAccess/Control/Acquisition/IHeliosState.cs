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
    /// Wrapper around IAPI IState. From IAPI Docs:<br/>
    /// This interface describes the current system and acquisition state.<br/>
    /// A missing connection can be detected by examining Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState.SystemState.NotConnected 
    /// informs about a missing link between driver and instrument, ServerFailed about a missing link between the current software and the driver.
    /// </summary>
    public interface IHeliosState
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
        InstrumentState SystemState { get; }
        
        /// <summary>
        /// From IAPI Docs:<br/>
        /// The system mode returns the processing mode the instrument is currently in. 
        /// Another state information is Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState.SystemState.
        /// </summary>
        /// <remarks>
        /// Rather than wrap this in a new object, just reuse an existing object in IAPI, and assume
        /// it will remain the same for both Fusion and Exploris
        /// </remarks>
        SystemMode SystemMode { get; }
    }

  internal class HeliosStateExploris : IHeliosState
  {
    public InstrumentState SystemState { get; }
    public SystemMode SystemMode { get; }
    public HeliosStateExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState s)
    {
      SystemState = (InstrumentState)s.SystemState;
      SystemMode = (SystemMode)s.SystemMode;
    }
  }

  internal class HeliosStateFusion : IHeliosState
  {
    public InstrumentState SystemState { get; }
    public SystemMode SystemMode { get; }
    public HeliosStateFusion(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState s)
    {
      SystemState = (InstrumentState)s.SystemState;
      SystemMode = (SystemMode)s.SystemMode;
    }
  }

  /// <summary>
  /// State of the Corona (VMS) backend. Before the pipe connects there is no link to the
  /// service, which the IAPI reports as Disconnected/ServerFailed.
  /// </summary>
  internal class HeliosStateVMS : IHeliosState
  {
    public InstrumentState SystemState { get; set; }
    public SystemMode SystemMode { get; set; }
    public HeliosStateVMS()
    {
      SystemState = InstrumentState.ServerFailed;
      SystemMode = SystemMode.Disconnected;
    }

    public HeliosStateVMS(IHeliosState s)
    {
      SystemState = s.SystemState;
      SystemMode = s.SystemMode;
    }
  }
}
