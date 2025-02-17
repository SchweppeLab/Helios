extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Spectrum;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{

  public abstract class MsScanEventArgs : EventArgs
  {
    protected MsScanEventArgs()
    {
    }

    public abstract IUMsScan GetScan();
  }

  internal class ExplorisMsScanEventArgs : MsScanEventArgs
  {
    private exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs eExploris = null;

    public ExplorisMsScanEventArgs(exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
    {
      eExploris = e;
    }

    public override IUMsScan GetScan()
    {
      IUMsScan scan = new UMsScanExploris(eExploris.GetScan());
      return scan;
    }
  }

  internal class FusionMsScanEventArgs : MsScanEventArgs
  {
    private Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs eFusion = null;

    public FusionMsScanEventArgs(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
    {
      eFusion = e;
    }

    public override IUMsScan GetScan()
    {
      IUMsScan scan = new UMsScanFusion(eFusion.GetScan());
      return scan;
    }
  }

  internal class VMSMsScanEventArgs : MsScanEventArgs
  {
    private Spectrum spec = null;
    public VMSMsScanEventArgs(byte[] arr)
    {
      this.spec = new Spectrum();
      spec.Deserialize(arr);
    }

    public override IUMsScan GetScan()
    {
      IUMsScan scan = new UMsScanVMS(spec);
      return scan;
    }
  }

}
