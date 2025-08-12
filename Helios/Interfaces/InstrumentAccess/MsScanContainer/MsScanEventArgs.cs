extern alias exploris;
extern alias fusion;

using System;
using Nova.Data;

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

  internal class VMSMsScanExEventArgs : MsScanEventArgs
  {
    private SpectrumEx spec = null;
    public VMSMsScanExEventArgs(byte[] arr)
    {
      this.spec = new SpectrumEx();
      spec.Deserialize(arr);
    }

    public override IMsScan GetScan()
    {
      IMsScan scan = new HeliosMsScanVMS(spec);
      return scan;
    }
  }

}
