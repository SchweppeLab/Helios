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
  /// UIAPI wrapper around AcquisitionOpeningEventArgs. From IAPI Docs:<br/>
  /// This implementation of EventArgs carries information about further scans to be acquired.
  /// </summary>
  public class AcquisitionOpeningEventArgs : EventArgs
  {
    /// <summary>
    /// From IAPI Docs:<br/>
    /// Get access to the information being available at the start of an acquisition. Text representation of numbers will always appear in the independent (US) locale.<br/><br/>
    /// The information is very similar to that of a scan but covers only those information that is available before a scan has been acquired.
    /// </summary>
    public IDictionary<string, string> StartingInformation { get; protected set; }

    /// <summary>
    /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.AOEventArgs from an
    /// Exporis Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs.
    /// </summary>
    /// <param name="e">Exploris IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs</param>
    public AcquisitionOpeningEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
    {
      StartingInformation = e.StartingInformation;
    }

    /// <summary>
    /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.AOEventArgs from an
    /// Fusion Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs.
    /// </summary>
    /// <param name="e">Fusion IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs</param>
    public AcquisitionOpeningEventArgs(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
    {
      StartingInformation = e.StartingInformation;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public AcquisitionOpeningEventArgs()
    {
      StartingInformation = new Dictionary<string, string>();
    }
  }
}
