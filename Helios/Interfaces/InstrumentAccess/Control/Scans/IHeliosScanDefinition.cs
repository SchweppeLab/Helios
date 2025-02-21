using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  /// <summary>
  /// From IAPI: This interface covers the functionality to define a custom or repeating scan.
  /// <remarks>This is a base interface of UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUCustomScan and UIAPI.Interfaces.InstrumentAccess.Control.Scans.IURepeatingScan.</remarks>
  /// </summary>
  public interface IHeliosScanDefinition
  {
    /// <summary>
    /// From IAPI: This number will be passed along with the scan job and can be used to identify it later when the acquired scan results arrive. 0 is a reserved value. The default value is 1.<br/>
    /// A change of Values can be identified in UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScan.Trailer.TryGetValue("Access id:", ...).
    /// </summary>
    long RunningNumber { get; set; }

    /// <summary>
    /// From IAPI: Get access to the value set. Any value not defined will be replaced by the value defined in the default scan.<br/>
    /// Illegal values will be ignored, values out of range will not be accepted.<br/>
    /// The set of possible values can be queried by accessing PossibleParameters in Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans.The key must be an Thermo.Interfaces.InstrumentAccess_V1.Control.IParameterDescription Name, the value must match the Selection definition.
    /// </summary>
    IDictionary<string, string> Values { get; }
  }
}
