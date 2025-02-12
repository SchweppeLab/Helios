using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.SpectrumFormat
{
  /// <summary>
  /// From IAPI: Baseline of the noise node. The value will be null if no baseline is available.
  /// </summary>
  public interface IUNoiseNode : Thermo.Interfaces.SpectrumFormat_V1.INoiseNode
  {
  }
}
