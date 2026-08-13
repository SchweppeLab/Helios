using System;
using System.Collections.Generic;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  // Everything that turns generated proto types into the plain-C# types the public surface
  // exposes -- no generated proto type crosses a public member on this library. Loop-based rather
  // than LINQ on the scan-mapping path in particular, since it sits right after the wire for the
  // highest-frequency data in the system.
  internal static class Mapping
  {
    public static InstrumentFamily ToClient(Contracts.InstrumentFamily f) => f switch
    {
      Contracts.InstrumentFamily.Fusion => InstrumentFamily.Fusion,
      Contracts.InstrumentFamily.Exploris => InstrumentFamily.Exploris,
      Contracts.InstrumentFamily.Simulated => InstrumentFamily.Simulated,
      _ => InstrumentFamily.Unspecified,
    };

    public static SystemMode ToClient(Contracts.SystemMode m) => m switch
    {
      Contracts.SystemMode.Malconfigured => SystemMode.Malconfigured,
      Contracts.SystemMode.Disconnected => SystemMode.Disconnected,
      Contracts.SystemMode.On => SystemMode.On,
      Contracts.SystemMode.Standby => SystemMode.Standby,
      Contracts.SystemMode.Off => SystemMode.Off,
      Contracts.SystemMode.RemoteControl => SystemMode.RemoteControl,
      Contracts.SystemMode.DirectControl => SystemMode.DirectControl,
      Contracts.SystemMode.Maintenance => SystemMode.Maintenance,
      Contracts.SystemMode.Calibration => SystemMode.Calibration,
      Contracts.SystemMode.Evaluation => SystemMode.Evaluation,
      Contracts.SystemMode.Bakeout => SystemMode.Bakeout,
      Contracts.SystemMode.AutoTune => SystemMode.AutoTune,
      Contracts.SystemMode.CalibrationPaused => SystemMode.CalibrationPaused,
      Contracts.SystemMode.EvaluationPaused => SystemMode.EvaluationPaused,
      Contracts.SystemMode.BakeoutPaused => SystemMode.BakeoutPaused,
      Contracts.SystemMode.AutoTunePaused => SystemMode.AutoTunePaused,
      Contracts.SystemMode.DirectControlPaused => SystemMode.DirectControlPaused,
      _ => SystemMode.Disconnected,
    };

    public static InstrumentState ToClient(Contracts.InstrumentState s) => s switch
    {
      Contracts.InstrumentState.Initializing => InstrumentState.Initializing,
      Contracts.InstrumentState.ReadyToDownload => InstrumentState.ReadyToDownload,
      Contracts.InstrumentState.Downloading => InstrumentState.Downloading,
      Contracts.InstrumentState.PreparingForRun => InstrumentState.PreparingForRun,
      Contracts.InstrumentState.ReadyForRun => InstrumentState.ReadyForRun,
      Contracts.InstrumentState.WaitingForContactClosure => InstrumentState.WaitingForContactClosure,
      Contracts.InstrumentState.Running => InstrumentState.Running,
      Contracts.InstrumentState.PostRun => InstrumentState.PostRun,
      Contracts.InstrumentState.Error => InstrumentState.Error,
      Contracts.InstrumentState.Busy => InstrumentState.Busy,
      Contracts.InstrumentState.NotConnected => InstrumentState.NotConnected,
      Contracts.InstrumentState.StandBy => InstrumentState.StandBy,
      Contracts.InstrumentState.InstrumentOff => InstrumentState.Off,
      Contracts.InstrumentState.ServerFailed => InstrumentState.ServerFailed,
      Contracts.InstrumentState.LampWarmup => InstrumentState.LampWarmup,
      Contracts.InstrumentState.NotReady => InstrumentState.NotReady,
      Contracts.InstrumentState.InstrumentDirectControl => InstrumentState.DirectControl,
      _ => InstrumentState.NotConnected,
    };

    public static SyringePumpStatus ToClient(Contracts.SyringePumpStatus s) => s switch
    {
      Contracts.SyringePumpStatus.SyringeOn => SyringePumpStatus.On,
      Contracts.SyringePumpStatus.SyringeOff => SyringePumpStatus.Off,
      Contracts.SyringePumpStatus.SyringeError => SyringePumpStatus.Error,
      Contracts.SyringePumpStatus.SyringeNotConnected => SyringePumpStatus.NotConnected,
      Contracts.SyringePumpStatus.SyringeLimitReached => SyringePumpStatus.LimitReached,
      _ => SyringePumpStatus.Unspecified,
    };

    public static AcquisitionStateSnapshot ToClient(Contracts.StateChangedEvent e) =>
      new() { Mode = ToClient(e.SystemMode), State = ToClient(e.SystemState) };

    public static IMsScan ToClient(Contracts.MsScanData d) => new SnapshotMsScan
    {
      DetectorName = d.DetectorName,
      ArrivalTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(d.ArrivalTimeUnixMs),
      Header = CopyMap(d.Header),
      Trailer = CopyMap(d.Trailer),
      StatusLog = CopyMap(d.StatusLog),
      TuneData = CopyMap(d.TuneData),
      HasProfileInformation = d.HasProfileInformation,
      Centroids = ToClient(d.Centroids),
    };

    private static CentroidBlock ToClient(Contracts.CentroidBlock? c)
    {
      if (c is null) return CentroidBlock.Empty;

      return new CentroidBlock
      {
        Mz = CopyArray(c.Mz),
        Intensity = CopyArray(c.Intensity),
        Charge = CopyArray(c.Charge),
        Resolution = CopyArray(c.Resolution),
        IsExceptional = CopyArray(c.IsExceptional),
        IsFragmented = CopyArray(c.IsFragmented),
        IsMerged = CopyArray(c.IsMerged),
        IsReferenced = CopyArray(c.IsReferenced),
      };
    }

    private static T[] CopyArray<T>(Google.Protobuf.Collections.RepeatedField<T> field)
    {
      var array = new T[field.Count];
      field.CopyTo(array, 0);
      return array;
    }

    private static Dictionary<string, string> CopyMap(Google.Protobuf.Collections.MapField<string, string> field)
    {
      var result = new Dictionary<string, string>(field.Count);
      foreach (var kv in field) result[kv.Key] = kv.Value;
      return result;
    }

    public static ScanParameterDescriptor[] ToClient(Google.Protobuf.Collections.RepeatedField<Contracts.ScanParameterDescriptor> source)
    {
      var result = new ScanParameterDescriptor[source.Count];
      for (int i = 0; i < source.Count; i++)
      {
        result[i] = new ScanParameterDescriptor { Name = source[i].Name, Selection = source[i].Selection, DefaultValue = source[i].DefaultValue, Help = source[i].Help };
      }
      return result;
    }

    public static IReadOnlyList<InstrumentMessage> ToClient(Google.Protobuf.Collections.RepeatedField<Contracts.InstrumentMessage> source)
    {
      var result = new InstrumentMessage[source.Count];
      for (int i = 0; i < source.Count; i++)
      {
        var m = source[i];
        var args = new string[m.MessageArgs.Count];
        for (int j = 0; j < m.MessageArgs.Count; j++) args[j] = m.MessageArgs[j];
        result[i] = new InstrumentMessage
        {
          InstrumentId = m.InstrumentId,
          InstrumentName = m.InstrumentName,
          MessageId = m.MessageId,
          CreationTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(m.CreationTimeUnixMs),
          Status = m.Status,
          Message = m.Message,
          MessageArgs = args,
        };
      }
      return result;
    }

    public static Contracts.CustomScanRequest ToProto(CustomScan scan)
    {
      var proto = new Contracts.CustomScanRequest
      {
        RunningNumber = scan.RunningNumber,
        SingleProcessingDelay = scan.SingleProcessingDelay,
        IsPagcScan = scan.IsPagcScan,
        PagcGroupIndex = scan.PagcGroupIndex,
      };
      foreach (var kv in scan.Values) proto.Values[kv.Key] = kv.Value;
      return proto;
    }

    public static Contracts.RepeatingScanRequest ToProto(RepeatingScan scan)
    {
      var proto = new Contracts.RepeatingScanRequest { RunningNumber = scan.RunningNumber };
      foreach (var kv in scan.Values) proto.Values[kv.Key] = kv.Value;
      return proto;
    }

    public static Contracts.StartAcquisitionRequest ToProto(AcquisitionWorkflowRequest request)
    {
      var proto = new Contracts.StartAcquisitionRequest
      {
        RawFileName = request.RawFileName,
        Comment = request.Comment,
        SingleProcessingDelay = request.SingleProcessingDelay,
        WaitForContactClosure = request.WaitForContactClosure,
      };
      switch (request.Kind)
      {
        case WorkflowKind.CountLimited:
          proto.CountLimited = new Contracts.CountLimitedWorkflow { ScanCount = request.ScanCount };
          break;
        case WorkflowKind.DurationLimited:
          proto.DurationLimited = new Contracts.DurationLimitedWorkflow { DurationMs = (long)request.Duration.TotalMilliseconds };
          break;
        case WorkflowKind.Method:
          proto.Method = new Contracts.MethodWorkflow { MethodFileName = request.MethodFileName };
          break;
        default:
          proto.Permanent = new Contracts.PermanentWorkflow();
          break;
      }
      return proto;
    }
  }

  internal sealed class SnapshotMsScan : IMsScan
  {
    public string DetectorName { get; init; } = string.Empty;
    public DateTimeOffset ArrivalTimeUtc { get; init; }
    public IReadOnlyDictionary<string, string> Header { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> Trailer { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> StatusLog { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> TuneData { get; init; } = new Dictionary<string, string>();
    public bool HasProfileInformation { get; init; }
    public CentroidBlock Centroids { get; init; } = CentroidBlock.Empty;
  }
}
