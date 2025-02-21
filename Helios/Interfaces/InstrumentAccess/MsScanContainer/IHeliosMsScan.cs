extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Spectrum;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;
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
  public interface IHeliosMsScan : IHeliosSpectrum, IDisposable
  {
    /// <summary>
    /// Get access to the information coming from the header. It is a set of name/value pairs. A pure name has a value of null.
    /// </summary>
    IDictionary<string,string> Header { get; }

    /// <summary>
    /// Get access to the StatusLog information source. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }

    /// <summary>
    /// Get access to the Trailer information source. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }

    /// <summary>
    /// Get access to the TuneData information source. This is mostly accessible at acquisition start. Text representation of numbers will always appear in the independent (US) locale.
    /// </summary>
    Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

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

  internal class UMsScanExploris : IHeliosMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }
    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand => NoiseList;
    private List<IHeliosNoiseNode> NoiseList { get; } = new List<IHeliosNoiseNode>();
    public int? CentroidCount { get; }
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    private ScanSource scanSource { get; } = ScanSource.Exploris;

    public UMsScanExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.StatusLog;
      Trailer = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.Trailer;
      TuneData = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.TuneData;
      //is a copy what we really want to do? Maybe find a way to just recast a reference?
      foreach (ICentroid centroid in m.Centroids)
      {
        CentroidList.Add(new UCentroid(centroid.Mz, centroid.Intensity));
      }
      foreach (INoiseNode n in m.NoiseBand)
      {
        NoiseList.Add(new UNoiseNode(n));
      }
      ChargeEnvelopes = (IChargeEnvelope[])m.ChargeEnvelopes;
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

  internal class UMsScanFusion : IHeliosMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand => NoiseList;
    private List<IHeliosNoiseNode> NoiseList { get; } = new List<IHeliosNoiseNode>();
    public int? CentroidCount { get; } = null;
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    private ScanSource scanSource { get; } = ScanSource.Fusion;

    public UMsScanFusion(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = m.StatusLog;
      Trailer = m.Trailer;
      TuneData = m.TuneData;
      //is a copy what we really want to do? Maybe find a way to just recast a reference?
      foreach(ICentroid centroid in m.Centroids)
      {
        CentroidList.Add(new UCentroid(centroid.Mz,centroid.Intensity));
      }
      ChargeEnvelopes = m.ChargeEnvelopes;
      foreach(INoiseNode n in m.NoiseBand)
      {
        NoiseList.Add(new UNoiseNode(n));
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

  internal class UMsScanVMS : IHeliosMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IHeliosNoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; } = null;
    public IEnumerable<IHeliosCentroid> Centroids => CentroidList;
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    private List<IHeliosCentroid> CentroidList { get; } = new List<IHeliosCentroid>();
    private ScanSource scanSource { get; } = ScanSource.VMS;
    public UMsScanVMS(Spectrum m)
    {
      Header = new Dictionary<string, string>();
      Header.Add("ScanNumber", m.ScanNumber.ToString());
      Header.Add("MSOrder", m.MsLevel.ToString());
      Header.Add("RetentionTime", m.RetentionTime.ToString());
      Header.Add("Filter", m.ScanFilter.ToString());
      if (m.Precursors.Count > 0)
      {
        Header.Add("PrecursorMass[0]", m.Precursors[0].IsolationMz.ToString());
        Header.Add("CollisionEnergy[0]",m.CollisionEnergy.ToString());
      }
      
      UInformationSourceAccess trailer = new UInformationSourceAccess();
      //Example of how to add trailer information from scratch using the nifty UInformationSourceAccess class.
      //trailer.Add("Access ID", "UIAPIzoink");
      trailer.Add("Scan Description", m.ScanFilter.ToString());
      if (m.Precursors.Count > 0)
      {
        trailer.Add("Monoisotopic M/Z", m.Precursors[0].MonoisotopicMz);
        trailer.Add("Charge State", m.Precursors[0].Charge);
      }
      Trailer = trailer;

      CentroidCount = m.Count;
      foreach (var dp in m.DataPoints)
      {
        CentroidList.Add(new UCentroid(dp.Mz, dp.Intensity));
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
