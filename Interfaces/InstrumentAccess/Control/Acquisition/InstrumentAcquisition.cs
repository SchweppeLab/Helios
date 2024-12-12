extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Acquisition
{

    /// <summary>
    /// Wrapper around IAcquisition. From IAPIDocs:<br/>
    /// This interface allows the direct access to method/acquisition starts, 
    /// to wait for contact closure or to extend the delay between the start of 
    /// the acquisition process and the real start of the first scan programmatically.
    /// </summary>
    public interface IInstAcquisition
    {
        /// <summary>
        /// This event will be fired after UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition.State has changed its value. 
        /// The current instrument state will be carried along.
        /// </summary>
        event EventHandler<SCEventArgs> StateChanged;

        /// <summary>
        /// From IAPI Docs:<br/>
        /// This event will be fired when the current acquisition ended.<br/><br/>
        /// Scans may be created without an explicite acquisition, so further scans may arrive after an acquisition stopped.
        /// It may even be possible that few scans belonging to the last acquisition may arrive 
        /// and that an opened rawfile will gather them because of a flushing data queue on the transport layer.<br/><br/>
        /// The specific information of a scan will reflect the information whether a scan belongs to an acquisition or not.
        /// </summary>
        event EventHandler<EventArgs> AcquisitionStreamClosing;

        /// <summary>
        /// From IAPI Docs:<br/>
        /// This event will be fired when a new acquisition is started and the system is about to open rawfiles, etc.<br/><br/>
        /// Scans may be created without an explicite acquisition if the instrument is 'just' set to running.
        /// An acquisition is not necessarily bound to a rawfile, but it is in most cases.<br/><br/>
        /// The individual information of a scan will reflect the information whether a scan belongs to an acquisition or not.
        /// </summary>
        event EventHandler<AOEventArgs> AcquisitionStreamOpening;
    }

    class InstAcquisitionExploris : IInstAcquisition
    {
        exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
        public event EventHandler<SCEventArgs> StateChanged;
        public event EventHandler<EventArgs> AcquisitionStreamClosing;
        public event EventHandler<AOEventArgs> AcquisitionStreamOpening;

        public InstAcquisitionExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
        {
            acquisition = c.Acquisition;
            acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingFusion;
            acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningFusion;
            acquisition.StateChanged += StateChangedFusion;
        }

        void AcquisitionStreamClosingFusion(object sender, EventArgs e)
        {
            OnAcquisitionStreamClosing(e);
        }

        void AcquisitionStreamOpeningFusion(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
        {
            AOEventArgs args = new AOEventArgs(e);
            OnAcquisitionStreamOpening(args);
        }

        protected virtual void OnAcquisitionStreamClosing(EventArgs e)
        {
            EventHandler<EventArgs> handler = AcquisitionStreamClosing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAcquisitionStreamOpening(AOEventArgs e)
        {
            EventHandler<AOEventArgs> handler = AcquisitionStreamOpening;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStateChanged(SCEventArgs e)
        {
            EventHandler<SCEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void StateChangedFusion(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
            SCEventArgs args = new SCEventArgs(e);
            OnStateChanged(args);
        }
    }

    class InstAcquisitionFusion : IInstAcquisition
    {
        Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
        public event EventHandler<SCEventArgs> StateChanged;
        public event EventHandler<EventArgs> AcquisitionStreamClosing;
        public event EventHandler<AOEventArgs> AcquisitionStreamOpening;
        
        public InstAcquisitionFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
        {
            acquisition = c.Acquisition;
            acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingFusion;
            acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningFusion;
            acquisition.StateChanged += StateChangedFusion;
        }

        void AcquisitionStreamClosingFusion(object sender, EventArgs e)
        {
            OnAcquisitionStreamClosing(e);
        }

        void AcquisitionStreamOpeningFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
        {
            AOEventArgs args = new AOEventArgs(e);
            OnAcquisitionStreamOpening(args);
        }

        protected virtual void OnAcquisitionStreamClosing(EventArgs e)
        {
            EventHandler<EventArgs> handler = AcquisitionStreamClosing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAcquisitionStreamOpening(AOEventArgs e)
        {
            EventHandler<AOEventArgs> handler = AcquisitionStreamOpening;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStateChanged(SCEventArgs e)
        {
            EventHandler<SCEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void StateChangedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
            SCEventArgs args = new SCEventArgs(e);
            OnStateChanged(args);
        }
    }

}
