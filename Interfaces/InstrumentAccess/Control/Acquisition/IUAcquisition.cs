extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Acquisition
{

  /// <summary>
  /// Wrapper around IAcquisition. From IAPIDocs:<br/>
  /// This interface allows the direct access to method/acquisition starts, 
  /// to wait for contact closure or to extend the delay between the start of 
  /// the acquisition process and the real start of the first scan programmatically.
  /// </summary>
  public interface IUAcquisition
  {

    /// <summary>
    /// Get access to the current state of the instrument.
    /// </summary>
    IUState State { get; }

    /// <summary>
    /// Will it be possible to pause the current operation?
    /// </summary>
    bool CanPause { get; }

    /// <summary>
    /// Will it be possible to pause the current operation?
    /// </summary>
    bool CanResume { get; }

    /// <summary>
    /// This event will be fired after UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IInstAcquisition.State has changed its value. 
    /// The current instrument state will be carried along.
    /// </summary>
    event EventHandler<StateChangedEventArgs> StateChanged;

    /// <summary>
    /// From IAPI Docs:<br/>
    /// This event will be fired when the current acquisition ended.<br/><br/>
    /// Scans may be created without an explicite acquisition, so further scans may arrive after an acquisition stopped.
    /// It may even be possible that few scans belonging to the last acquisition may arrive 
    /// and that an opened rawfile will gather them because of a flushing data queue on the transport layer.<br/><br/>
    /// The specific information of a scan will reflect the information whether a scan belongs to an acquisition or not.
    /// </summary>
    event EventHandler AcquisitionStreamClosing;

    /// <summary>
    /// From IAPI Docs:<br/>
    /// This event will be fired when a new acquisition is started and the system is about to open rawfiles, etc.<br/><br/>
    /// Scans may be created without an explicite acquisition if the instrument is 'just' set to running.
    /// An acquisition is not necessarily bound to a rawfile, but it is in most cases.<br/><br/>
    /// The individual information of a scan will reflect the information whether a scan belongs to an acquisition or not.
    /// </summary>
    event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;
  }

  internal class UAcquisitionExploris : IUAcquisition
  {
    exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IUState State { get; }
    public bool CanPause { get; }
    public bool CanResume { get; }

    public UAcquisitionExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
    {
      acquisition = c.Acquisition;
      acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingExploris;
      acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningExploris;
      acquisition.StateChanged += StateChangedFusion;

      State = new UStateExploris(acquisition.State);
      CanPause = acquisition.CanPause;
      CanResume = acquisition.CanResume;
    }

        void AcquisitionStreamClosingExploris(object sender, EventArgs e)
        {
            OnAcquisitionStreamClosing(e);
        }

        void AcquisitionStreamOpeningExploris(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
        {
            AcquisitionOpeningEventArgs args = new ExplorisAcquisitionOpeningEventArgs(e);
            OnAcquisitionStreamOpening(args);
        }

        protected virtual void OnAcquisitionStreamClosing(EventArgs e)
        {
            EventHandler handler = AcquisitionStreamClosing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAcquisitionStreamOpening(AcquisitionOpeningEventArgs e)
        {
            EventHandler<AcquisitionOpeningEventArgs> handler = AcquisitionStreamOpening;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            EventHandler<StateChangedEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void StateChangedFusion(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
      StateChangedEventArgs args = new ExplorisStateChangedEventArgs(e);
            OnStateChanged(args);
        }
    }

  internal class UAcquisitionFusion : IUAcquisition
  {
    Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IUState State { get; }
    public bool CanPause { get; }
    public bool CanResume { get; }

        
    public UAcquisitionFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
    {
      acquisition = c.Acquisition;
      acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingFusion;
      acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningFusion;
      acquisition.StateChanged += StateChangedFusion;
      State = new UStateFusion(acquisition.State);
      CanPause = acquisition.CanPause;
      CanResume = acquisition.CanResume;
    }

        void AcquisitionStreamClosingFusion(object sender, EventArgs e)
        {
            OnAcquisitionStreamClosing(e);
        }

        void AcquisitionStreamOpeningFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.AcquisitionOpeningEventArgs e)
        {
            AcquisitionOpeningEventArgs args = new FusionAcquisitionOpeningEventArgs(e);
            OnAcquisitionStreamOpening(args);
        }

        protected virtual void OnAcquisitionStreamClosing(EventArgs e)
        {
            EventHandler handler = AcquisitionStreamClosing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAcquisitionStreamOpening(AcquisitionOpeningEventArgs e)
        {
            EventHandler<AcquisitionOpeningEventArgs> handler = AcquisitionStreamOpening;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            EventHandler<StateChangedEventArgs> handler = StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void StateChangedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
      StateChangedEventArgs args = new FusionStateChangedEventArgs(e);
            OnStateChanged(args);
        }
    }

  internal class UAcquisitionVMS : IUAcquisition
  {
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IUState State { get; } = new UStateVMS();
    public bool CanPause { get; } = true;
    public bool CanResume { get; } = true;

    public void OnStateChanged(StateChangedEventArgs e)
    {
      StateChanged?.Invoke(this, e);
    }

    public void OnAcquisitionStreamClosing(EventArgs e)
    {
      AcquisitionStreamClosing?.Invoke(this, e);
    }

    public void OnAcquisitionStreamOpening(AcquisitionOpeningEventArgs e)
    {
      AcquisitionStreamOpening?.Invoke(this, e);
    }
  }
}
