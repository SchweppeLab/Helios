using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition
{

  //
  // Summary:
  //     This enumeration returns the overall system mode the instrument currently is
  //     in.
  //
  // Remarks:
  //     See Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition for
  //     an example how this enumeration can be used.
  public enum SystemMode
  {
    //
    // Summary:
    //     The instrument description within the PC does not allow a proper connection to
    //     the instrument. This is a very rare condition.
    Malconfigured = -1,
    //
    // Summary:
    //     The instrument is not connected to the service or the current program is not
    //     connected. This state is also entered when the instrument reboots.
    Disconnected = 0,
    //
    // Summary:
    //     The instrument is on. Other values in Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IState
    //     show if an acquisition is ongoing.
    On = 1,
    //
    // Summary:
    //     The instrument is on standby and ready for another operation mode.
    Standby = 2,
    //
    // Summary:
    //     The instrument is off and may need some time for perfect operation if it is set
    //     to on.
    Off = 3,
    //
    // Summary:
    //     The instrument is under remote control, usually by a program of Thermo Fisher.
    RemoteControl = 4,
    //
    // Summary:
    //     The instrument is under direct control, usually by a user or GUI program.
    DirectControl = 5,
    //
    // Summary:
    //     The instrument or its driver entered the maintenance mode.
    Maintenance = 6,
    //
    // Summary:
    //     The instrument performs a calibration.
    Calibration = 100,
    //
    // Summary:
    //     A system evaluation is pending.
    Evaluation = 101,
    //
    // Summary:
    //     The instrument performs a bakeout.
    Bakeout = 102,
    //
    // Summary:
    //     The instrument is optimizing its internal system parameters.
    AutoTune = 103,
    //
    // Summary:
    //     The instrument performs a calibration, but the operation pauses.
    CalibrationPaused = 200,
    //
    // Summary:
    //     A system evaluation is pending, but the evaluation pauses.
    EvaluationPaused = 201,
    //
    // Summary:
    //     The instrument performs a bakeout, but that operation pauses.
    BakeoutPaused = 202,
    //
    // Summary:
    //     The instrument is optimizing its internal system parameters, but the process
    //     pauses.
    AutoTunePaused = 203,
    //
    // Summary:
    //     The instrument is under direct control and acquires data, but storing data has
    //     been paused.
    DirectControlPaused = 305
  }

  public enum InstrumentState
  {
    //
    // Summary:
    //     "Initializing"
    //     Set during boot up or initialization of an acquisition service.
    Initializing,
    //
    // Summary:
    //     "Ready to Download"
    //     The instrument is ready to load a method, but any other mode is also possible.
    //
    //
    //     This state will be used even in case of power saving.
    ReadyToDownload,
    //
    // Summary:
    //     "Downloading"
    //     The instrument downloads a method.
    Downloading,
    //
    // Summary:
    //     "Preparing for Run"
    //     After method downloading the instrument sets up everything to start immediately
    //     without actually starting.
    PreparingForRun,
    //
    // Summary:
    //     "Ready for Run"
    //     The instrument is ready to run, but it still awaits the go.
    ReadyForRun,
    //
    // Summary:
    //     "Waiting for contact closure"
    //     The instrument has been started but waits for a contact closure or another trigger.
    WaitingForContactClosure,
    //
    // Summary:
    //     "Running"
    //     The instrument acquires data. The instrument closes the data file before it leaves
    //     this state.
    Running,
    //
    // Summary:
    //     "Post Run"
    //     The instrument is doing some post cleanup.
    PostRun,
    //
    // Summary:
    //     "Error"
    //     The instrument entered an error condition. The user must start the instrument
    //     setup program to resolve the error.
    Error,
    //
    // Summary:
    //     "Busy"
    //     The instrument is busy somehow, usually due to a maintenance operation.
    Busy,
    //
    // Summary:
    //     "Not Connected"
    //     The instrument software has lost the connection to the instrument.
    NotConnected,
    //
    // Summary:
    //     "Stand By"
    //     From the standby state it should take a very short time to be able to continue
    //     work.
    StandBy,
    //
    // Summary:
    //     "Off"
    //     As much hardware as possible is turned off by software. Some amount of time should
    //     be expected so that operation can be performed with best precision.
    Off,
    //
    // Summary:
    //     "Not Connected" in Xcalibur's run manager.
    //     There exists no connection to the service keeping the connection to the instrument.
    ServerFailed,
    //
    // Summary:
    //     "Lamp Warmup"
    //     Typically used by UV devices to indicate when they are waiting for their lamp
    //     to become ready. This may include lamps of the APPI source.
    LampWarmup,
    //
    // Summary:
    //     "Not Ready"
    //     The instrument is not ready to perform any operation.
    NotReady,
    //
    // Summary:
    //     "Direct Control"
    //     The device is under direct control of a user or GUI program.
    DirectControl
  }

}
