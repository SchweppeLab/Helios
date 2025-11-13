extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes
{
  public interface IHeliosOnMode:IHeliosMode
  {
  }

  internal class HeliosOnModeExploris : IHeliosOnMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }

    public HeliosOnModeExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IOnMode m) { 
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for(int i = 0; i < m.ValueNames.Length;i++) ValueNames[i]=m.ValueNames[i];
    }
  }

  internal class HeliosOnModeFusion : IHeliosOnMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }

    public HeliosOnModeFusion(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IOnMode m)
    {
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }
  }

  internal class HeliosOnModeVMS : IHeliosOnMode
  {
    public IDictionary<string, string> AdditionalValues { get; } = new Dictionary<string, string>();
    public string[] ValueNames { get; }
    public HeliosOnModeVMS()
    {
      ValueNames = new string[0];
    }
  }
}
