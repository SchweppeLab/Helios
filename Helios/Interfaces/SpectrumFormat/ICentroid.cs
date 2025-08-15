extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.SpectrumFormat
{
  public interface ICentroid : IMassIntensity
  {
    bool? IsExceptional { get; }
    bool? IsReferenced { get; }
    bool? IsMerged { get; }
    bool? IsFragmented { get; }
    int? Charge { get; }
    IMassIntensity[] Profile { get; }
    double? Resolution { get; }
    int? ChargeEnvelopeIndex { get; }
    bool? IsMonoisotopic { get; }
    bool? IsClusterTop { get; }
    bool? IsIsotopicallyResolved { get; }
    bool? IsSaturated { get; }

  }

  class Centroid : ICentroid
  {
    public bool? IsExceptional => throw new NotImplementedException();

    public bool? IsReferenced => throw new NotImplementedException();

    public bool? IsMerged => throw new NotImplementedException();

    public bool? IsFragmented => throw new NotImplementedException();

    public int? Charge => throw new NotImplementedException();

    public IMassIntensity[] Profile => throw new NotImplementedException();

    public double? Resolution { get; }

    public int? ChargeEnvelopeIndex => throw new NotImplementedException();

    public bool? IsMonoisotopic { get; } = null;

    public bool? IsClusterTop => throw new NotImplementedException();

    public bool? IsIsotopicallyResolved => throw new NotImplementedException();
    public bool? IsSaturated => throw new NotImplementedException();


    public double Mz { get; }

    public double Intensity { get; }

    public Centroid(double mz, double intensity, double? resolution)
    {
      Mz = mz;
      Intensity = intensity;
      Resolution = resolution;
    }

    public Centroid(exploris.Thermo.Interfaces.SpectrumFormat_V1.ICentroid c)
    {
      Mz = c.Mz;
      Intensity = c.Intensity;
      IsMonoisotopic = c.IsMonoisotopic;
    }
  }
}
