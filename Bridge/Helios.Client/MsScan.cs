using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  // Streaming starts as soon as this is constructed (i.e. as soon as IInstrumentAccess.
  // GetMsScanContainer is called for a given detector set) rather than lazily on first
  // MsScanArrived subscription -- simpler than reference-counted add/remove event accessors, and
  // callers are expected to call GetMsScanContainer only for detector sets they actually want.
  internal sealed class GrpcMsScanContainer : IMsScanContainer
  {
    private readonly Contracts.ScanStreamService.ScanStreamServiceClient _scans;
    private readonly int _msDetectorSet;
    private readonly CancellationToken _lifetime;

    public GrpcMsScanContainer(Contracts.ScanStreamService.ScanStreamServiceClient scans, int msDetectorSet, string detectorClass, CancellationToken lifetime)
    {
      _scans = scans;
      _msDetectorSet = msDetectorSet;
      _lifetime = lifetime;
      DetectorClass = detectorClass;
      _ = PumpScansAsync(lifetime);
    }

    public string DetectorClass { get; }

    public event EventHandler<MsScanEventArgs>? MsScanArrived;

    public async Task<IMsScan?> GetLastMsScanAsync(CancellationToken cancellationToken = default)
    {
      var response = await _scans.GetLastMsScanAsync(new Contracts.StreamScansRequest { MsDetectorSet = _msDetectorSet }, cancellationToken: cancellationToken).ConfigureAwait(false);
      return response.DetectorName.Length == 0 ? null : Mapping.ToClient(response);
    }

    private async Task PumpScansAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _scans.StreamMsScans(new Contracts.StreamScansRequest { MsDetectorSet = _msDetectorSet }, cancellationToken: cancellationToken);
        await foreach (var item in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          MsScanArrived?.Invoke(this, new MsScanEventArgs(Mapping.ToClient(item)));
        }
      }
      catch (OperationCanceledException)
      {
        // Expected on Dispose.
      }
    }
  }
}
