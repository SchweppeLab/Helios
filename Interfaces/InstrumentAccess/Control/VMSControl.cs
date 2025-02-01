using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control
{
  class VMSControl : IUControl
  {
    public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Acquisition { get; }
    public VMSControl()
    {
      //Acquisition = InstAcquisitionFactory.Get(control);
      //Acquisition = new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionFusion(control);
    }
  }
}
