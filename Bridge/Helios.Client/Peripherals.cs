using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Client
{
  // Fusion-only. GrpcControl only constructs one of these when the host reports
  // HasSyringePump=true at connect time, so IControl.SyringePumpControl is null for Exploris --
  // callers never see this class at all in that case. The Unimplemented handling below is
  // defensive (a host that changes backend mid-session, for instance), not the normal path.
  internal sealed class GrpcSyringePumpControl : ISyringePumpControl
  {
    private readonly Contracts.SyringePumpService.SyringePumpServiceClient _pump;
    private readonly CancellationToken _lifetime;

    public GrpcSyringePumpControl(Contracts.SyringePumpService.SyringePumpServiceClient pump, CancellationToken lifetime)
    {
      _pump = pump;
      _lifetime = lifetime;
      _ = PumpStatusAsync(lifetime);
    }

    public double Diameter { get; private set; }
    public double Volume { get; private set; }
    public double FlowRate { get; private set; }
    public SyringePumpStatus Status { get; private set; } = SyringePumpStatus.Unspecified;

    // The wire's SyringeStatusEvent carries no discriminator for which of these two conditions
    // triggered it (the host raises the identical snapshot for both) -- both fire on every
    // received snapshot rather than guessing which one applies.
    public event EventHandler? StatusChanged;
    public event EventHandler? ParameterValueChanged;

    public void Start() => HeliosClient.FireAndForget(_pump.StartAsync(new Contracts.Empty(), cancellationToken: _lifetime).ResponseAsync);
    public void Stop() => HeliosClient.FireAndForget(_pump.StopAsync(new Contracts.Empty(), cancellationToken: _lifetime).ResponseAsync);
    public void Toggle() => HeliosClient.FireAndForget(_pump.ToggleAsync(new Contracts.Empty(), cancellationToken: _lifetime).ResponseAsync);

    public void SetDiameter(double diameter) =>
      HeliosClient.FireAndForget(_pump.SetDiameterAsync(new Contracts.SetSyringeValueRequest { Value = diameter }, cancellationToken: _lifetime).ResponseAsync);

    public void SetVolume(double volume) =>
      HeliosClient.FireAndForget(_pump.SetVolumeAsync(new Contracts.SetSyringeValueRequest { Value = volume }, cancellationToken: _lifetime).ResponseAsync);

    public void SetFlowRate(double flowRate) =>
      HeliosClient.FireAndForget(_pump.SetFlowRateAsync(new Contracts.SetSyringeValueRequest { Value = flowRate }, cancellationToken: _lifetime).ResponseAsync);

    private async Task PumpStatusAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _pump.StreamSyringeStatus(new Contracts.Empty(), cancellationToken: cancellationToken);
        await foreach (var item in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          Diameter = item.Diameter;
          Volume = item.Volume;
          FlowRate = item.FlowRate;
          Status = Mapping.ToClient(item.Status);
          StatusChanged?.Invoke(this, EventArgs.Empty);
          ParameterValueChanged?.Invoke(this, EventArgs.Empty);
        }
      }
      catch (OperationCanceledException)
      {
        // Expected on Dispose. Also the ordinary outcome when connected to Exploris/no pump --
        // the host answers this RPC with Unimplemented immediately.
      }
      catch (Grpc.Core.RpcException) when (Status == SyringePumpStatus.Unspecified)
      {
        // Connected to Exploris (or a Simulated backend without a pump): the host's
        // SyringePumpServiceImpl throws Unimplemented for every method. Leave Status as
        // Unspecified rather than surfacing this as a fire-and-forget fault -- callers should be
        // gating on IInstrumentAccess.HasSyringePump anyway.
      }
    }
  }

  internal sealed class GrpcAnalogTraceContainer : IAnalogTraceContainer
  {
    private readonly Contracts.AnalogTraceService.AnalogTraceServiceClient _client;
    private readonly int _channel;

    public GrpcAnalogTraceContainer(Contracts.AnalogTraceService.AnalogTraceServiceClient client, int channel, CancellationToken lifetime)
    {
      _client = client;
      _channel = channel;
      Info = new AnalogTraceInfo();
      HeliosClient.FireAndForget(FetchInfoAsync(lifetime));
      _ = PumpTraceAsync(lifetime);
    }

    // Placeholder until the best-effort fetch in the constructor completes -- same
    // eventually-consistent tradeoff as GrpcScans.PossibleParameters.
    public AnalogTraceInfo Info { get; private set; }

    public event EventHandler<AnalogTracePointEventArgs>? AnalogTracePointArrived;

    private async Task FetchInfoAsync(CancellationToken cancellationToken)
    {
      var response = await _client.GetAnalogTraceInfoAsync(new Contracts.AnalogTraceRequest { Channel = _channel }, cancellationToken: cancellationToken).ConfigureAwait(false);
      Info = new AnalogTraceInfo
      {
        DetectorClass = response.DetectorClass,
        Minimum = response.Minimum,
        Maximum = response.Maximum,
        UpdateFrequencyHz = response.HasUpdateFrequencyHz ? response.UpdateFrequencyHz : null,
      };
    }

    private async Task PumpTraceAsync(CancellationToken cancellationToken)
    {
      try
      {
        using var call = _client.StreamAnalogTrace(new Contracts.AnalogTraceRequest { Channel = _channel }, cancellationToken: cancellationToken);
        await foreach (var item in call.ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
          AnalogTracePointArrived?.Invoke(this, new AnalogTracePointEventArgs { Value = item.Value, Occurrence = TimeSpan.FromMilliseconds(item.OccurrenceMs) });
        }
      }
      catch (OperationCanceledException)
      {
        // Expected on Dispose.
      }
    }
  }
}
