extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes
{
  public interface IHeliosOffMode : IHeliosMode
  {
  }

  internal class HeliosOffModeExploris : IHeliosOffMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }

    public HeliosOffModeExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IOffMode m)
    {
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }
  }

  internal class HeliosOffModeFusion : IHeliosOffMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }

    public HeliosOffModeFusion(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IOffMode m)
    {
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }
  }

  internal class HeliosOffModeVMS : IHeliosOffMode
  {
    public IDictionary<string, string> AdditionalValues { get; } = new Dictionary<string, string>();
    public string[] ValueNames { get; }
    public HeliosOffModeVMS()
    {
      ValueNames = new string[0];
    }
  }
}
