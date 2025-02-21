using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{
  internal static class HeliosDictionary
  {
    private static readonly Dictionary<string, string> HeaderExploris = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> TrailerExploris = new Dictionary<string, string>();

    private static readonly Dictionary<string, string> HeaderFusion = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> TrailerFusion = new Dictionary<string, string>();

    private static readonly Dictionary<string, string> HeaderVMS = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> TrailerVMS = new Dictionary<string, string>();

    static HeliosDictionary() {
      HeaderExploris.Add("Scan", "Scan");
      HeaderExploris.Add("ScanNumber", "Scan");
      HeaderExploris.Add("MSOrder", "MSOrder");

      HeaderExploris.Add("StartTime", "StartTime");
      HeaderExploris.Add("RetentionTime", "StartTime");

      TrailerExploris.Add("Access ID", "Access Id:");
      TrailerExploris.Add("Access Id:", "Access Id:");

      HeaderFusion.Add("Scan", "Scan");
      HeaderFusion.Add("ScanNumber", "Scan");
      HeaderFusion.Add("MSOrder", "MSOrder");

      HeaderFusion.Add("StartTime", "StartTime");
      HeaderFusion.Add("RetentionTime", "StartTime");

      TrailerFusion.Add("Access ID", "Access ID");
      TrailerFusion.Add("Access Id:", "Access ID");


      HeaderVMS.Add("Scan", "ScanNumber");
      HeaderVMS.Add("ScanNumber", "ScanNumber");
      HeaderVMS.Add("MSOrder", "MSOrder");

      HeaderVMS.Add("StartTime", "RetentionTime");
      HeaderVMS.Add("RetentionTime", "RetentionTime");

      TrailerVMS.Add("Access ID", "Access ID");
      TrailerVMS.Add("Access Id:", "Access ID");
    }

    public static string GetHeader(string id, ScanSource source)
    {
      string tmp;
      switch (source)
      {
        case ScanSource.Exploris:
          if (HeaderExploris.TryGetValue(id, out tmp)) return tmp;
          return null;
        case ScanSource.Fusion:
          if (HeaderFusion.TryGetValue(id, out tmp)) return tmp;
          return null;
        case ScanSource.VMS:
          if (HeaderVMS.TryGetValue(id, out tmp)) return tmp;
          return null;
        default:
          return null;
      }
    }

    public static string GetTrailer(string id, ScanSource source)
    {
      string tmp;
      switch (source)
      {
        case ScanSource.Exploris:
          if (TrailerExploris.TryGetValue(id, out tmp)) return tmp;
          return null;
        case ScanSource.Fusion:
          if (TrailerFusion.TryGetValue(id, out tmp)) return tmp;
          return null;
        case ScanSource.VMS:
          if (TrailerVMS.TryGetValue(id, out tmp)) return tmp;
          return null;
        default:
          return null;
      }
    }
  }
}
