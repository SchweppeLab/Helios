extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAPI.Interfaces.InstrumentAccess;

namespace UIAPI.Interfaces
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
    static public IUInstrumentAccessContainer Create()
    {
      UInstrumentAccessContainerFusion fusion = new UInstrumentAccessContainerFusion();
      if (fusion.Check()) return fusion;

      UInstrumentAccessContainerExploris exploris = new UInstrumentAccessContainerExploris();
      if (exploris.Check()) return exploris;

      UInstrumentAccessContainerVMS vm = new UInstrumentAccessContainerVMS();
      if (vm.Check()) return vm;

      return null;

    }
  }

}
