extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.SpectrumFormat
{
  /// <summary>
  /// From IAPI: This class describes a spectrum without information coming alongh with it.
  /// </summary>
  public interface IHeliosSpectrum 
  { 
    /// <summary>
    /// Get access to the name of the detector that acquired this scan.<br/>
    /// Example: The instrument name may be "Thermo Orbitrap Velos Pro", the detector class may be "Hybrid LinearIonTrap Orbitrap" 
    /// and the scan detector name may be "LinearIonTrap".
    /// </summary>
    string DetectorName { get; }

    /// <summary>
    /// Get access to the number of elements returned by Thermo.Interfaces.SpectrumFormat_V1.ISpectrum.NoiseBand.
    /// The value will be null if the detector doesn't support noise information at all.
    /// 0 will be returned if no peaks below noise level were cut off, 1 will be returned for a constant level of the returned intensity.
    /// </summary>
    int? NoiseCount { get; }

    /// <summary>
    /// Get access to the noise information. The noise nodes form a polygon that covers the noise area. 
    /// An empty enumeration is returned if noise information is not present. This value will not be null.
    /// </summary>
    IEnumerable<IHeliosNoiseNode> NoiseBand { get; }
 
    /// <summary>
    /// Get access to the number of centroids in this scan. The value is null if the detector is not capable to provide centroids. 
    /// The value may also be null if a computation cannot be performed.
    /// </summary>
    int? CentroidCount { get; }
  
    /// <summary>
    /// Get access to the centroids and further information to those peaks. An empty enumeration is returned if centroid information is not present. 
    /// This value will not be null.
    /// </summary>
    IEnumerable<IHeliosCentroid> Centroids { get; }

    //
    // Summary:
    //     Access to the array of all charge envelopes found in the spectrum. An empty array
    //     will be returned if nothing had been found. null will be returned if the detector's
    //     processing software doesn't support detection of charge envelopes for this scan.
    ///IChargeEnvelope[] ChargeEnvelopes { get; }
  }
}
