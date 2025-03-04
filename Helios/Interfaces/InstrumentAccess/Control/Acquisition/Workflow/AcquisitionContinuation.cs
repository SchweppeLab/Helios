using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow
{
  //
  // Summary:
  //     Defines what shall happen when an acquisition request ended.
  public enum AcquisitionContinuation
  {
    //
    // Summary:
    //     The instrument will continue to run, but no method execution is performed nor
    //     is data written to a raw file.
    StayOn,
    //
    // Summary:
    //     The instrument enters the standby state.
    Standby,
    //
    // Summary:
    //     The instrument enters the off state.
    Off
  }
}
