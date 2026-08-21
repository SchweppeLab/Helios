using System;
using System.Diagnostics;
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
      catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
      {
        // Also expected on Dispose -- Grpc.Net.Client surfaces a locally-cancelled streaming call
        // as RpcException(Cancelled), not OperationCanceledException.
      }
      catch (Exception ex)
      {
        // An unexpected terminal error (e.g. RpcException(Unavailable) from a dead transport) --
        // without this, it would become an unobserved task exception, since the constructor fires
        // this off without awaiting it. The connection-level ConnectionChanged notification (see
        // HeliosClient.PumpServiceEventsAsync) is what the app is expected to react to; this is
        // just so the failure isn't swallowed entirely.
        Debug.WriteLine($"Helios.Client: scan stream for detector set {_msDetectorSet} failed: {ex}");
      }
    }
  }
}
