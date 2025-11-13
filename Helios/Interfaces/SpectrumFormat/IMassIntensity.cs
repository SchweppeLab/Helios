extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.SpectrumFormat
{
  public interface IMassIntensity
  {
    //
    // Summary:
    //     m/z value of the information.
    double Mz { get; }

    //
    // Summary:
    //     Intensity at the given m/z value.
    double Intensity { get; }
  }
}
