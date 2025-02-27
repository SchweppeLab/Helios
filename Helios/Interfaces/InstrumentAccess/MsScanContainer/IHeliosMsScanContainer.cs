extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{
    public interface IHeliosMsScanContainer
    {
        string DetectorClass { get; }
        event EventHandler<MsScanEventArgs> MsScanArrived;
    }

  internal class HeliosMsScanContainerExploris : IHeliosMsScanContainer
  {
    exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.IExplorisMsScanContainer cont;
    public string DetectorClass { get; }
    public event EventHandler<MsScanEventArgs> MsScanArrived;

    public HeliosMsScanContainerExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess c, int msDetectorSet)
    {
      cont = c.GetMsScanContainer(msDetectorSet);
      cont.MsScanArrived += MsScanArrivedExploris;
      DetectorClass = cont.DetectorClass;
    }

    void MsScanArrivedExploris(object sender, exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
    {
      MsScanEventArgs args = new ExplorisMsScanEventArgs(e);
      OnMsScanArrived(args);
    }

    protected virtual void OnMsScanArrived(MsScanEventArgs e)
    {
      MsScanArrived?.Invoke(this, e);
    }
  }

  internal class HeliosMsScanContainerFusion : IHeliosMsScanContainer
  {
      fusion.Thermo.Interfaces.FusionAccess_V1.MsScanContainer.IFusionMsScanContainer cont;
      public string DetectorClass { get; }
      public event EventHandler<MsScanEventArgs> MsScanArrived;

      public HeliosMsScanContainerFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess c, int msDetectorSet)
      {
          cont = c.GetMsScanContainer(msDetectorSet);
          cont.MsScanArrived += MsScanArrivedFusion;
          DetectorClass = cont.DetectorClass;
      }

    void MsScanArrivedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
    {
      MsScanEventArgs args = new FusionMsScanEventArgs(e);
      OnMsScanArrived(args);
    }

      protected virtual void OnMsScanArrived(MsScanEventArgs e)
      {
          EventHandler<MsScanEventArgs> handler = MsScanArrived;
          if (handler != null)
          {
              handler(this, e);
          }
      }
  }
  
  internal class HeliosMsScanContainerVMS : IHeliosMsScanContainer
  {
    public string DetectorClass { get; }
    public event EventHandler<MsScanEventArgs> MsScanArrived;

    public HeliosMsScanContainerVMS()
    {
      DetectorClass = "VMS DetectorClass";
    }

    public void ReceiveScan(byte[] arr)
    {
      MsScanArrived?.Invoke(this, new VMSMsScanEventArgs(arr));
    }

    protected virtual void OnMsScanArrived(MsScanEventArgs e)
    {
      EventHandler<MsScanEventArgs> handler = MsScanArrived;
      if (handler != null)
      {
        handler(this, e);
      }
    }
  }

}
