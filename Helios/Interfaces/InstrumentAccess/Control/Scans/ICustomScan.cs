extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  /// <summary>
  /// From IAPI: This scan definition can be placed in the instrument's job queue with individual properties. A custom scan will be executed only once.
  /// </summary>
  /// <remarks>
  /// See UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans for an example how this interface can be used.<br/>
  /// An instance of this class will be created by UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans.CreateCustomScan.
  /// </remarks>
  public interface ICustomScan : IHeliosScanDefinition
  {
    bool IsPAGCScan { get; set; }

    long PAGCGroupIndex { get; set; }

    /// <summary>
    /// From IAPI: The instrument will not execute any further custom scan if this property is positive until the 
    /// delay has expired or a new custom scan has been defined.<br/>
    /// The unit of this property is seconds and possible values are between 0 and 600 inclusively. The default value is 0.
    /// </summary>
    double SingleProcessingDelay { get; set; }

    //fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan ToFusion();
  }

  internal class ExplorisCustomScan : exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.ICustomScan
  {
    public double SingleProcessingDelay { get; set; } = 0;
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public ExplorisCustomScan()
    {
      Values = new Dictionary<string, string>();
    }
    public ExplorisCustomScan(ICustomScan cs)
    {
      SingleProcessingDelay = cs.SingleProcessingDelay;
      RunningNumber = cs.RunningNumber;
      Values = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in cs.Values)
      {
        if (kvp.Key.Contains("Helios"))
        {
          KeyValuePair<string, string> hvp = HeliosCustomDictionary.GenerateKVPExploris(kvp.Key, kvp.Value);
          if (hvp.Key == null) continue; //should report error instead of ignore it
          if (hvp.Value == null) continue; //should report error instead of ignore it
          Values.Add(hvp);
        }
        else if (HeliosCustomDictionary.EclipseKey(kvp.Key))
        {
          Values.Add(kvp);
        }
      }
    }
  }

  internal class FusionCustomScan : fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan
  {
    public bool IsPAGCScan { get; set; }
    public long PAGCGroupIndex { get; set; }
    public double SingleProcessingDelay { get; set; } = 0;
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public FusionCustomScan()
    {
      Values = new Dictionary<string, string>();
    }
    public FusionCustomScan(ICustomScan cs)
    {
      IsPAGCScan = cs.IsPAGCScan;
      PAGCGroupIndex = cs.PAGCGroupIndex;
      SingleProcessingDelay = cs.SingleProcessingDelay;
      RunningNumber = cs.RunningNumber;
      Values = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in cs.Values)
      {
        if (kvp.Key.Contains("Helios"))
        {
          KeyValuePair<string, string> hvp = HeliosCustomDictionary.GenerateKVPFusion(kvp.Key, kvp.Value);
          if (hvp.Key == null) continue; //should report error instead of ignore it
          if (hvp.Value == null) continue; //should report error instead of ignore it
          Values.Add(hvp);
        }
        else if (HeliosCustomDictionary.FusionKey(kvp.Key))
        {
          Values.Add(kvp);
        }
        //TODO: capture any invalid keys and report in an error log somewhere
      }

    }
  }

  internal class HeliosCustomScan : ICustomScan
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
