using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Peripherals
{
  public enum SyringePumpStatus
  {
    On,
    Off,
    Error,
    NotConnected,
    LimitReached
  }
}
