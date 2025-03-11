extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Spectrum;
using Helios.Interfaces.SpectrumFormat;

namespace Helios.Interfaces.InstrumentAccess.MsScanContainer
{

  public enum ScanSource
  {
    None,
    Exploris,
    Fusion,
    VMS
  }

  /// <summary>
  /// Wrapper around IAPI IMsScan. Because the IMsScan interface could be ambiguous, I am 
  /// not sure yet how I want to play this out.
  /// </summary>
  public interface IMsScan : IHeliosSpectrum, IDisposable
  {
    /// <summary>
    /// Get access to the information coming from the header. It is a set of name/value pairs. A pure name has a value of null.
    /// </summary>
    IDictionary<string,string> Header { get; }

    /// <summary>
    /// Get access to the StatusLog information source. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    IHeliosInformationSourceAccess StatusLog { get; }

    /// <summary>
    /// Get access to the Trailer information source. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    IHeliosInformationSourceAccess Trailer { get; }

    /// <summary>
    /// Get access to the TuneData information source. This is mostly accessible at acquisition start. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    IHeliosInformationSourceAccess TuneData { get; }

    /// <summary>
    /// Get the string value of a header identifier using a universal dictionary.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryHeader(string id, out string value);

    /// <summary>
    /// Get the string value of a trailer identifier using a universal dictionary.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryTrailer(string id, out string value);

  }

  internal class HeliosMsScanExploris : IMsScan
  {
    //exploris.Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan msScan;
    public IDictionary<string, string> Header { get; }
    public IHeliosInformationSourceAccess StatusLog { get; }
    public IHeliosInformationSourceAccess Trailer { get; }
    public IHeliosInformationSourceAccess TuneData { get; }
    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand => NoiseList;
    private List<IHeliosNoiseNode> NoiseList { get; } = new List<IHeliosNoiseNode>();
    public int? CentroidCount { get; }
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    public IHeliosChargeEnvelope[] ChargeEnvelopes { get; } = null;

    private ScanSource scanSource { get; } = ScanSource.Exploris;

    public HeliosMsScanExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = new HeliosInformationSourceAccessExploris(m.StatusLog);
      Trailer = new HeliosInformationSourceAccessExploris(m.Trailer);
      TuneData = new HeliosInformationSourceAccessExploris(m.TuneData);
      //is a copy what we really want to do? Maybe find a way to just recast a reference?
      foreach (exploris.Thermo.Interfaces.SpectrumFormat_V1.ICentroid centroid in m.Centroids)
      {
        CentroidList.Add(new HeliosCentroid(centroid));
      }
      foreach (exploris.Thermo.Interfaces.SpectrumFormat_V1.INoiseNode n in m.NoiseBand)
      {
        NoiseList.Add(new HeliosNoiseNode(n));
      }
      if (m.ChargeEnvelopes != null)
      {
        ChargeEnvelopes = new IHeliosChargeEnvelope[m.ChargeEnvelopes.Length];
        for (int i = 0; i < ChargeEnvelopes.Length; i++)
        {
          ChargeEnvelopes[i] = new HeliosChargeEnvelope(m.ChargeEnvelopes[i]);
        }
      }
      CentroidCount = m.CentroidCount;
      NoiseCount = m.NoiseCount;
      DetectorName = m.DetectorName;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
          // free managed resources
      }
      // free native resources if there are any.
    }

    public bool TryHeader(string id, out string value)
    {
      string key=HeliosDictionary.GetHeader(id,ScanSource.Exploris);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Header.TryGetValue(key, out value);
    }

    public bool TryTrailer(string id, out string value)
    {
      string key = HeliosDictionary.GetTrailer(id, ScanSource.Exploris);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Trailer.TryGetValue(key, out value);
    }
  }

  internal class HeliosMsScanFusion : IMsScan
  {
    public IDictionary<string, string> Header { get; }
    public IHeliosInformationSourceAccess StatusLog { get; }
    public IHeliosInformationSourceAccess Trailer { get; }
    public IHeliosInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand => NoiseList;
    private List<IHeliosNoiseNode> NoiseList { get; } = new List<IHeliosNoiseNode>();
    public int? CentroidCount { get; } = null;
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    public IHeliosChargeEnvelope[] ChargeEnvelopes { get; } = null;

    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    private ScanSource scanSource { get; } = ScanSource.Fusion;

    public HeliosMsScanFusion(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = new HeliosInformationSourceAccessFusion(m.StatusLog);
      Trailer = new HeliosInformationSourceAccessFusion(m.Trailer);
      TuneData = new HeliosInformationSourceAccessFusion(m.TuneData);
      //is a copy what we really want to do? Maybe find a way to just recast a reference?
      foreach(Thermo.Interfaces.SpectrumFormat_V1.ICentroid centroid in m.Centroids)
      {
        CentroidList.Add(new HeliosCentroid(centroid.Mz,centroid.Intensity));
      }
      if (m.ChargeEnvelopes != null)
      {
        ChargeEnvelopes = new IHeliosChargeEnvelope[m.ChargeEnvelopes.Length];
        for (int i = 0; i < ChargeEnvelopes.Length; i++)
        {
          ChargeEnvelopes[i] = new HeliosChargeEnvelope(m.ChargeEnvelopes[i]);
        }
      }
      foreach (Thermo.Interfaces.SpectrumFormat_V1.INoiseNode n in m.NoiseBand)
      {
        NoiseList.Add(new HeliosNoiseNode(n));
      }
      CentroidCount = m.CentroidCount;
      NoiseCount = m.NoiseCount;
      DetectorName = m.DetectorName;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
          // free managed resources
      }
      // free native resources if there are any.
    }

    public bool TryHeader(string id, out string value)
    {
      string key = HeliosDictionary.GetHeader(id, ScanSource.Fusion);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Header.TryGetValue(key, out value);
    }

    public bool TryTrailer(string id, out string value)
    {
      string key = HeliosDictionary.GetTrailer(id, ScanSource.Fusion);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Trailer.TryGetValue(key, out value);
    }
  }

  internal class HeliosMsScanVMS : IMsScan
  {
    public IDictionary<string, string> Header { get; }
    public IHeliosInformationSourceAccess StatusLog { get; }
    public IHeliosInformationSourceAccess Trailer { get; }
    public IHeliosInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; } = null;
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    public IHeliosChargeEnvelope[] ChargeEnvelopes { get; }

    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    private ScanSource scanSource { get; } = ScanSource.VMS;
    public HeliosMsScanVMS(Spectrum m)
    {
      Header = new Dictionary<string, string>();
      Header.Add("Scan", m.ScanNumber.ToString());
      Header.Add("MSOrder", m.MsLevel.ToString());
      Header.Add("StartTime", m.RetentionTime.ToString());
      Header.Add("FirstMass",m.StartMz.ToString());
      Header.Add("LastMass",m.EndMz.ToString());
      Header.Add("BasePeakIntensity",m.BasePeakIntensity.ToString());
      Header.Add("TIC",m.TotalIonCurrent.ToString());
      Header.Add("Filter", m.ScanFilter.ToString());
      Header.Add("MassAnalyzer", m.Analyzer.ToString());
      Header.Add("InjectTime",m.IonInjectionTime.ToString());
      Header.Add("ScanMode",m.ScanType.ToString());
      if (m.Polarity) Header.Add("Polarity", "Positive");
      else Header.Add("Polarity", "Negative");
      if (m.Centroid) Header.Add("ScanData", "Centroid");
      else Header.Add("ScanData", "Profile");
      if (m.Precursors.Count > 0)
      {
        Header.Add("PrecursorMass[0]", m.Precursors[0].IsolationMz.ToString());
        Header.Add("CollisionEnergy[0]",m.CollisionEnergy.ToString());
      }
      
      HeliosInformationSourceAccess trailer = new HeliosInformationSourceAccess();
      //Example of how to add trailer information from scratch using the nifty UInformationSourceAccess class.
      //trailer.Add("Access ID", "UIAPIzoink");
      trailer.Add("Scan Description", m.ScanFilter.ToString());
      trailer.Add("FAIMS Voltage On",m.FaimsState.ToString());
      trailer.Add("FAIMS CV",m.FaimsCV.ToString());
      if (m.Precursors.Count > 0)
      {
        trailer.Add("Monoisotopic M/Z", m.Precursors[0].MonoisotopicMz.ToString());
        trailer.Add("Charge State", m.Precursors[0].Charge.ToString());
      }
      Trailer = trailer;

      CentroidCount = m.Count;
      foreach (var dp in m.DataPoints)
      {
        CentroidList.Add(new HeliosCentroid(dp.Mz, dp.Intensity));
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // free managed resources
      }
      // free native resources if there are any.
    }

    public bool TryHeader(string id, out string value)
    {
      string key = HeliosDictionary.GetHeader(id, ScanSource.VMS);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Header.TryGetValue(key, out value);
    }

    public bool TryTrailer(string id, out string value)
    {
      string key = HeliosDictionary.GetTrailer(id, ScanSource.VMS);
      if (key == null)
      {
        value = null;
        return false;
      }
      return Trailer.TryGetValue(key, out value);
    }
  }
}
