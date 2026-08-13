using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Client
{
  // These interfaces intentionally use the same names and read/event surface as Helios's own
  // Helios.Interfaces.InstrumentAccess.* (IInstrumentAccess, IControl, IAcquisition, IScans,
  // IMsScan, ...) so Core 8 application code looks like Helios application code. They are not a
  // byte-for-byte reproduction, though -- three deliberate divergences, each driven by the network
  // boundary this library adds, not by convenience:
  //
  //  1. Properties/events stay synchronous (State, PossibleParameters, StatusChanged, ...), backed
  //     by a local cache kept current by a background gRPC stream started at connect time -- exact
  //     parity with Helios's live, event-driven design.
  //  2. Genuine request/response calls that need a round trip and aren't meant to block a caller
  //     (GetInstrumentValuesAsync, GetPossibleScanParametersAsync, SubmitCustomScanAsync, ...) are
  //     Task-returning here where Helios's real interface is synchronous. Blocking the calling
  //     thread on network I/O to preserve an exactly-synchronous signature is a correctness risk
  //     (UI-thread deadlocks) for no real benefit, so this library doesn't do it. Fire-and-forget
  //     commands that IAPI itself already treats as best-effort/non-blocking (SetMode,
  //     StartAcquisition, CancelAcquisition, the syringe pump's Start/Stop/Toggle/Set*) stay
  //     synchronous void calls, matching IAPI's own "submit and watch events for the real outcome"
  //     semantics.
  //  3. IMsScan.Centroids is the columnar CentroidBlock here, not IEnumerable<ICentroid> --
  //     preserving the one-allocation-per-scan (not one-per-peak) shape that's the entire point of
  //     the wire format on this, the system's highest-frequency path.
  public interface IInstrumentAccess : IAsyncDisposable
  {
    IControl Control { get; }

    bool Connected { get; }
    int InstrumentId { get; }
    string InstrumentName { get; }
    string[] DetectorClasses { get; }
    int CountMsDetectors { get; }
    int CountAnalogChannels { get; }
    InstrumentFamily Family { get; }

    event EventHandler<EventArgs> ConnectionChanged;
    event EventHandler<ContactClosureEventArgs> ContactClosureChanged;
    event EventHandler<MessagesArrivedEventArgs> MessagesArrived;

    IMsScanContainer GetMsScanContainer(int msDetectorSet);
    IAnalogTraceContainer? GetAnalogTraceContainer(int analogDetectorSet);

    Task<IReadOnlyDictionary<string, string>> GetInstrumentValuesAsync(IReadOnlyList<string>? names = null, CancellationToken cancellationToken = default);
  }

  public interface IControl
  {
    IAcquisition Acquisition { get; }
    IScans Scans { get; }
    ISyringePumpControl? SyringePumpControl { get; }
  }

  public interface IAcquisition
  {
    IHeliosState State { get; }

    event EventHandler<StateChangedEventArgs> StateChanged;
    event EventHandler<AcquisitionStreamOpeningEventArgs> AcquisitionStreamOpening;
    event EventHandler AcquisitionStreamClosing;

    void SetMode(bool on);
    void StartAcquisition(AcquisitionWorkflowRequest request);
    void CancelAcquisition();
  }

  public interface IHeliosState
  {
    InstrumentState SystemState { get; }
    SystemMode SystemMode { get; }
  }

  public interface IScans
  {
    ScanParameterDescriptor[] PossibleParameters { get; }

    event EventHandler CanAcceptNextCustomScan;
    event EventHandler<EventArgs> PossibleParametersChanged;

    Task<bool> SubmitCustomScanAsync(CustomScan scan, CancellationToken cancellationToken = default);
    Task<bool> SubmitRepeatingScanAsync(RepeatingScan scan, CancellationToken cancellationToken = default);
    Task<bool> CancelCustomScanAsync(CancellationToken cancellationToken = default);
    Task<bool> CancelRepetitionAsync(CancellationToken cancellationToken = default);
  }

  public interface IMsScanContainer
  {
    string DetectorClass { get; }

    event EventHandler<MsScanEventArgs> MsScanArrived;

    Task<IMsScan?> GetLastMsScanAsync(CancellationToken cancellationToken = default);
  }

  public interface IMsScan
  {
    string DetectorName { get; }
    DateTimeOffset ArrivalTimeUtc { get; }
    IReadOnlyDictionary<string, string> Header { get; }
    IReadOnlyDictionary<string, string> Trailer { get; }
    IReadOnlyDictionary<string, string> StatusLog { get; }
    IReadOnlyDictionary<string, string> TuneData { get; }
    bool HasProfileInformation { get; }
    CentroidBlock Centroids { get; }
  }

  public sealed class MsScanEventArgs : EventArgs
  {
    public IMsScan Scan { get; }

    public MsScanEventArgs(IMsScan scan) => Scan = scan;
  }

  public interface ISyringePumpControl
  {
    double Diameter { get; }
    double Volume { get; }
    double FlowRate { get; }
    SyringePumpStatus Status { get; }

    event EventHandler StatusChanged;
    event EventHandler ParameterValueChanged;

    void Start();
    void Stop();
    void Toggle();
    void SetDiameter(double diameter);
    void SetVolume(double volume);
    void SetFlowRate(double flowRate);
  }

  public interface IAnalogTraceContainer
  {
    AnalogTraceInfo Info { get; }

    event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
  }
}
