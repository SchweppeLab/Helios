extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
    public interface IUMsScan : IMsScan, IDisposable
    {
        IDictionary<string,string> Header { get; }
        Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
        Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
        Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }
    }

    class UMsScanExploris : IUMsScan
    {
        public IDictionary<string, string> Header { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }
    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<INoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; }
    public IEnumerable<ICentroid> Centroids { get; }
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public UMsScanExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
        {
            Header = m.Header;
            StatusLog = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.StatusLog;
            Trailer = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.Trailer;
            TuneData = (Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess)m.TuneData;
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

    class UMsScanFusion : IUMsScan
    {
        public IDictionary<string, string> Header { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess StatusLog { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess Trailer { get; }
        public Thermo.Interfaces.SpectrumFormat_V1.IInformationSourceAccess TuneData { get; }

    public string DetectorName { get; }
    public int? NoiseCount { get; }
    public IEnumerable<INoiseNode> NoiseBand { get; }
    public int? CentroidCount { get; }
    public IEnumerable<ICentroid> Centroids { get; }
    public IChargeEnvelope[] ChargeEnvelopes { get; }

    public UMsScanFusion(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScan m)
        {
            Header = m.Header;
            StatusLog = m.StatusLog;
            Trailer = m.Trailer;
            TuneData = m.TuneData;
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
