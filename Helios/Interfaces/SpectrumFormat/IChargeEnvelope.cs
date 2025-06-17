extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.SpectrumFormat
{
  //
  // Summary:
  //     This interface covers charge envelopes. All isotopes just having a different
  //     charge belong to the same charge envelope.
  public interface IChargeEnvelope
  {
    //
    // Summary:
    //     This is the monoisotopic mass that a particular peak belongs to. The value is
    //     a calculated one, it is very likely that this cannot be seen at all in the spectrum.
    //     But it is the reference point of all members of one charge envelope.
    double MonoisotopicMass { get; }

    //
    // Summary:
    //     This cross-correlation factor is the maximum of all cross-correlation values
    //     over all averagines. An averagine is a statistical model of the isotope distribution
    //     of the same molecule (see remarks of Thermo.Interfaces.SpectrumFormat_V1.IChargeEnvelope)
    //     at a given charge. The observed peaks within the spectrum are fitted to this,
    //     the overlap is calculated by a cross-correlation that only takes the intensities
    //     into account.
    //
    //     The averagine model is strongly linked to peptide analysis. As an example, averagine
    //     mass distribution for pesticides are totally different.
    //
    //     Cross-correlation factors vary in the range 0 to 1. 0 will be set if Thermo.Interfaces.SpectrumFormat_V1.IChargeEnvelope.MonoisotopicMass
    //     is also 0. In this case, the fit was unsuccessful.
    double CrossCorrelation { get; }

    //
    // Summary:
    //     This is the index to the top peak centroid in the centroid list (Thermo.Interfaces.SpectrumFormat_V1.ISpectrum.Centroids).
    //     One can use this to get access to the mass of the so called top peak.
    //
    // Remarks:
    //     The top peak is that peak in a charge envelope that fulfils two requirements
    //     in this order: 1) never being considered to be part of another charge envelope,
    //     and 2) having the highest abundance.
    int TopPeakCentroidId { get; }
  }

  internal class HeliosChargeEnvelope : IChargeEnvelope
  {
    public double MonoisotopicMass { get; } = 0.0;
    public double CrossCorrelation { get; } = 0.0;
    public int TopPeakCentroidId { get; } = 0;

    public HeliosChargeEnvelope(exploris.Thermo.Interfaces.SpectrumFormat_V1.IChargeEnvelope e)
    {
      MonoisotopicMass = e.MonoisotopicMass;
      CrossCorrelation = e.CrossCorrelation;
      TopPeakCentroidId = e.TopPeakCentroidId;
    }

    public HeliosChargeEnvelope(Thermo.Interfaces.SpectrumFormat_V1.IChargeEnvelope e)
    {
      MonoisotopicMass = e.MonoisotopicMass;
      CrossCorrelation = e.CrossCorrelation;
      TopPeakCentroidId = e.TopPeakCentroidId;
    }
  }
}
