extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess
{
    /// <summary>
    /// Wraps the ContactClosureEventArgs (Fusion only). From IAPI docs:<br/>
    /// The event data for when a contact closure occurs. This can happen on either the rising and/or falling edges.
    /// </summary>
    public class ContactClosureEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if any falling edge events occured since it was last polled
        /// </summary>
        public bool DidFall { get; }
        /// <summary>
        /// Indicates if any rising edge events occured since it was last polled
        /// </summary>
        public bool DidRise { get; }
        /// <summary>
        /// The number of falling edges detected on the contact closure input since it was last polled
        /// </summary>
        public int FallingEdges { get; protected set; }
        /// <summary>
        /// The number of rising edges detected on the contact closure input since it was last polled
        /// </summary>
        public int RisingEdges { get; protected set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="e"></param>
        public ContactClosureEventArgs(fusion.Thermo.Interfaces.FusionAccess_V1.ContactClosureEventArgs e)
        {
            DidFall = e.DidFall;
            DidRise = e.DidRise;
            FallingEdges = e.FallingEdges;
            RisingEdges = e.RisingEdges;
        }
    }
}
