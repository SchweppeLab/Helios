using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  // Fusion-only. Every method throws Unimplemented when connected to an Exploris (or a
  // Simulated backend configured without a pump) -- callers should check
  // StatusResponse.HasSyringePump (from InstrumentService) before using this service at all.
  internal sealed class SyringePumpServiceImpl : SyringePumpService.SyringePumpServiceBase
  {
    private readonly IInstrumentGateway _gateway;

    public SyringePumpServiceImpl(IInstrumentGateway gateway) => _gateway = gateway;

    private ISyringePumpChannel Pump =>
      _gateway.SyringePump ?? throw new RpcException(new Status(StatusCode.Unimplemented, "Connected instrument has no syringe pump."));

    public override Task<Empty> Start(Empty request, ServerCallContext context)
    {
      Pump.Start();
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> Stop(Empty request, ServerCallContext context)
    {
      Pump.Stop();
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> Toggle(Empty request, ServerCallContext context)
    {
      Pump.Toggle();
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> SetDiameter(SetSyringeValueRequest request, ServerCallContext context)
    {
      Pump.SetDiameter(request.Value);
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> SetVolume(SetSyringeValueRequest request, ServerCallContext context)
    {
      Pump.SetVolume(request.Value);
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> SetFlowRate(SetSyringeValueRequest request, ServerCallContext context)
    {
      Pump.SetFlowRate(request.Value);
      return Task.FromResult(new Empty());
    }

    public override async Task StreamSyringeStatus(Empty request, IServerStreamWriter<SyringeStatusEvent> responseStream, ServerCallContext context)
    {
      var pump = Pump;
      EventHandler? onStatusChanged = null;
      EventHandler? onParameterValueChanged = null;

      await GrpcStreaming.PumpAsync<SyringeStatusEvent>(
        subscribe: write =>
        {
          onStatusChanged = (_, _) => write(Snapshot(pump));
          onParameterValueChanged = (_, _) => write(Snapshot(pump));
          pump.StatusChanged += onStatusChanged;
          pump.ParameterValueChanged += onParameterValueChanged;
          write(Snapshot(pump));
        },
        unsubscribe: () =>
        {
          pump.StatusChanged -= onStatusChanged;
          pump.ParameterValueChanged -= onParameterValueChanged;
        },
        responseStream: responseStream,
        context: context).ConfigureAwait(false);
    }

    private static SyringeStatusEvent Snapshot(ISyringePumpChannel pump) => new()
    {
      Diameter = pump.Diameter,
      Volume = pump.Volume,
      FlowRate = pump.FlowRate,
      Status = EnumConversions.ToProto(pump.Status),
    };
  }
}
