extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMIDAS.Data.Spectrum;
using Thermo.Interfaces.SpectrumFormat_V1;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
  class Centroid : ICentroid
  {
    public bool? IsExceptional => throw new NotImplementedException();

    public bool? IsReferenced => throw new NotImplementedException();

    public bool? IsMerged => throw new NotImplementedException();

    public bool? IsFragmented => throw new NotImplementedException();

    public int? Charge => throw new NotImplementedException();

    public IMassIntensity[] Profile => throw new NotImplementedException();

    public double? Resolution => throw new NotImplementedException();

    public int? ChargeEnvelopeIndex => throw new NotImplementedException();

    public bool? IsMonoisotopic => throw new NotImplementedException();

    public bool? IsClusterTop => throw new NotImplementedException();

    public double Mz { get; }

    public double Intensity { get; }

    public Centroid(double mz, double intensity)
    {
      Mz = mz;  
      Intensity = intensity;
    }
  }

  class VMSScan : IUMsScan
  {
    public IDictionary<string, string> Header { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
    public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<INoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; } = null;
    public IEnumerable<ICentroid> Centroids => CentroidList;
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public List<ICentroid> CentroidList { get; } = new List<ICentroid>();

    public VMSScan(Spectrum m)
    {
      Header = new Dictionary<string, string>();
      Header.Add("ScanNumber",m.ScanNumber.ToString());
      Header.Add("MSOrder",m.MsLevel.ToString());
      Header.Add("RetentionTime",m.RetentionTime.ToString());
      Header.Add("Filter",m.ScanFilter.ToString());
      if (m.Centroid) CentroidCount = m.Count;
      //else CentroidCount = null; //not needed?
      foreach (var dp in m.DataPoints) 
      { 
        CentroidList.Add(new Centroid(dp.Mz, dp.Intensity));
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
