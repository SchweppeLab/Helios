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

    fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan ToFusion();
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

    public fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan ToFusion()
    {
      FusionCustomScan customScan = new FusionCustomScan();
      customScan.IsPAGCScan = IsPAGCScan;
      customScan.PAGCGroupIndex = PAGCGroupIndex;
      customScan.SingleProcessingDelay = SingleProcessingDelay;
      customScan.RunningNumber = RunningNumber;
      foreach(KeyValuePair<string, string> kvp in Values)
      {
        if (kvp.Key == "CollisionEnergy") customScan.Values.Add(kvp);
        else if (kvp.Key == "ScanRate") customScan.Values.Add(kvp);
        else if (kvp.Key == "FirstMass") customScan.Values.Add(kvp);
        else if (kvp.Key == "LastMass") customScan.Values.Add(kvp);
        else if (kvp.Key == "Analyzer") customScan.Values.Add(kvp);
        else if (kvp.Key == "ScanType") customScan.Values.Add(kvp);
        else if (kvp.Key == "Polarity") customScan.Values.Add(kvp);
        else if (kvp.Key == "DataType") customScan.Values.Add(kvp);
        else if (kvp.Key == "SrcRFLens") customScan.Values.Add(kvp);
        else if (kvp.Key == "SourceCIDEnergy") customScan.Values.Add(kvp);
        else if (kvp.Key == "ScanRangeMode") customScan.Values.Add(kvp);
        else if (kvp.Key == "SourceCIDScalingFactor") customScan.Values.Add(kvp);
        else if (kvp.Key == "FAIMS CV") customScan.Values.Add(kvp);
        else if (kvp.Key == "FAIMS Voltages") customScan.Values.Add(kvp);
        else if (kvp.Key == "IsolationMode") customScan.Values.Add(kvp);
        else if (kvp.Key == "OrbitrapResolution") customScan.Values.Add(kvp);
        else if (kvp.Key == "IsolationWidth") customScan.Values.Add(kvp);
        else if (kvp.Key == "ActivationType") customScan.Values.Add(kvp);
        else if (kvp.Key == "ChargeStates") customScan.Values.Add(kvp);
        else if (kvp.Key == "ActivationQ") customScan.Values.Add(kvp);
        else if (kvp.Key == "AGCTarget") customScan.Values.Add(kvp);
        else if (kvp.Key == "MaxIT") customScan.Values.Add(kvp);
        else if (kvp.Key == "Microscans") customScan.Values.Add(kvp);
        else if (kvp.Key == "PrecursorMass") customScan.Values.Add(kvp);
        else if (kvp.Key == "ReactionTime") customScan.Values.Add(kvp);
        else if (kvp.Key == "MassRange") customScan.Values.Add(kvp);
        else if (kvp.Key == "MSANeutralLoss") customScan.Values.Add(kvp);
        else if (kvp.Key == "MSXTargets") customScan.Values.Add(kvp);
        else if (kvp.Key == "ScanDescription") customScan.Values.Add(kvp);
      }
      return customScan;
    }
  } 

}
