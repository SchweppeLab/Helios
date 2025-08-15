using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
      AddHeader("IonizationMode", "IonizationMode", "IonizationMode", "IonizationMode", "Typically electrospray (ESI) or nanospray (NSI)");
      AddHeader("ScanRate", null, "ScanRate", "ScanRate", null);
      AddHeader("ScanMode", "ScanMode", "ScanMode", "ScanMode", "The scan mode that was set.");
      AddHeader("TIC", "TIC", "TIC", "TIC", "Total Ion Current of the spectrum.");
      AddHeader("BasePeakIntensity", "BasePeakIntensity", "BasePeakIntensity", "BasePeakIntensity", "The intensity value of the most abundant scan peak.");
      AddHeader("BasePeakMass", "BasePeakMass", "BasePeakMass", "BasePeakMass", "The m/z value of the most abundant scan peak.");
      AddHeader("CycleNumber", "CycleNumber", "CycleNumber", "CycleNumber", null);
      AddHeader("Polarity", "Polarity", "Polarity", "Polarity", "Typically positive (or 0) or negative (1).");
      AddHeader("Microscans", "Microscans", "Microscans", "Microscans", "Number of microscans.");
      AddHeader("InjectTime", "InjectTime", "InjectTime", "InjectTime", "The ion injection time in milliseconds.");
      AddHeader("ScanData", "ScanData", "ScanData", "ScanData", "The peak representation, typically profile or centroid.");
      AddHeader("Segments", null, "Segments", null, null);
      AddHeader("Monoisotopic", null, "Monoisotopic", "Monoisotopic", null);
      AddHeader("MasterScan", null, "MasterScan", "MasterScan", "The parent scan number of");
      AddHeader("FirstMass", "LowMass", "FirstMass", "FirstMass", "The lower boundary m/z in the scan.");
      AddHeader("LastMass", "HighMass", "LastMass", "LastMass", "The upper boundary m/z of the scan.");
      AddHeader("Checksum", null, "Checksum", "Checksum", "A data validation value.");
      AddHeader("MSOrder", "MSOrder", "MSOrder", "MSOrder", "The MS level of the scan, i.e., whether MS1 or MS2, etc.");
      AddHeader("Average", null, "Average", "Average", null);
      AddHeader("Dependent", "Dependent", "Dependent", "Dependent", null);
      AddHeader("MSX", null, "MSX", "MSX", "Indicates if multiple precursors were combined in a single scan.");
      AddHeader("SourceFragmentation", "SourceFragmentation", "SourceFragmentation", "SourceFragmentation", null);
      AddHeader("SourceFragmentationEnergy", null, "SourceFragmentationEnergy", "SourceFragmentationEnergy", null);
      AddHeader("RawOvFtT", null, "RawOvFtT", "RawOvFtT", null);
      AddHeader("Injection t0", null, "Injection t0", null, null);
      AddHeader("CollisionEnergy[0]", null, "CollisionEnergy[0]", null, null);

      AddTrailer("Access ID", "Access Id:", "Access ID", "Access ID", "The identification number of an IAPI custom scan.");
      AddTrailer("Charge State", "Charge State", "Charge State", "Charge State", null);
      AddTrailer("FAIMS Voltage On", "FAIMS Voltage On", "FAIMS Voltage On", "FAIMS Voltage On", "Indicates if FAIMS was used.");
      AddTrailer("FAIMS CV", "FAIMS CV", "FAIMS CV", "FAIMS CV", "The compensation voltage for FAIMS.");
      AddTrailer("Monoisotopic M/Z", "Monoisotopic M/Z", "Monoisotopic M/Z", "Monoisotopic M/Z", null);
      AddTrailer("Scan Description", "Scan Description", "Scan Description", "Scan Description", null);
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

    public static List<HeliosTerm> GetHeliosHeaders()
    {
      List<HeliosTerm> heliosTerms = new List<HeliosTerm>();
      foreach(HeliosTerm h in HeliosLexiconHeader)
      {
        heliosTerms.Add(h);
      }
      return heliosTerms;
    }
  }
}
