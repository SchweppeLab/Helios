extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
  class VMSMsScanContainer : IInstMsScanContainer
  {
    public string DetectorClass { get; }
    public event EventHandler<IMSEventArgs> MsScanArrived;

    public VMSMsScanContainer()
    {
      DetectorClass = "VMS DetectorClass";
    }

    public void ReceiveScan(byte[] arr)
    {
      MsScanArrived?.Invoke(this, new VMSMSEventArgs(arr));
    }

    protected virtual void OnMsScanArrived(IMSEventArgs e)
    {
      EventHandler<IMSEventArgs> handler = MsScanArrived;
      if (handler != null)
      {
        handler(this, e);
      }
    }
  }
}
