using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  // The hot path. IMsScanChannel now hands back Contracts.MsScanData directly (built once, in
  // HeliosInstrumentGateway.ToProto / SimulatedInstrumentGateway.EmitScan) -- this service is a
  // pure pass-through with no conversion step of its own, unlike the rest of the *ServiceImpl
  // classes, which each map their own host-local DTOs to proto.
  internal sealed class ScanStreamServiceImpl : ScanStreamService.ScanStreamServiceBase
  {
    private readonly IInstrumentGateway _gateway;

    public ScanStreamServiceImpl(IInstrumentGateway gateway) => _gateway = gateway;

    public override async Task StreamMsScans(StreamScansRequest request, IServerStreamWriter<MsScanData> responseStream, ServerCallContext context)
    {
      var channel = _gateway.GetMsScanContainer(request.MsDetectorSet);
      EventHandler<MsScanEventArgs>? onMsScanArrived = null;

      await GrpcStreaming.PumpAsync<MsScanData>(
        subscribe: handler =>
        {
          onMsScanArrived = (_, e) => handler(e.Scan);
          channel.MsScanArrived += onMsScanArrived;
        },
        unsubscribe: () => channel.MsScanArrived -= onMsScanArrived,
        responseStream: responseStream,
        context: context).ConfigureAwait(false);
    }

    public override Task<MsScanData> GetLastMsScan(StreamScansRequest request, ServerCallContext context)
    {
      var channel = _gateway.GetMsScanContainer(request.MsDetectorSet);
      return Task.FromResult(channel.GetLastMsScan() ?? new MsScanData());
    }
  }
}
