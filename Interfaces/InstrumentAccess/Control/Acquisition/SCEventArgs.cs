extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Acquisition
{
    /// <summary>
    /// Wrapper around StateChangedEventArgs in IAPI. From IAPI Docs:<br/>
    /// This implementation of EventArgs carries an UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IUIAPIState.
    /// </summary>
    public class SCEventArgs : EventArgs
    {
        /// <summary>
        /// Get access to the current state of the instrument. 
        /// </summary>
        public IUIAPIState State { get; protected set; }

        /// <summary>
        /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.SCEventArgs from an
        /// Exploris Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs.
        /// </summary>
        /// <param name="e">Exploris IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs</param>
        public SCEventArgs(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
            State = new UIAPIStateFusion(e.State);
        }

        /// <summary>
        /// Create a new UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.SCEventArgs from an
        /// Fusion Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs.
        /// </summary>
        /// <param name="e">Fusion IAPI Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs</param>
        public SCEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
            State = new UIAPIStateExploris(e.State);
        }
    }
}
