extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  /// <summary>
  /// From IAPI: This scan definition can be placed in the instrument's job queue with individual properties. A custom scan will be executed only once.
  /// </summary>
  /// <remarks>
  /// See UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans for an example how this interface can be used.<br/>
  /// An instance of this class will be created by UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans.CreateCustomScan.
  /// </remarks>
  public interface IHeliosCustomScan : IHeliosScanDefinition
  {
    bool IsPAGCScan { get; set; }

    long PAGCGroupIndex { get; set; }

    /// <summary>
    /// From IAPI: The instrument will not execute any further custom scan if this property is positive until the 
    /// delay has expired or a new custom scan has been defined.<br/>
    /// The unit of this property is seconds and possible values are between 0 and 600 inclusively. The default value is 0.
    /// </summary>
    double SingleProcessingDelay { get; set; }
  }

  internal class HeliosCustomScan : IHeliosCustomScan
  {
    public bool IsPAGCScan { get; set; }
    public long PAGCGroupIndex { get; set; }
    public double SingleProcessingDelay { get; set; } = 0;
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public HeliosCustomScan()
    {
      Values = new Dictionary<string, string>();
    }
  } 

}
