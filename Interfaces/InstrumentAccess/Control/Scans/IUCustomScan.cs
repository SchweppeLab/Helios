extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Scans
{
  /// <summary>
  /// From IAPI: This scan definition can be placed in the instrument's job queue with individual properties. A custom scan will be executed only once.
  /// </summary>
  /// <remarks>
  /// See UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans for an example how this interface can be used.<br/>
  /// An instance of this class will be created by UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans.CreateCustomScan.
  /// </remarks>
  public interface IUCustomScan : exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.ICustomScan, fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan, IUScanDefinition
  {
    /// <summary>
    /// The instrument will not execute any further custom scan if this property is positive until the delay has expired or a new custom scan has been defined.<br/>
    /// The unit of this property is seconds and possible values are between 0 and 600 inclusively. The default value is 0.
    /// </summary>
    new double SingleProcessingDelay { get; set; }

    /// <summary>
    /// Get access to the value set. Any value not defined will be replaced by the value defined in the default scan.<br/>
    /// Illegal values will be ignored, values out of range will not be accepted.<br/>
    /// The set of possible values can be queried by accessing PossibleParameters in UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans. The key must be an UIAPI.Interfaces.InstrumentAccess.Control.IUParameterDescription Name, the value must match the Selection definition.
    /// </summary>
    new IDictionary<string, string> Values { get; set; }
  }

  internal class UCustomScan : IUCustomScan
  {
    public bool IsPAGCScan { get; set; }
    public long PAGCGroupIndex { get; set; }
    public double SingleProcessingDelay { get; set; } = 0;
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public UCustomScan()
    {
      Values = new Dictionary<string, string>();
    }
  }
}
