extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
    public interface IUMsScanContainer
    {
        string DetectorClass { get; }
        event EventHandler<MsScanEventArgs> MsScanArrived;
    }

    internal class UMsScanContainerExploris : IUMsScanContainer
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.IExplorisMsScanContainer cont;
        public string DetectorClass { get; }
        public event EventHandler<MsScanEventArgs> MsScanArrived;

        public UMsScanContainerExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess c, int msDetectorSet)
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
            EventHandler<MsScanEventArgs> handler = MsScanArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

  internal class UMsScanContainerFusion : IUMsScanContainer
  {
      fusion.Thermo.Interfaces.FusionAccess_V1.MsScanContainer.IFusionMsScanContainer cont;
      public string DetectorClass { get; }
      public event EventHandler<MsScanEventArgs> MsScanArrived;

      public UMsScanContainerFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess c, int msDetectorSet)
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
  
  internal class UMsScanContainerVMS : IUMsScanContainer
  {
    public string DetectorClass { get; }
    public event EventHandler<MsScanEventArgs> MsScanArrived;

    public UMsScanContainerVMS()
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
