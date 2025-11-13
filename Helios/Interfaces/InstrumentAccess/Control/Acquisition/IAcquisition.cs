extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition
{

  /// <summary>
  /// Wrapper around IAcquisition. From IAPIDocs:<br/>
  /// This interface allows the direct access to method/acquisition starts, 
  /// to wait for contact closure or to extend the delay between the start of 
  /// the acquisition process and the real start of the first scan programmatically.
  /// </summary>
  public interface IAcquisition
  {

    /// <summary>
    /// Get access to the current state of the instrument.
    /// </summary>
    IHeliosState State { get; }

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

    //
    // Summary:
    //     Create a new state change object that can put the instrument in the On state.
    //     No data acquisition will be performed, a separate command/logic exists for this
    //     purpose.
    //
    // Returns:
    //     The created state change object can be used in Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition.SetMode(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode)
    //     to set the new state.
    IHeliosOnMode CreateOnMode();

    //
    // Summary:
    //     Create a new state change object that can put the instrument in the Off state.
    //
    //
    // Returns:
    //     The created state change object can be used in Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition.SetMode(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode)
    //     to set the new mode.
    IHeliosOffMode CreateOffMode();

    //
    // Summary:
    //     Transport a new state change request to the instrument. Even if the command has
    //     been submitted successfully to the instrument it may still be possible that the
    //     instrument rejects the request because it has entered a different state in between.
    //
    //
    // Parameters:
    //   newMode:
    //     The new mode that shall be assumed by the instrument.
    //
    // Exceptions:
    //   T:System.ServiceModel.CommunicationException:
    //     The connection to the instrument is not established.
    //
    //   T:System.ArgumentException:
    //     The mode change request has illegal values.
    //
    //   T:System.FormatException:
    //     The mode change request is of an unknown type. Use Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition
    //     to generate a valid type.
    //
    //   T:System.AccessViolationException:
    //     The instrument is under exclusive use of a different component or software package.
    //
    //
    //   T:System.InvalidOperationException:
    //     The instrument is not in the proper condition to accept the state change request.
    void SetMode(IHeliosMode newMode);

    IAcquisitionWorkflow CreatePermanentAcquisition();
    void StartAcquisition(IAcquisitionWorkflow workflow);
    void CancelAcquisition();
    IAcquisitionLimitedByCount CreateAcquisitionLimitedByCount(int count);

    IAcquisitionLimitedByTime CreateAcquisitionLimitedByDuration(TimeSpan duration);

    IAcquisitionMethodRun CreateMethodAcquisition(string methodFileName);
  }

  internal class HeliosAcquisitionExploris : IAcquisition
  {
    exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IHeliosState State { get; }
    public bool CanPause { get; }
    public bool CanResume { get; }

    public HeliosAcquisitionExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
    {
      acquisition = c.Acquisition;
      acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingExploris;
      acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningExploris;
      acquisition.StateChanged += OnStateChanged;

      State = new HeliosStateExploris(acquisition.State);
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

    public void CancelAcquisition()
    {
      //not implemented in Exploris
    }

    public IAcquisitionLimitedByCount CreateAcquisitionLimitedByCount(int count)
    {
      return new HeliosAcquisitionLimitedByCount();
    }

    public IAcquisitionLimitedByTime CreateAcquisitionLimitedByDuration(TimeSpan duration)
    {
      return new HeliosAcquisitionLimitedByTime();
    }

    public IAcquisitionMethodRun CreateMethodAcquisition(string methodFileName)
    {
      return new HeliosAcquisitionMethodRun();
    }

    public IHeliosOffMode CreateOffMode()
    {
      return new HeliosOffModeExploris(acquisition.CreateOffMode());
    }

    public IHeliosOnMode CreateOnMode()
    {
      return new HeliosOnModeExploris(acquisition.CreateOnMode());
    }

    public IAcquisitionWorkflow CreatePermanentAcquisition()
    {
      return null; //not supported in Exploris
    }

    public void SetMode(IHeliosMode newMode)
    {
      acquisition.SetMode(new ExplorisMode(newMode));
    }

    public void StartAcquisition(IAcquisitionWorkflow workflow)
    {
      //not implemented in Exploris
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

    protected virtual void OnStateChanged(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
    {
      StateChanged?.Invoke(this, new ExplorisStateChangedEventArgs(e));
    }

  }

  internal class HeliosAcquisitionFusion : IAcquisition
  {
    Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition acquisition;
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IHeliosState State { get; }
    public bool CanPause { get; }
    public bool CanResume { get; }

        
    public HeliosAcquisitionFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
    {
      acquisition = c.Acquisition;
      acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingFusion;
      acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningFusion;
      acquisition.StateChanged += StateChangedFusion;
      State = new HeliosStateFusion(acquisition.State);
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

    public void CancelAcquisition()
    {
      acquisition.CancelAcquisition();
    }
    public IAcquisitionLimitedByCount CreateAcquisitionLimitedByCount(int count)
    {
      return new HeliosAcquisitionLimitedByCount(acquisition.CreateAcquisitionLimitedByCount(count));
    }

    public IAcquisitionLimitedByTime CreateAcquisitionLimitedByDuration(TimeSpan duration)
    {
      return new HeliosAcquisitionLimitedByTime(acquisition.CreateAcquisitionLimitedByDuration(duration));
    }

    public IAcquisitionMethodRun CreateMethodAcquisition(string methodFileName)
    {
      return new HeliosAcquisitionMethodRun(acquisition.CreateMethodAcquisition(methodFileName));
    }

    public IHeliosOffMode CreateOffMode()
    {
      return new HeliosOffModeFusion(acquisition.CreateOffMode());
    }

    public IHeliosOnMode CreateOnMode()
    {
      return new HeliosOnModeFusion(acquisition.CreateOnMode());
    }

    public IAcquisitionWorkflow CreatePermanentAcquisition()
    {
      return new HeliosAcquisitionWorkflow();
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

    public void SetMode(IHeliosMode newMode)
    {
      acquisition.SetMode(new FusionMode(newMode));
    }

    public void StartAcquisition(IAcquisitionWorkflow workflow)
    {
      acquisition.StartAcquisition(new FusionAcquisitionWorkflow(workflow));
    }

    void StateChangedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
      StateChangedEventArgs args = new FusionStateChangedEventArgs(e);
            OnStateChanged(args);
        }
    }

  internal class HeliosAcquisitionVMS : IAcquisition
  {
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    public IHeliosState State { get; } = new HeliosStateVMS();
    public bool CanPause { get; } = true;
    public bool CanResume { get; } = true;

    public void CancelAcquisition()
    {
      //do nothing
    }

    public IAcquisitionLimitedByCount CreateAcquisitionLimitedByCount(int count)
    {
      return new HeliosAcquisitionLimitedByCount();
    }

    public IAcquisitionLimitedByTime CreateAcquisitionLimitedByDuration(TimeSpan duration)
    {
      return new HeliosAcquisitionLimitedByTime();
    }

    public IAcquisitionMethodRun CreateMethodAcquisition(string methodFileName)
    {
      return new HeliosAcquisitionMethodRun();
    }

    public IHeliosOffMode CreateOffMode()
    {
      return new HeliosOffModeVMS();
    }

    public IHeliosOnMode CreateOnMode()
    {
      return new HeliosOnModeVMS();
    }

    public IAcquisitionWorkflow CreatePermanentAcquisition()
    {
      return new HeliosAcquisitionWorkflow();
    }

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

    public void SetMode(IHeliosMode newMode)
    {

    }

    public void SetState(InstrumentState state)
    {
      ((HeliosStateVMS)State).SetSystemState(state);
      OnStateChanged(new VMSStateChangedEventArgs(State));
    }

    public void StartAcquisition(IAcquisitionWorkflow workflow)
    {
      //do nothing
    }

  }
}
