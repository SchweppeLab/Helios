extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;

namespace Helios.Interfaces.InstrumentAccess.Control.InstrumentValues
{
  public interface IHeliosInstrumentValues
  {
    //
    // Summary:
    //     Get access to the list of all possible names for Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IInstrumentValues.Get(System.String).
    string[] ValueNames { get; }

    //
    // Summary:
    //     Get access to an instrument value by its name.
    //
    //     Each name has also a numeric representation, see Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IInstrumentValues.Get(System.UInt64).
    //
    //
    // Parameters:
    //   name:
    //     The name of the value
    //
    // Returns:
    //     null is returned for an invalid or unknown name, otherwise the value linked with
    //     the passed name is returned.
    //
    // Remarks:
    //     Accessing the same internal object twice just increments an internal reference,
    //     a further object will not be returned.
    IHeliosValue Get(string name);

    //
    // Summary:
    //     Get access to an instrument value by its number.
    //
    // Parameters:
    //   number:
    //     The number of the value
    //
    // Returns:
    //     The value linked with the passed name is returned. Nodes unknown to the instrument
    //     may return in a dumb value instance.
    //
    // Remarks:
    //     Accessing the same internal object twice just increments an internal reference,
    //     a further object will not be returned.
    IHeliosValue Get(ulong number);
  }

  internal class UInstrumentValues : IHeliosInstrumentValues
  {
    public string[] ValueNames { get; }

    public UInstrumentValues(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
    {
      ValueNames = c.InstrumentValues.ValueNames;
    }

    public UInstrumentValues(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
    {
      ValueNames = c.InstrumentValues.ValueNames;
    }

    public UInstrumentValues()
    {
      ValueNames = new string[0];
    }

    public IHeliosValue Get(string name)
    {
      return null;
    }
    public IHeliosValue Get(ulong number)
    {
      return null;
    }
  }
}
