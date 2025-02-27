using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess
{
  //
  // Summary:
  //     This interface gives access to values helpful to identify a client process or
  //     some kind of caller.
  //
  //     The instrument itself can also be treated as a caller. All values will be null
  //     except for Thermo.Interfaces.InstrumentAccess_V1.IClient.ComputerName, which
  //     will be the name of the instrument. The Thermo.Interfaces.InstrumentAccess_V1.IClient.ProcessId
  //     will be negative in that case.
  public interface IHeliosClient
  {
    //
    // Summary:
    //     Name of the computer to be identified. For an instrument, this will be the instrument
    //     name.
    string ComputerName { get; }

    //
    // Summary:
    //     Name of the account to be identified. For an instrument, this will be null. An
    //     example can be "EMEA\joe.smith".
    string AccountName { get; }

    //
    // Summary:
    //     Name of the user of the account to be identified. For an instrument, this will
    //     be null. An example can be "Joe Smith".
    string UserName { get; }

    //
    // Summary:
    //     Name of the application to be identified. For an instrument, this will be null.
    string ApplicationName { get; }

    //
    // Summary:
    //     ID of the application to be identified. For an instrument, this will be the negative
    //     instrument id.
    int ProcessId { get; }
  }
}
