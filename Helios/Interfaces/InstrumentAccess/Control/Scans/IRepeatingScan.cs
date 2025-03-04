extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  public interface IRepeatingScan : IHeliosScanDefinition
  {
  }

  internal class ExplorisRepeatingScan : exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IRepeatingScan
  {
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public ExplorisRepeatingScan()
    {
      Values = new Dictionary<string, string>();
    }
    public ExplorisRepeatingScan(IRepeatingScan cs)
    {
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

  internal class FusionRepeatingScan : Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IRepeatingScan
  {
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public FusionRepeatingScan()
    {
      Values = new Dictionary<string, string>();
    }
    public FusionRepeatingScan(IRepeatingScan cs)
    {
      RunningNumber = cs.RunningNumber;
      Values = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in Values)
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

  internal class HeliosRepeatingScan : IRepeatingScan
  {
    public double SingleProcessingDelay { get; set; } = 0;
    public IDictionary<string, string> Values { get; set; }
    public long RunningNumber { get; set; }
    public HeliosRepeatingScan()
    {
      Values = new Dictionary<string, string>();
    }
  }
}
