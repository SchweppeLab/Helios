extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes
{
  public interface IHeliosMode
  {
    //
    // Summary:
    //     Get access to a collection of additional values. This will not be used in most
    //     cases.
    IDictionary<string, string> AdditionalValues { get; }

    //
    // Summary:
    //     Get access to the list of all possible names to be used in Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode.AdditionalValues.
    string[] ValueNames { get; }

    //
    // Summary:
    //     This is an informational description of the new mode.
    //
    // Returns:
    //     The returned string gives a short description of the mode.
    new string ToString();
  }

  internal class HeliosMode : IHeliosMode
  {
    public IDictionary<string, string> AdditionalValues { get; } = new Dictionary<string, string>();
    public string[] ValueNames { get; }

    public HeliosMode()
    {
      ValueNames = new string[0];
    }

    public HeliosMode(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode m)
    {
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }

    public HeliosMode(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode m)
    {
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }

  }

  internal class ExplorisMode : exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }
    public ExplorisMode(IHeliosMode m)
    {
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }
  }

  internal class FusionMode: Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode
  {
    public IDictionary<string, string> AdditionalValues { get; }
    public string[] ValueNames { get; }
    public FusionMode(IHeliosMode m)
    {
      AdditionalValues = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvp in m.AdditionalValues) AdditionalValues.Add(kvp);
      ValueNames = new string[m.ValueNames.Length];
      for (int i = 0; i < m.ValueNames.Length; i++) ValueNames[i] = m.ValueNames[i];
    }
  }
}
