using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  internal sealed class GrpcAcquisition : IAcquisition
  {
    private readonly Contracts.InstrumentControlService.InstrumentControlServiceClient _control;
    private readonly CancellationToken _lifetime;

    // Seeded with Disconnected/NotConnected until the first StreamAcquisitionState event, which
    // the host sends on subscription with the instrument's current state; later events are changes.
    private AcquisitionStateSnapshot _state = new() { Mode = SystemMode.Disconnected, State = InstrumentState.NotConnected };

    public GrpcAcquisition(Contracts.InstrumentControlService.InstrumentControlServiceClient control, CancellationToken lifetime)
    {
      _control = control;
      _lifetime = lifetime;
      _ = PumpAcquisitionStateAsync(lifetime);
    }

    public IHeliosState State => new HeliosStateSnapshot(_state);

    public event EventHandler<StateChangedEventArgs>? StateChanged;
    public event EventHandler<AcquisitionStreamOpeningEventArgs>? AcquisitionStreamOpening;
    public event EventHandler? AcquisitionStreamClosing;

    public void SetMode(bool on) =>
      HeliosClient.FireAndForget(_control.SetAcquisitionModeAsync(
        new Contracts.SetAcquisitionModeRequest { Mode = on ? Contracts.AcquisitionModeKind.ModeOn : Contracts.AcquisitionModeKind.ModeOff },
        cancellationToken: _lifetime).ResponseAsync);

    public void StartAcquisition(AcquisitionWorkflowRequest request) =>
      HeliosClient.FireAndForget(_control.StartAcquisitionAsync(Mapping.ToProto(request), cancellationToken: _lifetime).ResponseAsync);

    public void CancelAcquisition() =>
      HeliosClient.FireAndForget(_control.CancelAcquisitionAsync(new Contracts.Empty(), cancellationToken: _lifetime).ResponseAsync);

    private async Task PumpAcquisitionStateAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _control.StreamAcquisitionState(new Contracts.Empty(), cancellationToken: cancellationToken);
        await foreach (var e in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          switch (e.EventCase)
          {
            case Contracts.AcquisitionStateEvent.EventOneofCase.StateChanged:
              _state = Mapping.ToClient(e.StateChanged);
              StateChanged?.Invoke(this, new StateChangedEventArgs(_state));
              break;
            case Contracts.AcquisitionStateEvent.EventOneofCase.StreamOpening:
              var info = new System.Collections.Generic.Dictionary<string, string>();
              foreach (var kv in e.StreamOpening.StartingInformation) info[kv.Key] = kv.Value;
              AcquisitionStreamOpening?.Invoke(this, new AcquisitionStreamOpeningEventArgs { StartingInformation = info });
              break;
            case Contracts.AcquisitionStateEvent.EventOneofCase.StreamClosing:
              AcquisitionStreamClosing?.Invoke(this, EventArgs.Empty);
              break;
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

    private sealed class HeliosStateSnapshot : IHeliosState
    {
      public HeliosStateSnapshot(AcquisitionStateSnapshot s)
      {
        SystemMode = s.Mode;
        SystemState = s.State;
      }

      public InstrumentState SystemState { get; }
      public SystemMode SystemMode { get; }
    }
  }
}
