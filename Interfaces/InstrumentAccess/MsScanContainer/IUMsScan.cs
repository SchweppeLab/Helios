extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMIDAS.Data.Spectrum;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;
using UIAPI.Interfaces.SpectrumFormat;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
  /// <summary>
  /// Wrapper around IAPI IMsScan. Because the IMsScan interface could be ambiguous, I am 
  /// not sure yet how I want to play this out.
  /// </summary>
  public interface IUMsScan : IUSpectrum, IDisposable
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
  }

  internal class UMsScanExploris : IUMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }
    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IUNoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; }
    public IEnumerable<IUCentroid> Centroids { get; }
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public UMsScanExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.StatusLog;
      Trailer = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.Trailer;
      TuneData = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.TuneData;
      Centroids = (IEnumerable<IUCentroid>)m.Centroids;
      ChargeEnvelopes = (IChargeEnvelope[])m.ChargeEnvelopes;
      NoiseBand = (IEnumerable<IUNoiseNode>)m.NoiseBand;
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
  }

  internal class UMsScanFusion : IUMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IUNoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; }
    public IEnumerable<IUCentroid> Centroids { get; }
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public UMsScanFusion(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
    {
      Header = m.Header;
      StatusLog = m.StatusLog;
      Trailer = m.Trailer;
      TuneData = m.TuneData;
      Centroids = (IEnumerable<IUCentroid>)m.Centroids;
      ChargeEnvelopes = m.ChargeEnvelopes;
      NoiseBand = (IEnumerable<IUNoiseNode>)m.NoiseBand;
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
  }

  internal class UMsScanVMS : IUMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<IUNoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; } = null;
    public IEnumerable<IUCentroid> Centroids => CentroidList;
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public List<IUCentroid> CentroidList { get; } = new List<IUCentroid>();

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

      if (m.Centroid) CentroidCount = m.Count;
      //else CentroidCount = null; //not needed?
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
  }
}
