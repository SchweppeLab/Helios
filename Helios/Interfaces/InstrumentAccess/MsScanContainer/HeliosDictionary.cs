using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{

  internal class HeliosTerm
  {
    public readonly string HeliosID;
    public readonly string ExplorisTerm;
    public readonly string FusionTerm;
    public readonly string VMSTerm;
    public readonly string Description;
    public HeliosTerm(string heliosID, string explorisTerm, string fusionTerm, string vmsTerm, string description)
    {
      HeliosID = heliosID;
      ExplorisTerm = explorisTerm;
      FusionTerm = fusionTerm;
      VMSTerm = vmsTerm;
      Description = description;
    }
  }

  internal static class HeliosDictionary
  {
    //key is whatever term is used by any instrument, the value is the universal term used in Helios
    private static Dictionary<string,string> HeaderSynonyms = new Dictionary<string,string>();
    private static Dictionary<string,string> TrailerSynonyms = new Dictionary<string,string>();

    //key is the universal term used in Helios, the value is the index to the lexicon
    private static Dictionary<string, int> HeaderHelios = new Dictionary<string, int>();
    private static Dictionary<string, int> TrailerHelios = new Dictionary<string, int>();

    //Note that headers and trailers can have the same identifiers, so two lexicons are necessary
    private static List<HeliosTerm> HeliosLexiconHeader = new List<HeliosTerm>();
    private static List<HeliosTerm> HeliosLexiconTrailer = new List<HeliosTerm>();

    static HeliosDictionary() {
      AddHeader("Scan", "Scan", "Scan", "Scan", "The scan number of the spectrum.");
      AddHeader("StartTime", "StartTime", "StartTime", "StartTime", "The run time when the scan was acquired.");
      AddHeader("MassAnalyzer", "MassAnalyzer", "MassAnalyzer", "MassAnalyzer", "The mass analyzer used to acquire the scan.");
      AddHeader("IonizationMode", "IonizationMode", "IonizationMode", "IonizationMode", null);
      AddHeader("ScanRate", null, "ScanRate", "ScanRate", null);
      AddHeader("ScanMode", "ScanMode", "ScanMode", "ScanMode", null);
      AddHeader("TIC", "TIC", "TIC", "TIC", "Total Ion Current of the spectrum.");
      AddHeader("BasePeakIntensity", "BasePeakIntensity", "BasePeakIntensity", "BasePeakIntensity", null);
      AddHeader("BasePeakMass", "BasePeakMass", "BasePeakMass", "BasePeakMass", null);
      AddHeader("CycleNumber", "CycleNumber", "CycleNumber", "CycleNumber", null);
      AddHeader("Polarity", "Polarity", "Polarity", "Polarity", null);
      AddHeader("Microscans", "Microscans", "Microscans", "Microscans", null);
      AddHeader("InjectTime", "InjectTime", "InjectTime", "InjectTime", null);
      AddHeader("ScanData", "ScanData", "ScanData", "ScanData", null);
      AddHeader("Segments", null, "Segments", null, null);
      AddHeader("Monoisotopic", null, "Monoisotopic", "Monoisotopic", null);
      AddHeader("MasterScan", null, "MasterScan", "MasterScan", null);
      AddHeader("FirstMass", "LowMass", "FirstMass", "FirstMass", null);
      AddHeader("LastMass", "HighMass", "LastMass", "LastMass", null);
      AddHeader("Checksum", null, "Checksum", "Checksum", null);
      AddHeader("MSOrder", "MSOrder", "MSOrder", "MSOrder", null);
      AddHeader("Average", null, "Average", "Average", null);
      AddHeader("Dependent", "Dependent", "Dependent", "Dependent", null);
      AddHeader("MSX", null, "MSX", "MSX", null);
      AddHeader("SourceFragmentation", "SourceFragmentation", "SourceFragmentation", "SourceFragmentation", null);
      AddHeader("SourceFragmentationEnergy", null, "SourceFragmentationEnergy", "SourceFragmentationEnergy", null);
      AddHeader("RawOvFtT", null, "RawOvFtT", "RawOvFtT", null);
      AddHeader("Injection t0", null, "Injection t0", null, null);

      AddTrailer("Access ID", "Access Id:", "Access ID", "Access ID", null);
    }

    private static void AddHeader(string heliosID, string explorisTerm, string fusionTerm, string vmsTerm, string definition)
    {

      //Sanity check - don't duplicate helios ids
      if (HeaderHelios.ContainsKey(heliosID))
      {
        //Should print some warning message, I suppose...
        return;
      }

      //Add universal helios id to our lookup dictionary
      HeaderHelios.Add(heliosID, HeliosLexiconHeader.Count);

      //Add universal helios id and the instrument terms to our lexicon
      if (definition == null) definition = "(no description)";
      HeliosLexiconHeader.Add(new HeliosTerm(heliosID, explorisTerm, fusionTerm, vmsTerm, definition));

      //Add all unique terms to our list of synonyms pointing to the heliosID
      string tmp;
      if(explorisTerm!=null && !HeaderSynonyms.TryGetValue(explorisTerm, out tmp))
      {
        HeaderSynonyms.Add(explorisTerm, heliosID);
      }
      if (fusionTerm != null && !HeaderSynonyms.TryGetValue(fusionTerm, out tmp))
      {
        HeaderSynonyms.Add(fusionTerm, heliosID);
      }
      if (vmsTerm != null && !HeaderSynonyms.TryGetValue(vmsTerm, out tmp))
      {
        HeaderSynonyms.Add(vmsTerm, heliosID);
      }
    }

    private static void AddTrailer(string heliosID, string explorisTerm, string fusionTerm, string vmsTerm, string definition)
    {
      //Sanity check - don't duplicate helios ids
      if (TrailerHelios.ContainsKey(heliosID))
      {
        //Should print some warning message, I suppose...
        return;
      }

      //Add universal helios id to our lookup dictionary
      TrailerHelios.Add(heliosID, HeliosLexiconTrailer.Count);

      //Add universal helios id and the instrument terms to our lexicon
      if (definition == null) definition = "(no description)";
      HeliosLexiconTrailer.Add(new HeliosTerm(heliosID, explorisTerm, fusionTerm, vmsTerm, definition));

      //Add all unique terms to our list of synonyms pointing to the heliosID
      string tmp;
      if (explorisTerm != null && !TrailerSynonyms.TryGetValue(explorisTerm, out tmp))
      {
        TrailerSynonyms.Add(explorisTerm, heliosID);
      }
      if (fusionTerm != null && !TrailerSynonyms.TryGetValue(fusionTerm, out tmp))
      {
        TrailerSynonyms.Add(fusionTerm, heliosID);
      }
      if (vmsTerm != null && !TrailerSynonyms.TryGetValue(vmsTerm, out tmp))
      {
        TrailerSynonyms.Add(vmsTerm, heliosID);
      }
    }

    public static string GetHeader(string id, ScanSource source)
    {
      string heliosID;
      if(!HeaderSynonyms.TryGetValue(id, out heliosID))
      {
        return null;
      }

      int index;
      HeaderHelios.TryGetValue(heliosID, out index);

      switch (source)
      {
        case ScanSource.Exploris:
          return HeliosLexiconHeader[index].ExplorisTerm;
        case ScanSource.Fusion:
          return HeliosLexiconHeader[index].FusionTerm;
        case ScanSource.VMS:
          return HeliosLexiconHeader[index].VMSTerm;
        default:
          return null;
      }
    }

    public static string GetTrailer(string id, ScanSource source)
    {
      string heliosID;
      if (!TrailerSynonyms.TryGetValue(id, out heliosID))
      {
        return null;
      }

      int index;
      TrailerHelios.TryGetValue(heliosID, out index);

      switch (source)
      {
        case ScanSource.Exploris:
          return HeliosLexiconTrailer[index].ExplorisTerm;
        case ScanSource.Fusion:
          return HeliosLexiconTrailer[index].FusionTerm;
        case ScanSource.VMS:
          return HeliosLexiconTrailer[index].VMSTerm;
        default:
          return null;
      }
    }
  }
}
