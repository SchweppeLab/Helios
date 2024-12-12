extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control
{
    /// <summary>
    /// This interface wraps the extensions to IControl by Exploris and Fusion based instruments.
    /// </summary>    
    public interface IInstControl
    {
        /// <summary>
        /// Get access to the acquisition interface.
        /// This property is the instrument-specific implementation for Fusion and Exploris-based instruments.
        /// </summary>  
        UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Acquisition { get; }
    }

    class InstControlExploris : IInstControl
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl control;
        public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Acquisition { get; }
        public InstControlExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess ia)
        {
            control = ia.Control;
            //Acquisition = InstAcquisitionFactory.Get(control);
            Acquisition = new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionExploris(control);
        }
    }
    class InstControlFusion : IInstControl
    {
        fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl control;
        public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Acquisition { get; }
        public InstControlFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess ia)
        {
            control = ia.Control;
            //Acquisition = InstAcquisitionFactory.Get(control);
            Acquisition = new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionFusion(control);
        }
    }

    //static class InstAcquisitionFactory
    //{

    //    static public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Get(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
    //    {
    //        UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionExploris instAcquisition = new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionExploris(c);
    //        return instAcquisition;
    //    }

    //    static public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition Get(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
    //    {
    //        UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionFusion instAcquisition = new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.InstAcquisitionFusion(c);
    //        return instAcquisition;
    //    }
    //}
}
