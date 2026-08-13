using Helios.Bridge.Contracts;
using Instruments = Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  // Host-local enums (Instruments/Models.cs) mirror Helios.Interfaces.InstrumentAccess's own
  // member names; proto enums use UPPER_SNAKE_CASE (protobuf style guide). Names don't line up
  // mechanically, so this is an explicit table rather than a reflection-based mapper.
  internal static class EnumConversions
  {
    public static InstrumentFamily ToProto(string hostFamily) => hostFamily switch
    {
      "Fusion" => InstrumentFamily.Fusion,
      "Exploris" => InstrumentFamily.Exploris,
      "Simulated" => InstrumentFamily.Simulated,
      _ => InstrumentFamily.Unspecified,
    };

    public static SystemMode ToProto(Instruments.SystemMode mode) => mode switch
    {
      Instruments.SystemMode.Malconfigured => SystemMode.Malconfigured,
      Instruments.SystemMode.Disconnected => SystemMode.Disconnected,
      Instruments.SystemMode.On => SystemMode.On,
      Instruments.SystemMode.Standby => SystemMode.Standby,
      Instruments.SystemMode.Off => SystemMode.Off,
      Instruments.SystemMode.RemoteControl => SystemMode.RemoteControl,
      Instruments.SystemMode.DirectControl => SystemMode.DirectControl,
      Instruments.SystemMode.Maintenance => SystemMode.Maintenance,
      Instruments.SystemMode.Calibration => SystemMode.Calibration,
      Instruments.SystemMode.Evaluation => SystemMode.Evaluation,
      Instruments.SystemMode.Bakeout => SystemMode.Bakeout,
      Instruments.SystemMode.AutoTune => SystemMode.AutoTune,
      Instruments.SystemMode.CalibrationPaused => SystemMode.CalibrationPaused,
      Instruments.SystemMode.EvaluationPaused => SystemMode.EvaluationPaused,
      Instruments.SystemMode.BakeoutPaused => SystemMode.BakeoutPaused,
      Instruments.SystemMode.AutoTunePaused => SystemMode.AutoTunePaused,
      Instruments.SystemMode.DirectControlPaused => SystemMode.DirectControlPaused,
      _ => SystemMode.Unspecified,
    };

    public static InstrumentState ToProto(Instruments.InstrumentState state) => state switch
    {
      Instruments.InstrumentState.Initializing => InstrumentState.Initializing,
      Instruments.InstrumentState.ReadyToDownload => InstrumentState.ReadyToDownload,
      Instruments.InstrumentState.Downloading => InstrumentState.Downloading,
      Instruments.InstrumentState.PreparingForRun => InstrumentState.PreparingForRun,
      Instruments.InstrumentState.ReadyForRun => InstrumentState.ReadyForRun,
      Instruments.InstrumentState.WaitingForContactClosure => InstrumentState.WaitingForContactClosure,
      Instruments.InstrumentState.Running => InstrumentState.Running,
      Instruments.InstrumentState.PostRun => InstrumentState.PostRun,
      Instruments.InstrumentState.Error => InstrumentState.Error,
      Instruments.InstrumentState.Busy => InstrumentState.Busy,
      Instruments.InstrumentState.NotConnected => InstrumentState.NotConnected,
      Instruments.InstrumentState.StandBy => InstrumentState.StandBy,
      Instruments.InstrumentState.Off => InstrumentState.InstrumentOff,
      Instruments.InstrumentState.ServerFailed => InstrumentState.ServerFailed,
      Instruments.InstrumentState.LampWarmup => InstrumentState.LampWarmup,
      Instruments.InstrumentState.NotReady => InstrumentState.NotReady,
      Instruments.InstrumentState.DirectControl => InstrumentState.InstrumentDirectControl,
      _ => InstrumentState.Unspecified,
    };

    public static SyringePumpStatus ToProto(Instruments.SyringePumpStatus status) => status switch
    {
      Instruments.SyringePumpStatus.On => SyringePumpStatus.SyringeOn,
      Instruments.SyringePumpStatus.Off => SyringePumpStatus.SyringeOff,
      Instruments.SyringePumpStatus.Error => SyringePumpStatus.SyringeError,
      Instruments.SyringePumpStatus.NotConnected => SyringePumpStatus.SyringeNotConnected,
      Instruments.SyringePumpStatus.LimitReached => SyringePumpStatus.SyringeLimitReached,
      _ => SyringePumpStatus.Unspecified,
    };

    public static Instruments.WorkflowKind ToHost(StartAcquisitionRequest.WorkflowOneofCase workflowCase) => workflowCase switch
    {
      StartAcquisitionRequest.WorkflowOneofCase.CountLimited => Instruments.WorkflowKind.CountLimited,
      StartAcquisitionRequest.WorkflowOneofCase.DurationLimited => Instruments.WorkflowKind.DurationLimited,
      StartAcquisitionRequest.WorkflowOneofCase.Method => Instruments.WorkflowKind.Method,
      _ => Instruments.WorkflowKind.Permanent,
    };
  }
}
