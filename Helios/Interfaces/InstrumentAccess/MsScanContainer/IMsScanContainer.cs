extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{
  public interface IMsScanContainer
  {
    string DetectorClass { get; }
    event EventHandler<MsScanEventArgs> MsScanArrived;

    //
    // Summary:
    //     Get access to the last scan seen in the system. The value can be null initially.
    //
    //
    //     Note that accessing this property forces the consumer to dispose the item as
    //     soon as possible in order to free its shared memory resources.
    //
    // Returns:
    //     The method returns the latest scan the framework is aware off. It may be null.
    //     It is castable to an Thermo.Interfaces.InstrumentAccess_V2.MsScanContainer.IMsScan
    IMsScan GetLastMsScan();
  }

  internal class HeliosMsScanContainerExploris : IMsScanContainer
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
      MsScanArrived?.Invoke(this, new ExplorisMsScanEventArgs(e));
    }

    public IMsScan GetLastMsScan()
    {
      return new HeliosMsScanExploris(cont.GetLastMsScan());
    }

  }

  internal class HeliosMsScanContainerFusion : IMsScanContainer
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
      MsScanArrived?.Invoke(this, new FusionMsScanEventArgs(e));
    }

    public IMsScan GetLastMsScan()
    {
      return new HeliosMsScanFusion(cont.GetLastMsScan());
    }
  }
  
  internal class HeliosMsScanContainerVMS : IMsScanContainer
  {
    public string DetectorClass { get; }
    public event EventHandler<MsScanEventArgs> MsScanArrived;
    private IMsScan LastScan;

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
      LastScan = e.GetScan();
      MsScanArrived?.Invoke(this, e);
    }

    public IMsScan GetLastMsScan()
    {
      return LastScan;
    }
  }

}
