extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess;

namespace Helios.Interfaces
{
  //TODO: think about moving this up to the parent namesapace.

  /// <summary>
  /// Factory class that attempts to first connect to a Fusion instrument. Failing
  /// that, it attempts to connect to an Exploris instrument.
  /// </summary>
  static public class InstrumentAccessContainerFactory
  {
    /// <summary>
    /// Get an instance of the instrument.
    /// </summary>
    /// <returns>UIAPI.Interfaces.IInstrument object</returns>
    static public IInstrumentAccessContainer Create()
    {
      HeliosInstrumentAccessContainerFusion fusion = new HeliosInstrumentAccessContainerFusion();
      if (fusion.Check()) return fusion;

      HeliosInstrumentAccessContainerExploris exploris = new HeliosInstrumentAccessContainerExploris();
      if (exploris.Check()) return exploris;

      HeliosInstrumentAccessContainerVMS vm = new HeliosInstrumentAccessContainerVMS();
      if (vm.Check()) return vm;

      return null;

    }
  }

}
