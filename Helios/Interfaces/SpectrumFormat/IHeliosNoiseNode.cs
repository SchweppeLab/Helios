using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.SpectrumFormat
{
  /// <summary>
  /// From IAPI: Baseline of the noise node. The value will be null if no baseline is available.
  /// </summary>
  public interface IHeliosNoiseNode : IHeliosMassIntensity
  {
    double? Baseline { get; }
  }

  internal class UNoiseNode : IHeliosNoiseNode
  {
    public double? Baseline { get; }
    public double Mz { get; }

    public double Intensity { get; }

    public UNoiseNode(Thermo.Interfaces.SpectrumFormat_V1.INoiseNode n)
    {
      Baseline = n.Baseline;
      Mz = n.Mz;
      Intensity = n.Intensity;
    }
  }
}
