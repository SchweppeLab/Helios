using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  internal sealed class GrpcScans : IScans
  {
    private readonly Contracts.InstrumentControlService.InstrumentControlServiceClient _control;
    private readonly CancellationToken _lifetime;

    private ScanParameterDescriptor[] _possibleParameters = Array.Empty<ScanParameterDescriptor>();

    public GrpcScans(Contracts.InstrumentControlService.InstrumentControlServiceClient control, CancellationToken lifetime)
    {
      _control = control;
      _lifetime = lifetime;

      // Best-effort initial fetch (PossibleParameters is otherwise only kept current by the
      // PossibleParametersChanged stream below) -- until it completes, PossibleParameters reads
      // empty, same "eventually consistent cache" tradeoff as everywhere else in this client.
      HeliosClient.FireAndForget(FetchInitialParametersAsync(lifetime));
      _ = PumpScanControlEventsAsync(lifetime);
    }

    public ScanParameterDescriptor[] PossibleParameters => _possibleParameters;

    public event EventHandler? CanAcceptNextCustomScan;
    public event EventHandler<EventArgs>? PossibleParametersChanged;

    public async Task<bool> SubmitCustomScanAsync(CustomScan scan, CancellationToken cancellationToken = default)
    {
      var response = await _control.SubmitCustomScanAsync(Mapping.ToProto(scan), cancellationToken: cancellationToken).ConfigureAwait(false);
      return response.Accepted;
    }

    public async Task<bool> SubmitRepeatingScanAsync(RepeatingScan scan, CancellationToken cancellationToken = default)
    {
      var response = await _control.SubmitRepeatingScanAsync(Mapping.ToProto(scan), cancellationToken: cancellationToken).ConfigureAwait(false);
      return response.Accepted;
    }

    public async Task<bool> CancelCustomScanAsync(CancellationToken cancellationToken = default)
    {
      var response = await _control.CancelCustomScanAsync(new Contracts.Empty(), cancellationToken: cancellationToken).ConfigureAwait(false);
      return response.Accepted;
    }

    public async Task<bool> CancelRepetitionAsync(CancellationToken cancellationToken = default)
    {
      var response = await _control.CancelRepetitionAsync(new Contracts.Empty(), cancellationToken: cancellationToken).ConfigureAwait(false);
      return response.Accepted;
    }

    private async Task FetchInitialParametersAsync(CancellationToken cancellationToken)
    {
      var response = await _control.GetPossibleScanParametersAsync(new Contracts.GetPossibleScanParametersRequest(), cancellationToken: cancellationToken).ConfigureAwait(false);
      _possibleParameters = Mapping.ToClient(response.Parameters);
    }

    private async Task PumpScanControlEventsAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _control.StreamScanControlEvents(new Contracts.Empty(), cancellationToken: cancellationToken);
        await foreach (var e in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          switch (e.EventCase)
          {
            case Contracts.ScanControlEvent.EventOneofCase.CanAcceptNext:
              CanAcceptNextCustomScan?.Invoke(this, EventArgs.Empty);
              break;
            case Contracts.ScanControlEvent.EventOneofCase.PossibleParametersChanged:
              _possibleParameters = Mapping.ToClient(e.PossibleParametersChanged.Parameters);
              PossibleParametersChanged?.Invoke(this, EventArgs.Empty);
              break;
            // NumOpenSlots has no client-facing event today -- see HeliosInstrumentGateway's own
            // note that Helios.dll's IScans has no "open custom scan slots" concept to source it
            // from on real hardware; only the Simulated backend ever sends it.
          }
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
    }
  }
}
