using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMIDAS.Data.Spectrum;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
  class VMSMSEventArgs : EventArgs, IMSEventArgs
  {
    private Spectrum spec;
    public VMSMSEventArgs(byte[] arr) 
    {
      this.spec = new Spectrum();
      spec.Deserialize(arr);
    }
    public IUMsScan GetScan()
    {
      IUMsScan scan = new VMSScan(spec);
      return scan;
    }
  }
}
