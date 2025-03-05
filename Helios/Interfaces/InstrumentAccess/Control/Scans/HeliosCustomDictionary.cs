using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{

  internal class HeliosCustomTerm
  {
    public readonly string HeliosID;
    public readonly string ExplorisSingle;
    public readonly string ExplorisMulti;
    public readonly string FusionTerm;

    public HeliosCustomTerm(string heliosID, string explorisSingle, string explorisMulti, string fusionTerm)
    {
      HeliosID = heliosID;
      ExplorisSingle = explorisSingle;
      ExplorisMulti = explorisMulti;
      FusionTerm = fusionTerm;
    }
  }
  internal class ExplorisCustomTerm
  {
    public readonly string Single;
    public readonly string Multi;

    public ExplorisCustomTerm(string single, string multi)
    {
      Single = single;
      Multi = multi;
    }
  }

  internal static class HeliosCustomDictionary
  {
    /// <summary>
    /// The list of valid Exploris custom scan params. This will be populated upon connection to an Exploris.
    /// </summary>
    private static Dictionary<string,int> ExplorisParams = new Dictionary<string,int>();

    /// <summary>
    /// A dictionary of Helios IDs (keys) and indexes to the ExplorisTerm (values).
    /// </summary>
    private static Dictionary<string, int> ExplorisLexicon = new Dictionary<string, int>();

    /// <summary>
    /// Exploris custom scan params that map to a Helios ID. Some params have different terms for a single value or for
    /// multiple values.
    /// </summary>
    private static List<ExplorisCustomTerm> ExplorisTerm = new List<ExplorisCustomTerm>();


    /// <summary>
    /// The list of valid Fusion custom scan params. This will be populated upon connection to an Exploris.
    /// </summary>
    private static Dictionary<string, int> FusionParams = new Dictionary<string, int>();

    /// <summary>
    /// A dictionary of Helios IDs (keys) and the FusionTerm (value)
    /// </summary>
    private static Dictionary<string, string> FusionLexicon = new Dictionary<string, string>();

    /// <summary>
    /// A list of all the Helios IDs that can be used in custom scans.
    /// </summary>
    public static List<HeliosCustomTerm> HeliosLexicon = new List<HeliosCustomTerm>();

    static HeliosCustomDictionary()
    {
      Add("HeliosNCE", "NCE", "MsxInjectNCEs", "CollisionEnergy");
      Add("HeliosMaxIT", "MaxIT", "MsxInjectMaxITs", "MaxIT");
      Add("HeliosResolution", "Resolution", null, "OrbitrapResolution");
    }

    public static KeyValuePair<string, string> GenerateKVPExploris(string key, string value)
    {
      int eIndex;
      if (!ExplorisLexicon.TryGetValue(key, out eIndex))
      {
        return new KeyValuePair<string, string>(null, null);
      }

      string eKey;
      string eValue;
      if (value.Contains(';'))
      {
        eKey = ExplorisTerm[eIndex].Multi;
        eValue = "[" + value.Replace(";", ", ") + "]";
      }
      else
      {
        eKey = ExplorisTerm[eIndex].Single;
        eValue = value;
      }
      return new KeyValuePair<string, string>(eKey, eValue);
    }

    public static KeyValuePair<string, string> GenerateKVPFusion(string key, string value)
    {
      string fKey;
      if(!FusionLexicon.TryGetValue(key, out fKey))
      {
        return new KeyValuePair<string,string>(null,null);
      }
      return new KeyValuePair<string, string>(fKey, value.Replace(';',','));
    }

    private static void Add(string heliosID, string explorisSingle, string explorisMulti, string fusionTerm)
    {
      ExplorisLexicon.Add(heliosID, ExplorisTerm.Count);
      ExplorisTerm.Add(new ExplorisCustomTerm(explorisSingle, explorisMulti));

      FusionLexicon.Add(heliosID,fusionTerm);

      //Not really using this well
      HeliosLexicon.Add(new HeliosCustomTerm(heliosID, explorisSingle, explorisMulti, fusionTerm));
    }

    public static void AddExplorisParam(string param)
    {
      if (ExplorisParams.ContainsKey(param)) return;
      ExplorisParams.Add(param, 0);
    }

    public static void AddFusionParam(string param)
    {
      if (FusionParams.ContainsKey(param)) return;
      FusionParams.Add(param, 0);
    }

    public static bool EclipseKey(string key)
    {
      return ExplorisParams.ContainsKey(key);
    }

    public static bool FusionKey(string key)
    {
      return FusionParams.ContainsKey(key);
    }

    public static string GetDescription(string heliosID)
    {
      int eIndex;
      ExplorisLexicon.TryGetValue(heliosID, out eIndex);

      string str = "Alternative to " + ExplorisTerm[eIndex].Single + " (exploris)";
      if (ExplorisTerm[eIndex].Multi != null) str += " or " + ExplorisTerm[eIndex].Multi + " (exploris)";

      string fTerm;
      FusionLexicon.TryGetValue(heliosID, out fTerm);
      str += " or " + fTerm + " (tribrid).";

      return str;
    }
  }
}

