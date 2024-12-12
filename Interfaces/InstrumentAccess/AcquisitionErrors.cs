extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess
{
    /// <summary>
    /// A wrapper around IAPI IMessage interface. Documentation from IAPI:<br/>
    /// Describes an error coming from the instrument during an acquisition. This interface will not be used for status reports or messages of the transport layer.
    /// </summary>
    public interface IUIAPIAcquisitionErrors
    {
        /// <summary>
        /// The text content of the error
        /// </summary>
        string Content { get; }

        /// <summary>
        /// The time difference between acquisition start and this error.
        /// </summary>
        TimeSpan Occurrence { get; }
    }
    class UIAPIAcquisitionErrors : IUIAPIAcquisitionErrors
    {
        public string Content { get; }
        public TimeSpan Occurrence { get; }
        public UIAPIAcquisitionErrors()
        {

        }
        public UIAPIAcquisitionErrors(Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionError ae)
        {
            Content = ae.Content;
            Occurrence = ae.Occurrence;
        }
        public UIAPIAcquisitionErrors(exploris.Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionError ae)
        {
            Content = ae.Content;
            Occurrence = ae.Occurrence;
        }
    }

    /// <summary>
    /// A wrapper around IAPI AcquisitionErrorsArrivedEventArgs. From IAPI Documentation:<br/>
    /// This implementation of EventArgs carries a list of Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionErrors. This class will not be used for status reports or messages of the transport layer.
    /// </summary>
    public class AcquisitionErrorsEventArgs : EventArgs
    {
        /// <summary>
        /// Get access to the errors that have arrived from the instrument.
        /// </summary>
        public IList<IUIAPIAcquisitionErrors> Errors;

        /// <summary>
        /// Create a new UIAPI.Interfaces.InstrumentAccess.AcquisitionErrorsEventArgs from Fusion Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs.
        /// </summary>
        /// <param name="e"></param>
        public AcquisitionErrorsEventArgs(Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            foreach (var v in e.Errors)
            {
                IUIAPIAcquisitionErrors err = new UIAPIAcquisitionErrors(v);
                Errors.Add(err);
            }

        }

        /// <summary>
        /// Create a new UIAPI.Interfaces.InstrumentAccess.AcquisitionErrorsEventArgs from Exploris Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs.
        /// </summary>
        /// <param name="e"></param>
        public AcquisitionErrorsEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            foreach (var v in e.Errors)
            {
                IUIAPIAcquisitionErrors err = new UIAPIAcquisitionErrors(v);
                Errors.Add(err);
            }

        }
    }
}
