extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow;
using Nova.IPC.Pipes;
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

    // Read from IAPI on every get: IAPI keeps these current, a copy taken at construction would not be.
    public IHeliosState State => new HeliosStateExploris(acquisition.State);
    public bool CanPause => acquisition.CanPause;
    public bool CanResume => acquisition.CanResume;

    public HeliosAcquisitionExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c)
    {
      acquisition = c.Acquisition;
      acquisition.AcquisitionStreamClosing += AcquisitionStreamClosingExploris;
      acquisition.AcquisitionStreamOpening += AcquisitionStreamOpeningExploris;
      acquisition.StateChanged += OnStateChanged;
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

    // The IAPI reads the requested mode from the object's type (IOnMode, IOffMode), so the object
    // handed to it must come from its own Create* call.
    public void SetMode(IHeliosMode newMode)
    {
      exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode mode;
      if (newMode is IHeliosOnMode) mode = acquisition.CreateOnMode();
      else if (newMode is IHeliosOffMode) mode = acquisition.CreateOffMode();
      else throw new FormatException("Unknown mode type; use CreateOnMode or CreateOffMode.");
      foreach (KeyValuePair<string, string> kvp in newMode.AdditionalValues) mode.AdditionalValues[kvp.Key] = kvp.Value;
      acquisition.SetMode(mode);
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

    // Read from IAPI on every get: IAPI keeps these current, a copy taken at construction would not be.
    public IHeliosState State => new HeliosStateFusion(acquisition.State);
    public bool CanPause => acquisition.CanPause;
    public bool CanResume => acquisition.CanResume;


    public HeliosAcquisitionFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
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

    // The IAPI reads the requested mode from the object's type (IOnMode, IOffMode), so the object
    // handed to it must come from its own Create* call.
    public void SetMode(IHeliosMode newMode)
    {
      Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes.IMode mode;
      if (newMode is IHeliosOnMode) mode = acquisition.CreateOnMode();
      else if (newMode is IHeliosOffMode) mode = acquisition.CreateOffMode();
      else throw new FormatException("Unknown mode type; use CreateOnMode or CreateOffMode.");
      foreach (KeyValuePair<string, string> kvp in newMode.AdditionalValues) mode.AdditionalValues[kvp.Key] = kvp.Value;
      acquisition.SetMode(mode);
    }

    // The IAPI reads the kind of acquisition from the object's type and expects objects its own
    // Create* calls return, so build one of the same kind and copy the values in.
    public void StartAcquisition(IAcquisitionWorkflow workflow)
    {
      Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionWorkflow f;
      if (workflow is IAcquisitionMethodRun m)
      {
        Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionMethodRun fm = acquisition.CreateMethodAcquisition(m.MethodName);
        fm.Duration = m.Duration;
        f = fm;
      }
      else if (workflow is IAcquisitionLimitedByCount c)
      {
        f = acquisition.CreateAcquisitionLimitedByCount(c.NumberOfScans);
      }
      else if (workflow is IAcquisitionLimitedByTime t)
      {
        f = acquisition.CreateAcquisitionLimitedByDuration(t.Duration);
      }
      else
      {
        f = acquisition.CreatePermanentAcquisition();
      }
      f.RawFileName = workflow.RawFileName;
      f.Continuation = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation)workflow.Continuation;
      f.SampleName = workflow.SampleName;
      f.Comment = workflow.Comment;
      f.SingleProcessingDelay = workflow.SingleProcessingDelay;
      f.WaitForContactClosure = workflow.WaitForContactClosure;
      acquisition.StartAcquisition(f);
    }

    void StateChangedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.StateChangedEventArgs e)
        {
      StateChangedEventArgs args = new FusionStateChangedEventArgs(e);
            OnStateChanged(args);
        }
    }

  /// <summary>
  /// Acquisition control of the Corona (VMS) backend. State comes from Corona's state reports
  /// (pipe message '6'); mode changes, acquisition starts and cancels go to Corona as pipe
  /// messages 'M', 'S' and 'C'. Payloads are "Key=Value" lines. See docs/corona-pipe-protocol.md.
  /// </summary>
  internal class HeliosAcquisitionVMS : IAcquisition
  {
    public event EventHandler<StateChangedEventArgs> StateChanged;
    public event EventHandler AcquisitionStreamClosing;
    public event EventHandler<AcquisitionOpeningEventArgs> AcquisitionStreamOpening;

    private readonly object stateLock = new object();
    private readonly HeliosStateVMS current = new HeliosStateVMS();
    private bool canPause = true;
    private bool canResume = true;
    // Set by the first state report. Until then the stream messages set Running/ReadyForRun,
    // which keeps Corona builds that do not report state working as before.
    private bool reported = false;
    private readonly PipesClient pipesClient;

    public HeliosAcquisitionVMS(PipesClient pc = null)
    {
      pipesClient = pc;
    }

    /// <summary>A copy of the state at the time of the call.</summary>
    public IHeliosState State { get { lock (stateLock) return new HeliosStateVMS(current); } }
    public bool CanPause { get { lock (stateLock) return canPause; } }
    public bool CanResume { get { lock (stateLock) return canResume; } }

    /// <summary>
    /// Applies a state report from Corona. Keys: Mode (SystemMode name), State (InstrumentState
    /// name), CanPause, CanResume. Missing keys leave their field unchanged.
    /// </summary>
    internal void ApplyStateReport(string payload)
    {
      SystemMode? mode = null;
      InstrumentState? state = null;
      bool? pause = null;
      bool? resume = null;
      foreach (KeyValuePair<string, string> kvp in ParseLines(payload))
      {
        switch (kvp.Key)
        {
          case "Mode": if (Enum.TryParse(kvp.Value, true, out SystemMode m)) mode = m; break;
          case "State": if (Enum.TryParse(kvp.Value, true, out InstrumentState s)) state = s; break;
          case "CanPause": if (bool.TryParse(kvp.Value, out bool p)) pause = p; break;
          case "CanResume": if (bool.TryParse(kvp.Value, out bool r)) resume = r; break;
        }
      }
      if (mode == null && state == null)
      {
        Console.WriteLine("Corona state report without a readable Mode or State: {0}", payload);
        return;
      }
      HeliosStateVMS snapshot;
      lock (stateLock)
      {
        reported = true;
        if (mode.HasValue) current.SystemMode = mode.Value;
        if (state.HasValue) current.SystemState = state.Value;
        if (pause.HasValue) canPause = pause.Value;
        if (resume.HasValue) canResume = resume.Value;
        snapshot = new HeliosStateVMS(current);
      }
      OnStateChanged(new VMSStateChangedEventArgs(snapshot));
    }

    /// <summary>
    /// State inferred from the stream-opening and stream-closing messages. Ignored once Corona
    /// has reported state itself.
    /// </summary>
    internal void SetLegacyState(InstrumentState state)
    {
      HeliosStateVMS snapshot;
      lock (stateLock)
      {
        if (reported || current.SystemState == state) return;
        current.SystemState = state;
        snapshot = new HeliosStateVMS(current);
      }
      OnStateChanged(new VMSStateChangedEventArgs(snapshot));
    }

    /// <summary>
    /// The pipe to Corona connected or dropped. A drop is Disconnected/ServerFailed, as the IAPI
    /// reports a lost service link. A connect before any state report is On/ReadyForRun; a
    /// reporting Corona sends its state right after the connect.
    /// </summary>
    internal void SetServiceLink(bool connected)
    {
      HeliosStateVMS snapshot;
      lock (stateLock)
      {
        if (connected)
        {
          if (reported) return;
          current.SystemMode = SystemMode.On;
          current.SystemState = InstrumentState.ReadyForRun;
        }
        else
        {
          current.SystemMode = SystemMode.Disconnected;
          current.SystemState = InstrumentState.ServerFailed;
        }
        snapshot = new HeliosStateVMS(current);
      }
      OnStateChanged(new VMSStateChangedEventArgs(snapshot));
    }

    public void CancelAcquisition()
    {
      Send('C', "");
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
      string mode = newMode is IHeliosOnMode ? "On" : newMode is IHeliosOffMode ? "Off" : null;
      if (mode == null) throw new FormatException("Unknown mode type; use CreateOnMode or CreateOffMode.");
      Send('M', "Mode=" + mode);
    }

    public void StartAcquisition(IAcquisitionWorkflow workflow)
    {
      if (workflow == null) throw new ArgumentNullException(nameof(workflow));
      Send('S', SerializeWorkflow(workflow));
    }

    private static string SerializeWorkflow(IAcquisitionWorkflow w)
    {
      StringBuilder sb = new StringBuilder();
      CultureInfo inv = CultureInfo.InvariantCulture;
      IAcquisitionLimitedByCount byCount = w as IAcquisitionLimitedByCount;
      IAcquisitionLimitedByTime byTime = w as IAcquisitionLimitedByTime;
      IAcquisitionMethodRun method = w as IAcquisitionMethodRun;
      string kind = byCount != null ? "LimitedByCount" : byTime != null ? "LimitedByTime" : method != null ? "MethodRun" : "Permanent";
      sb.Append("Kind=").Append(kind).Append('\n');
      sb.Append("RawFileName=").Append(w.RawFileName).Append('\n');
      sb.Append("SampleName=").Append(w.SampleName).Append('\n');
      sb.Append("Comment=").Append(w.Comment).Append('\n');
      sb.Append("Continuation=").Append(w.Continuation).Append('\n');
      sb.Append("SingleProcessingDelay=").Append(w.SingleProcessingDelay.ToString(inv)).Append('\n');
      sb.Append("WaitForContactClosure=").Append(w.WaitForContactClosure).Append('\n');
      if (byCount != null) sb.Append("NumberOfScans=").Append(byCount.NumberOfScans.ToString(inv)).Append('\n');
      if (byTime != null) sb.Append("Duration=").Append(byTime.Duration.ToString("c", inv)).Append('\n');
      if (method != null)
      {
        sb.Append("MethodName=").Append(method.MethodName).Append('\n');
        if (method.Duration.HasValue) sb.Append("Duration=").Append(method.Duration.Value.ToString("c", inv)).Append('\n');
      }
      return sb.ToString();
    }

    private void Send(char code, string payload)
    {
      if (pipesClient == null) return;
      PipeMessage pm = new PipeMessage();
      pm.EncodeString(payload);
      pm.MsgCode = code; // after EncodeString, which resets the code to '0'
      pipesClient.Send(pm);
    }

    /// <summary>Splits "Key=Value" lines; a line without '=' is skipped.</summary>
    internal static IEnumerable<KeyValuePair<string, string>> ParseLines(string payload)
    {
      foreach (string line in (payload ?? "").Split('\n'))
      {
        int eq = line.IndexOf('=');
        if (eq <= 0) continue;
        yield return new KeyValuePair<string, string>(line.Substring(0, eq).Trim(), line.Substring(eq + 1).Trim());
      }
    }

  }
}
