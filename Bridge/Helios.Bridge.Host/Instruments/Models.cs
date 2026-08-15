using System;
using System.Collections.Generic;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Bridge.Host.Instruments
{
  // Mirrors Helios.Interfaces.InstrumentAccess.Control.Acquisition.SystemMode member-for-member.
  // Kept as a host-local enum (rather than reusing Helios's directly) so this whole namespace
  // stays free of any Helios.dll/IAPI dependency -- mapping to Helios.Bridge.Contracts happens only
  // in the Services layer, and mapping from Helios's own enum happens only in the gateway adapter.
  public enum SystemMode
  {
    Malconfigured,
    Disconnected,
    On,
    Standby,
    Off,
    RemoteControl,
    DirectControl,
    Maintenance,
    Calibration,
    Evaluation,
    Bakeout,
    AutoTune,
    CalibrationPaused,
    EvaluationPaused,
    BakeoutPaused,
    AutoTunePaused,
    DirectControlPaused,
  }

  // Mirrors Helios.Interfaces.InstrumentAccess.Control.Acquisition.InstrumentState.
  public enum InstrumentState
  {
    Initializing,
    ReadyToDownload,
    Downloading,
    PreparingForRun,
    ReadyForRun,
    WaitingForContactClosure,
    Running,
    PostRun,
    Error,
    Busy,
    NotConnected,
    StandBy,
    Off,
    ServerFailed,
    LampWarmup,
    NotReady,
    DirectControl,
  }

  public enum SyringePumpStatus
  {
    On,
    Off,
    Error,
    NotConnected,
    LimitReached,
  }

  public enum WorkflowKind
  {
    Permanent,
    CountLimited,
    DurationLimited,
    Method,
  }

  public sealed class AcquisitionStateSnapshot
  {
    public SystemMode Mode { get; init; }
    public InstrumentState State { get; init; }
  }

  public sealed class AcquisitionWorkflowRequest
  {
    public WorkflowKind Kind { get; init; }
    public string RawFileName { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public double SingleProcessingDelay { get; init; }
    public bool WaitForContactClosure { get; init; }
    public int ScanCount { get; init; }
    public TimeSpan Duration { get; init; }
    public string MethodFileName { get; init; } = string.Empty;
  }

  public sealed class ScanParameterDescriptor
  {
    public string Name { get; init; } = string.Empty;
    public string Selection { get; init; } = string.Empty;
    public string DefaultValue { get; init; } = string.Empty;
    public string Help { get; init; } = string.Empty;
  }

  public sealed class CustomScan
  {
    public long RunningNumber { get; set; } = 1;
    public double SingleProcessingDelay { get; set; }
    public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();
    public bool IsPagcScan { get; set; }
    public long PagcGroupIndex { get; set; }
  }

  public sealed class RepeatingScan
  {
    public long RunningNumber { get; set; } = 1;
    public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();
  }

  public sealed class InstrumentMessage
  {
    public int InstrumentId { get; init; }
    public string InstrumentName { get; init; } = string.Empty;
    public uint MessageId { get; init; }
    public DateTime CreationTimeUtc { get; init; }
    public int Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> MessageArgs { get; init; } = Array.Empty<string>();
  }

  public sealed class MessagesArrivedEventArgs : EventArgs
  {
    public IReadOnlyList<InstrumentMessage> Messages { get; }

    public MessagesArrivedEventArgs(IReadOnlyList<InstrumentMessage> messages) => Messages = messages;
  }

  public sealed class ContactClosureChangedEventArgs : EventArgs
  {
    public int RisingEdges { get; init; }
    public int FallingEdges { get; init; }
  }

  // Carries the wire-shaped Contracts.MsScanData directly rather than a host-local snapshot DTO --
  // the gateway builds it straight from the raw backend data (real IAPI or Simulated), and
  // Services/ScanStreamServiceImpl ships it out unmodified. Deliberately breaks this namespace's
  // usual "no proto types cross this boundary" rule (see IInstrumentGateway.cs), specifically for
  // the scan-streaming hot path -- see HeliosInstrumentGateway.cs's ToProto for why.
  public sealed class MsScanEventArgs : EventArgs
  {
    public Contracts.MsScanData Scan { get; }

    public MsScanEventArgs(Contracts.MsScanData scan) => Scan = scan;
  }

  public sealed class AcquisitionStateChangedEventArgs : EventArgs
  {
    public AcquisitionStateSnapshot State { get; }

    public AcquisitionStateChangedEventArgs(AcquisitionStateSnapshot state) => State = state;
  }

  public sealed class AcquisitionStreamOpeningEventArgs : EventArgs
  {
    public IReadOnlyDictionary<string, string> StartingInformation { get; init; } = new Dictionary<string, string>();
  }

  public sealed class NumOpenCustomScanSlotsEventArgs : EventArgs
  {
    public int NumOpenCustomScanSlots { get; init; }
  }

  public sealed class AnalogTracePointEventArgs : EventArgs
  {
    public double Value { get; init; }

    // Helios/IAPI reports this as a TimeSpan (offset since acquisition start), not an absolute
    // timestamp.
    public TimeSpan Occurrence { get; init; }
  }

  public sealed class AnalogTraceInfo
  {
    public string DetectorClass { get; init; } = string.Empty;
    public double Minimum { get; init; }
    public double Maximum { get; init; }
    public double? UpdateFrequencyHz { get; init; }
  }
}
