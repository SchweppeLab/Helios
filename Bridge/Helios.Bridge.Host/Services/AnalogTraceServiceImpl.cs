using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  internal sealed class AnalogTraceServiceImpl : AnalogTraceService.AnalogTraceServiceBase
  {
    private readonly IInstrumentGateway _gateway;

    public AnalogTraceServiceImpl(IInstrumentGateway gateway) => _gateway = gateway;

    private IAnalogTraceChannel Channel(int index) =>
      _gateway.GetAnalogTraceContainer(index) ?? throw new RpcException(new Status(StatusCode.NotFound, $"No analog trace channel {index}."));

    public override Task<AnalogTraceInfoResponse> GetAnalogTraceInfo(AnalogTraceRequest request, ServerCallContext context)
    {
      var info = Channel(request.Channel).Info;
      var response = new AnalogTraceInfoResponse
      {
        DetectorClass = info.DetectorClass,
        Minimum = info.Minimum,
        Maximum = info.Maximum,
      };
      if (info.UpdateFrequencyHz.HasValue) response.UpdateFrequencyHz = info.UpdateFrequencyHz.Value;
      return Task.FromResult(response);
    }

    public override async Task StreamAnalogTrace(AnalogTraceRequest request, IServerStreamWriter<AnalogTracePointEvent> responseStream, ServerCallContext context)
    {
      var channel = Channel(request.Channel);
      EventHandler<AnalogTracePointEventArgs>? onPointArrived = null;

      await GrpcStreaming.PumpAsync<AnalogTracePointEvent>(
        subscribe: write =>
        {
          onPointArrived = (_, e) => write(new AnalogTracePointEvent { Value = e.Value, OccurrenceMs = (long)e.Occurrence.TotalMilliseconds });
          channel.AnalogTracePointArrived += onPointArrived;
        },
        unsubscribe: () => channel.AnalogTracePointArrived -= onPointArrived,
        responseStream: responseStream,
        context: context).ConfigureAwait(false);
    }
  }
}
