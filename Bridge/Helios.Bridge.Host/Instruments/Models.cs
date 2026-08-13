using System;
using System.Collections.Generic;

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

  // Structure-of-arrays centroid block -- see Helios.Bridge.Contracts' scans.proto for why this
  // shape was chosen over one object per centroid. Built once per scan by the gateway and handed
  // straight through to the gRPC layer for packing into CentroidBlock.
  public sealed class CentroidBlock
  {
    public static readonly CentroidBlock Empty = new()
    {
      Mz = Array.Empty<double>(),
      Intensity = Array.Empty<double>(),
      Charge = Array.Empty<int>(),
      Resolution = Array.Empty<double>(),
      IsExceptional = Array.Empty<bool>(),
      IsFragmented = Array.Empty<bool>(),
      IsMerged = Array.Empty<bool>(),
      IsReferenced = Array.Empty<bool>(),
    };

    public double[] Mz { get; init; } = Array.Empty<double>();
    public double[] Intensity { get; init; } = Array.Empty<double>();
    public int[] Charge { get; init; } = Array.Empty<int>();
    public double[] Resolution { get; init; } = Array.Empty<double>();
    public bool[] IsExceptional { get; init; } = Array.Empty<bool>();
    public bool[] IsFragmented { get; init; } = Array.Empty<bool>();
    public bool[] IsMerged { get; init; } = Array.Empty<bool>();
    public bool[] IsReferenced { get; init; } = Array.Empty<bool>();
  }

  public sealed class MsScanSnapshot
  {
    public string DetectorName { get; init; } = string.Empty;
    public DateTime ArrivalTimeUtc { get; init; }
    public IReadOnlyDictionary<string, string> Header { get; init; } = EmptyMap;
    public IReadOnlyDictionary<string, string> Trailer { get; init; } = EmptyMap;
    public IReadOnlyDictionary<string, string> StatusLog { get; init; } = EmptyMap;
    public IReadOnlyDictionary<string, string> TuneData { get; init; } = EmptyMap;
    public bool HasProfileInformation { get; init; }
    public CentroidBlock Centroids { get; init; } = CentroidBlock.Empty;

    private static readonly IReadOnlyDictionary<string, string> EmptyMap = new Dictionary<string, string>();
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

  public sealed class MsScanEventArgs : EventArgs
  {
    public MsScanSnapshot Scan { get; }

    public MsScanEventArgs(MsScanSnapshot scan) => Scan = scan;
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
