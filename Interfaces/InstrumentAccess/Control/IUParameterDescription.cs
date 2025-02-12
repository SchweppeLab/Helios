using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control;

namespace UIAPI.Interfaces.InstrumentAccess.Control
{
  /// <summary>
  /// From IAPI: This interface has information about a possible Set argument for an UIAPI.Interfaces.InstrumentAccess.Control.InstrumentValues.IUValue or a property in a Scan parameter set.
  /// <remarks>An instance of this class will be created by UIAPI.Interfaces.InstrumentAccess.Control.InstrumentValues.IUValue.SetParameterDescription 
  /// or by UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans.PossibleParameters</remarks>
  /// </summary>
  public interface IUParameterDescription : IParameterDescription
  {
  }
}
