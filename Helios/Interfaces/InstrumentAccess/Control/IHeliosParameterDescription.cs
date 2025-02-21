using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control
{
  /// <summary>
  /// From IAPI: This interface has information about a possible Set argument for an UIAPI.Interfaces.InstrumentAccess.Control.InstrumentValues.IUValue or a property in a Scan parameter set.
  /// <remarks>An instance of this class will be created by UIAPI.Interfaces.InstrumentAccess.Control.InstrumentValues.IUValue.SetParameterDescription 
  /// or by UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans.PossibleParameters</remarks>
  /// </summary>
  public interface IHeliosParameterDescription
  {  
    /// <summary>
    /// From IAPI: This is the name of the command/property this parameter description belongs to.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// From IAPI: The selection is a little bit complicated. It can have these values: <br/>
    /// value:                       example/description<br/>
    /// empty:                       This empty string is allowed and doesn't allow the user any selection<br/> 
    /// string:                      This special value (verbatim "string") allows the user to enter an arbitrary string <br/>
    /// num1-num2:                   Integer selection between num1 and num2 inclusively<br/>
    /// num1.frac-num2.frac:         Floating point selection between num1.frac and num2.frac inclusively,frac may be "0"<br/>
    /// selection1,selection2,...:   lets the user select one of the shown values
    /// </summary>
    string Selection { get; }
  
    /// <summary>
    /// This value will be the default value for the argument or the empty string if it is unknown.
    /// </summary>
    string DefaultValue { get; }
  
    /// <summary>
    /// This returns the empty string or some help about the command/property.
    /// </summary>
    string Help { get; }
  }

  internal class HeliosParameterDescription : IHeliosParameterDescription
  {
    public string Name { get; }
    public string Selection { get; }
    public string DefaultValue { get; }
    public string Help { get; }
    public HeliosParameterDescription(string name, string selection, string defaultValue, string help)
    {
      Name = name;
      Selection = selection;
      DefaultValue = defaultValue;
      Help = help;
    }
  }
}
