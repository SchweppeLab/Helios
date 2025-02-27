extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Helios.Interfaces.SpectrumFormat
{

  /// <summary>
  /// From IAPI: This interface defines the functionality to access a particular information source which is usually TuneData, Trailer or StatusLog (wording known to Xcalibur users).
  /// </summary>
  public interface IHeliosInformationSourceAccess
  {
    //
    // Summary:
    //     Access to the items by use of their name. The returned value is sorted according
    //     to its original order and contains also comment entries.
    IEnumerable<string> ItemNames { get; }

    //
    // Summary:
    //     Return whether this particular information source has been made available by
    //     the instrument.
    bool Available { get; }

    //
    // Summary:
    //     Return whether this particular information source block matches the expected
    //     format.
    bool Valid { get; }

    //
    // Summary:
    //     Try to retrieve the value of an item.
    //
    // Parameters:
    //   name:
    //     The name for that the content shall be returned
    //
    //   value:
    //     value of the item
    //
    // Returns:
    //     Returns true if the item has been found, false otherwise.
    bool TryGetRawValue(string name, out object value);

    //
    // Summary:
    //     Try to retrieve the textual representation of an item.
    //
    // Parameters:
    //   name:
    //     The name for that the content shall be returned
    //
    //   value:
    //     textual representation of the value of the item
    //
    // Returns:
    //     Returns true if the item has been found, false otherwise.
    bool TryGetValue(string name, out string value);
  }

  internal class HeliosInformationSourceAccessExploris : IHeliosInformationSourceAccess
  {
    private readonly exploris.Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess isa;

    public IEnumerable<string> ItemNames => isa.ItemNames;
    public bool Available => isa.Available;
    public bool Valid => isa.Valid;

    public HeliosInformationSourceAccessExploris(exploris.Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess a)
    {
      isa = a;
    }

    public bool TryGetRawValue(string name, out object value)
    {
      return isa.TryGetRawValue(name, out value);
    }

    public bool TryGetValue(string name, out string value)
    {
      return isa.TryGetValue(name, out value);
    }
  }

  internal class HeliosInformationSourceAccessFusion : IHeliosInformationSourceAccess
  {
    private readonly Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess isa;

    public IEnumerable<string> ItemNames => isa.ItemNames;
    public bool Available => isa.Available;
    public bool Valid => isa.Valid;

    public HeliosInformationSourceAccessFusion(Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess a)
    {
      isa = a;
    }

    public bool TryGetRawValue(string name, out object value)
    {
      return isa.TryGetRawValue(name, out value);
    }

    public bool TryGetValue(string name, out string value)
    {
      return isa.TryGetValue(name, out value);
    }
  }

  /// <summary>
  /// This class is derived from Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess interface. It stores all trailer contents
  /// in a string,object pair, where the object is probably a string anyway, but leaves open the possibility it isn't. I *think* this
  /// is how IAPI implements its own trailers, but it really is a guess.
  /// </summary>
  internal class HeliosInformationSourceAccess : IHeliosInformationSourceAccess
  {
    /// <summary>
    /// The actual contents of the trailer, where object can possibly hold any data structure, but probably usually holds a string
    /// </summary>
    private Dictionary<string, object> pairs = new Dictionary<string, object>();

    public IEnumerable<string> ItemNames => pairs.Keys;

    public bool Available { get; }

    public bool Valid { get; }

    /// <summary>
    /// Adds a new item to the trailer.
    /// </summary>
    /// <param name="key">: a string identifier</param>
    /// <param name="value">: the item itself</param>
    public void Add(string key, object value)
    {
      pairs.Add(key, value);
    }

    public bool TryGetRawValue(string name, out object value)
    {
      pairs.TryGetValue(name, out value);
      if(value == null) return false;
      return true;
    }


    public bool TryGetValue(string name, out string value)
    {
      value = null;
      object tmp;
      pairs.TryGetValue(name, out tmp);
      if (tmp == null) return false;
      value=tmp.ToString();
      return true;
    }

  }
}
