using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.InstrumentValues
{
  /// <summary>
  /// This interface describes the methods and properties of a value. An IValue is
  /// a representation of a value of various purposes in the instrument. <br/>
  /// The content of the value can be taken from the property Thermo.Interfaces.InstrumentAccess_V1.Control.IReadback.Content. <br/>
  /// A request for a value change will be sent to the instrument by using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IValue.Set(System.String),
  /// but there is no guarantee for acceptance or execution at all. The Thermo.Interfaces.InstrumentAccess_V1.Control.IReadback.Content
  /// or other values can be observed to test for a proper response.
  /// </summary>
  /// <remarks>
  /// See Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IInstrumentValues
  /// for an example how this interface can be used.<br/>
  /// An instance of this class will be created by Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IInstrumentValues.Get(System.UInt64)
  /// or by Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IInstrumentValues.Get(System.String).
  /// </remarks>
  public interface IHeliosValue
  {  
    /// <summary>
    /// Get access to the ID of the value.
    /// </summary>
    ulong Id { get; }
  
    /// <summary>
    /// Get a description about the parameter to the set access to a list of available commands. The list may change if the user role changes. 
    /// Any executed command name will be tested against this list.<br/>
    /// The commands will be null if the instrument is not connected or if the IValue is unknown to the instrument or if 
    /// Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IValue.Set(System.String) cannot be performed for this value.
    /// </summary>
    IHeliosParameterDescription SetParameterDescription { get; }
   
    /// <summary>
    /// Set the content of the value. The command will be verified using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues.IValue.SetParameterDescription.
    /// </summary>
    /// <param name="argument">: null or the argument of the command.</param>
    /// <returns>true if the value change command has been sent to the instrument, false otherwise</returns>
    bool Set(string argument);
  }
}
