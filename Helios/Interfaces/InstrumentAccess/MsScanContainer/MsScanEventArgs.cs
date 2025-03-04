extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Spectrum;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{

  public abstract class MsScanEventArgs : EventArgs
  {
    protected MsScanEventArgs()
    {
    }

    public abstract IMsScan GetScan();
  }

  internal class ExplorisMsScanEventArgs : MsScanEventArgs
  {
    private exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs eExploris = null;

    public ExplorisMsScanEventArgs(exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
    {
      eExploris = e;
    }

    public override IMsScan GetScan()
    {
      using (StreamWriter writer = new StreamWriter("uiapiLog.txt", true))
      {
        writer.WriteLine("Getting Exploris Scan");
      }
      IMsScan scan = new HeliosMsScanExploris(eExploris.GetScan());
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

    public override IMsScan GetScan()
    {
      IMsScan scan = new HeliosMsScanFusion(eFusion.GetScan());
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

    public override IMsScan GetScan()
    {
      IMsScan scan = new HeliosMsScanVMS(spec);
      return scan;
    }
  }

}
