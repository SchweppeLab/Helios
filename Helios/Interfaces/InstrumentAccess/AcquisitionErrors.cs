extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess
{
    /// <summary>
    /// A wrapper around IAPI IMessage interface. Documentation from IAPI:<br/>
    /// Describes an error coming from the instrument during an acquisition. This interface will not be used for status reports or messages of the transport layer.
    /// </summary>
    public interface IHeliosAcquisitionError
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
    internal class HeliosAcquisitionError : IHeliosAcquisitionError
    {
        public string Content { get; }
        public TimeSpan Occurrence { get; }
        public HeliosAcquisitionError()
        {

        }
        public HeliosAcquisitionError(Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionError ae)
        {
            Content = ae.Content;
            Occurrence = ae.Occurrence;
        }
        public HeliosAcquisitionError(exploris.Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionError ae)
        {
            Content = ae.Content;
            Occurrence = ae.Occurrence;
        }
    }

  //
  // Summary:
  //     This implementation of EventArgs carries a list of Thermo.Interfaces.InstrumentAccess_V1.IAcquisitionErrors.
  //     This class will not be used for status reports or messages of the transport layer.
  //
  //
  // Remarks:
  //     An instance of this class will be created by Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccess.AcquisitionErrorsArrived.
  public abstract class AcquisitionErrorsArrivedEventArgs : EventArgs
  {
    //
    // Summary:
    //     Get access to the errors that have arrived from the instrument.
    public IList<IHeliosAcquisitionError> Errors { get; protected set; }

    //
    // Summary:
    //     Create a new Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs.
    protected AcquisitionErrorsArrivedEventArgs()
    {
    }
  }

  internal class ExplorisAcquisitionErrorsArrivedEventArgs : AcquisitionErrorsArrivedEventArgs
  { 
    public ExplorisAcquisitionErrorsArrivedEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
    {
      Errors = new List<IHeliosAcquisitionError>();
      foreach (var v in e.Errors)
      {
        IHeliosAcquisitionError err = new HeliosAcquisitionError(v);
        Errors.Add(err);
      }

    }
  }

 
  internal class FusionAcquisitionErrorsArrivedEventArgs : AcquisitionErrorsArrivedEventArgs
  {

    public FusionAcquisitionErrorsArrivedEventArgs(Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
    {
      Errors = new List<IHeliosAcquisitionError>();
      foreach (var v in e.Errors)
      {
        IHeliosAcquisitionError err = new HeliosAcquisitionError(v);
        Errors.Add(err);
      }

    }
  }
}
