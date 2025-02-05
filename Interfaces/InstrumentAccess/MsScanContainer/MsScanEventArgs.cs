extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMIDAS.Data.Spectrum;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{

  public class MsScanEventArgs : EventArgs
  {
    private exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs eExploris = null;
    private Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs eFusion = null;
    private Spectrum spec = null;
    public MsScanEventArgs(exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
    {
      eExploris = e;
    }

    public MsScanEventArgs(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
    {
      eFusion = e;
    }

    public MsScanEventArgs(byte[] arr)
    {
      this.spec = new Spectrum();
      spec.Deserialize(arr);
    }

    public IUMsScan GetScan()
    {
      if (eExploris != null)
      {
        IUMsScan scan = new UMsScanExploris(eExploris.GetScan());
        return scan;
      }
      else if (eFusion != null)
      {
        IUMsScan scan = new UMsScanFusion(eFusion.GetScan());
        return scan;
      }
      else
      {
        IUMsScan scan = new UMsScanVMS(spec);
        return scan;
      }
    }
  }

}
