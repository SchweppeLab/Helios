using System;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.Collections;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  // The hot path. Kept as small and allocation-light as reasonable: one MsScanSnapshot in,
  // one MsScanData out, straight onto the stream -- no batching, no intermediate copies beyond
  // what RepeatedField<T>.AddRange already needs to do to move the arrays into the message.
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
          onMsScanArrived = (_, e) => handler(ToProto(e.Scan));
          channel.MsScanArrived += onMsScanArrived;
        },
        unsubscribe: () => channel.MsScanArrived -= onMsScanArrived,
        responseStream: responseStream,
        context: context).ConfigureAwait(false);
    }

    public override Task<MsScanData> GetLastMsScan(StreamScansRequest request, ServerCallContext context)
    {
      var channel = _gateway.GetMsScanContainer(request.MsDetectorSet);
      var scan = channel.GetLastMsScan();
      return Task.FromResult(scan is null ? new MsScanData() : ToProto(scan));
    }

    private static MsScanData ToProto(MsScanSnapshot scan)
    {
      var proto = new MsScanData
      {
        DetectorName = scan.DetectorName,
        ArrivalTimeUnixMs = new DateTimeOffset(scan.ArrivalTimeUtc).ToUnixTimeMilliseconds(),
        HasProfileInformation = scan.HasProfileInformation,
        Centroids = ToProto(scan.Centroids),
      };
      CopyInto(proto.Header, scan.Header);
      CopyInto(proto.Trailer, scan.Trailer);
      CopyInto(proto.StatusLog, scan.StatusLog);
      CopyInto(proto.TuneData, scan.TuneData);
      return proto;
    }

    private static Helios.Bridge.Contracts.CentroidBlock ToProto(Instruments.CentroidBlock block)
    {
      var proto = new Helios.Bridge.Contracts.CentroidBlock();
      proto.Mz.Add(block.Mz);
      proto.Intensity.Add(block.Intensity);
      proto.Charge.Add(block.Charge);
      proto.Resolution.Add(block.Resolution);
      proto.IsExceptional.Add(block.IsExceptional);
      proto.IsFragmented.Add(block.IsFragmented);
      proto.IsMerged.Add(block.IsMerged);
      proto.IsReferenced.Add(block.IsReferenced);
      return proto;
    }

    private static void CopyInto(MapField<string, string> target, System.Collections.Generic.IReadOnlyDictionary<string, string> source)
    {
      foreach (var kv in source) target[kv.Key] = kv.Value;
    }
  }
}
