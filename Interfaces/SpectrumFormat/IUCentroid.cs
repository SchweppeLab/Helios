extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.SpectrumFormat
{
  public interface IUCentroid : IUMassIntensity
  {
    bool? IsExceptional { get; }
    bool? IsReferenced { get; }
    bool? IsMerged { get; }
    bool? IsFragmented { get; }
    int? Charge { get; }
    new IUMassIntensity[] Profile { get; }
    double? Resolution { get; }
    int? ChargeEnvelopeIndex { get; }
    bool? IsMonoisotopic { get; }
    bool? IsClusterTop { get; }
    bool? IsIsotopicallyResolved { get; }
    bool? IsSaturated { get; }
    double Mz { get; }
    double Intensity { get; }

  }

  internal class UCentroid : IUCentroid
  {
    public bool? IsExceptional => throw new NotImplementedException();

    public bool? IsReferenced => throw new NotImplementedException();

    public bool? IsMerged => throw new NotImplementedException();

    public bool? IsFragmented => throw new NotImplementedException();

    public int? Charge => throw new NotImplementedException();

    public IUMassIntensity[] Profile => throw new NotImplementedException();

    public double? Resolution => throw new NotImplementedException();

    public int? ChargeEnvelopeIndex => throw new NotImplementedException();

    public bool? IsMonoisotopic => throw new NotImplementedException();

    public bool? IsClusterTop => throw new NotImplementedException();

    public bool? IsIsotopicallyResolved => throw new NotImplementedException();
    public bool? IsSaturated => throw new NotImplementedException();


    public double Mz { get; }

    public double Intensity { get; }

    public UCentroid(double mz, double intensity)
    {
      Mz = mz;
      Intensity = intensity;
    }
  }
}
