extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
    public interface IInstMsScanContainer
    {
        string DetectorClass { get; }
        event EventHandler<IMSEventArgs> MsScanArrived;
    }

    class InstMsScanContainerExploris : IInstMsScanContainer
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.IExplorisMsScanContainer cont;
        public string DetectorClass { get; }
        public event EventHandler<IMSEventArgs> MsScanArrived;

        public InstMsScanContainerExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess c, int msDetectorSet)
        {
            cont = c.GetMsScanContainer(msDetectorSet);
            cont.MsScanArrived += MsScanArrivedExploris;
            DetectorClass = cont.DetectorClass;
        }

        void MsScanArrivedExploris(object sender, exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
        {
            IMSEventArgs args = new MSEventArgsExploris(e);
            OnMsScanArrived(args);
        }

        protected virtual void OnMsScanArrived(IMSEventArgs e)
        {
            EventHandler<IMSEventArgs> handler = MsScanArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    class InstMsScanContainerFusion : IInstMsScanContainer
    {
        fusion.Thermo.Interfaces.FusionAccess_V1.MsScanContainer.IFusionMsScanContainer cont;
        public string DetectorClass { get; }
        public event EventHandler<IMSEventArgs> MsScanArrived;

        public InstMsScanContainerFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess c, int msDetectorSet)
        {
            cont = c.GetMsScanContainer(msDetectorSet);
            cont.MsScanArrived += MsScanArrivedFusion;
            DetectorClass = cont.DetectorClass;
        }

        void MsScanArrivedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
        {
            IMSEventArgs args = new MSEventArgsFusion(e);
            OnMsScanArrived(args);
        }

        protected virtual void OnMsScanArrived(IMSEventArgs e)
        {
            EventHandler<IMSEventArgs> handler = MsScanArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
