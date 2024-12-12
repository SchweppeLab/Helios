extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.MsScanContainer
{
    public interface IMSEventArgs
    {
        IUMsScan GetScan();
    }

    class MSEventArgsExploris : EventArgs, IMSEventArgs
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs args;
        public MSEventArgsExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer.ExplorisMsScanEventArgs e)
        {
            args = e;
        }

        public IUMsScan GetScan()
        {
            IUMsScan scan = new UMsScanExploris(args.GetScan());
            return scan;
        }
    }

    class MSEventArgsFusion : EventArgs, IMSEventArgs
    {
        Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs args;

        public MSEventArgsFusion(Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.MsScanEventArgs e)
        {
            args = e;
        }

        public IUMsScan GetScan()
        {
            IUMsScan scan = new UMsScanFusion(args.GetScan());
            return scan;
        }
    }

}
