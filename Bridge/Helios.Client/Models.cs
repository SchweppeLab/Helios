using System;
using System.Collections.Generic;

namespace Helios.Client
{
  // Mirrors Helios.Interfaces.InstrumentAccess.Control.Acquisition.SystemMode member-for-member.
  public enum SystemMode
  {
    Malconfigured = -1,
    Disconnected = 0,
    On = 1,
    Standby = 2,
    Off = 3,
    RemoteControl = 4,
    DirectControl = 5,
    Maintenance = 6,
    Calibration = 100,
    Evaluation = 101,
    Bakeout = 102,
    AutoTune = 103,
    CalibrationPaused = 200,
    EvaluationPaused = 201,
    BakeoutPaused = 202,
    AutoTunePaused = 203,
    DirectControlPaused = 305,
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
    Unspecified,
    On,
    Off,
    Error,
    NotConnected,
    LimitReached,
  }

  public enum InstrumentFamily
  {
    Unspecified,
    Fusion,
    Exploris,
    Simulated,
  }

  public enum WorkflowKind
  {
    Permanent,
    CountLimited,
    DurationLimited,
    Method,
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

  public sealed class CentroidBlock
  {
    public static readonly CentroidBlock Empty = new();

    public double[] Mz { get; init; } = Array.Empty<double>();
    public double[] Intensity { get; init; } = Array.Empty<double>();
    public int[] Charge { get; init; } = Array.Empty<int>();
    public double[] Resolution { get; init; } = Array.Empty<double>();
    public bool[] IsExceptional { get; init; } = Array.Empty<bool>();
    public bool[] IsFragmented { get; init; } = Array.Empty<bool>();
    public bool[] IsMerged { get; init; } = Array.Empty<bool>();
    public bool[] IsReferenced { get; init; } = Array.Empty<bool>();
  }

  public sealed class InstrumentMessage
  {
    public int InstrumentId { get; init; }
    public string InstrumentName { get; init; } = string.Empty;
    public uint MessageId { get; init; }
    public DateTimeOffset CreationTimeUtc { get; init; }
    public int Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> MessageArgs { get; init; } = Array.Empty<string>();
  }

  public sealed class MessagesArrivedEventArgs : EventArgs
  {
    public IReadOnlyList<InstrumentMessage> Messages { get; }

    public MessagesArrivedEventArgs(IReadOnlyList<InstrumentMessage> messages) => Messages = messages;
  }

  public sealed class ContactClosureEventArgs : EventArgs
  {
    public int RisingEdges { get; init; }
    public int FallingEdges { get; init; }
  }

  public sealed class AcquisitionStateSnapshot
  {
    public SystemMode Mode { get; init; }
    public InstrumentState State { get; init; }
  }

  public sealed class StateChangedEventArgs : EventArgs
  {
    public AcquisitionStateSnapshot State { get; }

    public StateChangedEventArgs(AcquisitionStateSnapshot state) => State = state;
  }

  public sealed class AcquisitionStreamOpeningEventArgs : EventArgs
  {
    public IReadOnlyDictionary<string, string> StartingInformation { get; init; } = new Dictionary<string, string>();
  }

  public sealed class AnalogTraceInfo
  {
    public string DetectorClass { get; init; } = string.Empty;
    public double Minimum { get; init; }
    public double Maximum { get; init; }
    public double? UpdateFrequencyHz { get; init; }
  }

  public sealed class AnalogTracePointEventArgs : EventArgs
  {
    public double Value { get; init; }
    public TimeSpan Occurrence { get; init; }
  }
}
