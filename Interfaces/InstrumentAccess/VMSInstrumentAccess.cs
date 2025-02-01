using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAPI.Interfaces.InstrumentAccess.Control;
using UIAPI.Interfaces.InstrumentAccess.MsScanContainer;

namespace UIAPI.Interfaces.InstrumentAccess

{
  class VMSInstrumentAccess : IUInstrumentAccess
  {
    public VMSMsScanContainer MsScanCont { get; }
    public IUControl Control { get; }
    public int CountAnalogChannels { get; }
    public int CountMsDetectors { get; }
    public string[] DetectorClasses { get; }
    public int InstrumentId { get; }
    public string InstrumentName { get; }
    public event EventHandler<AcquisitionErrorsEventArgs> AcquisitionErrorsArrived;
    public event EventHandler<EventArgs> ConnectionChanged;
    public event EventHandler<CCEventArgs> ContactClosureChanged;

    public VMSInstrumentAccess()
    {
      Control = new VMSControl();
      MsScanCont = new VMSMsScanContainer();
      CountAnalogChannels = 1;
      CountMsDetectors = 1;
      DetectorClasses = new string[1] { "dunno" };
      InstrumentId = 1;
      InstrumentName = "VirtualMS Instrument Name";
    }

    public bool Connected()
    {
      return true;
    }
    public IInstMsScanContainer GetMsScanContainer(int msDetectorSet)
    {
      return MsScanCont;
    }

  }
}
