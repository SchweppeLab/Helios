using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  internal sealed class InstrumentControlServiceImpl : InstrumentControlService.InstrumentControlServiceBase
  {
    private readonly IInstrumentGateway _gateway;

    public InstrumentControlServiceImpl(IInstrumentGateway gateway) => _gateway = gateway;

    public override Task<Empty> SetAcquisitionMode(SetAcquisitionModeRequest request, ServerCallContext context)
    {
      _gateway.Acquisition.SetMode(request.Mode == AcquisitionModeKind.ModeOn);
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> StartAcquisition(StartAcquisitionRequest request, ServerCallContext context)
    {
      _gateway.Acquisition.StartAcquisition(new AcquisitionWorkflowRequest
      {
        Kind = EnumConversions.ToHost(request.WorkflowCase),
        RawFileName = request.RawFileName,
        Comment = request.Comment,
        SingleProcessingDelay = request.SingleProcessingDelay,
        WaitForContactClosure = request.WaitForContactClosure,
        ScanCount = request.CountLimited?.ScanCount ?? 0,
        Duration = TimeSpan.FromMilliseconds(request.DurationLimited?.DurationMs ?? 0),
        MethodFileName = request.Method?.MethodFileName ?? string.Empty,
      });
      return Task.FromResult(new Empty());
    }

    public override Task<Empty> CancelAcquisition(Empty request, ServerCallContext context)
    {
      _gateway.Acquisition.CancelAcquisition();
      return Task.FromResult(new Empty());
    }

    public override async Task StreamAcquisitionState(Empty request, IServerStreamWriter<AcquisitionStateEvent> responseStream, ServerCallContext context)
    {
      var channel = System.Threading.Channels.Channel.CreateUnbounded<AcquisitionStateEvent>(new System.Threading.Channels.UnboundedChannelOptions
      {
        SingleReader = true,
        SingleWriter = false,
      });

      void OnStateChanged(object? s, AcquisitionStateChangedEventArgs e) =>
        channel.Writer.TryWrite(new AcquisitionStateEvent
        {
          StateChanged = new StateChangedEvent
          {
            SystemMode = EnumConversions.ToProto(e.State.Mode),
            SystemState = EnumConversions.ToProto(e.State.State),
          },
        });

      void OnStreamOpening(object? s, AcquisitionStreamOpeningEventArgs e)
      {
        var evt = new AcquisitionStreamOpeningEvent();
        foreach (var kv in e.StartingInformation) evt.StartingInformation[kv.Key] = kv.Value;
        channel.Writer.TryWrite(new AcquisitionStateEvent { StreamOpening = evt });
      }

      void OnStreamClosing(object? s, EventArgs e) =>
        channel.Writer.TryWrite(new AcquisitionStateEvent { StreamClosing = new AcquisitionStreamClosingEvent() });

      _gateway.Acquisition.StateChanged += OnStateChanged;
      _gateway.Acquisition.AcquisitionStreamOpening += OnStreamOpening;
      _gateway.Acquisition.AcquisitionStreamClosing += OnStreamClosing;

      try
      {
        while (await channel.Reader.WaitToReadAsync(context.CancellationToken).ConfigureAwait(false))
        {
          while (channel.Reader.TryRead(out var item))
          {
            await responseStream.WriteAsync(item).ConfigureAwait(false);
          }
        }
      }
      finally
      {
        _gateway.Acquisition.StateChanged -= OnStateChanged;
        _gateway.Acquisition.AcquisitionStreamOpening -= OnStreamOpening;
        _gateway.Acquisition.AcquisitionStreamClosing -= OnStreamClosing;
      }
    }

    // request.ExclusiveAccess isn't honored yet: the gateway acquires Scans once, non-exclusively,
    // at connect time. Helios's own IControl.GetScans(bool exclusiveAccess) does support exclusive
    // re-acquisition, unlike the raw IAPI wrapping this replaced -- wiring that through (a reconnect
    // of just the Scans handle) is a follow-up, not required for this merge to build and work.
    public override Task<ScanParametersResponse> GetPossibleScanParameters(GetPossibleScanParametersRequest request, ServerCallContext context)
    {
      var response = new ScanParametersResponse();
      foreach (var p in _gateway.Scans.PossibleParameters)
      {
        response.Parameters.Add(new Helios.Bridge.Contracts.ScanParameterDescriptor
        {
          Name = p.Name,
          Selection = p.Selection,
          DefaultValue = p.DefaultValue,
          Help = p.Help,
        });
      }
      return Task.FromResult(response);
    }

    public override Task<SubmitScanResponse> SubmitCustomScan(CustomScanRequest request, ServerCallContext context)
    {
      var scan = new CustomScan
      {
        RunningNumber = request.RunningNumber,
        SingleProcessingDelay = request.SingleProcessingDelay,
        IsPagcScan = request.IsPagcScan,
        PagcGroupIndex = request.PagcGroupIndex,
      };
      foreach (var kv in request.Values) scan.Values[kv.Key] = kv.Value;
      return Task.FromResult(new SubmitScanResponse { Accepted = _gateway.Scans.SubmitCustomScan(scan) });
    }

    public override Task<SubmitScanResponse> SubmitRepeatingScan(RepeatingScanRequest request, ServerCallContext context)
    {
      var scan = new RepeatingScan { RunningNumber = request.RunningNumber };
      foreach (var kv in request.Values) scan.Values[kv.Key] = kv.Value;
      return Task.FromResult(new SubmitScanResponse { Accepted = _gateway.Scans.SubmitRepeatingScan(scan) });
    }

    public override Task<SubmitScanResponse> CancelCustomScan(Empty request, ServerCallContext context) =>
      Task.FromResult(new SubmitScanResponse { Accepted = _gateway.Scans.CancelCustomScan() });

    public override Task<SubmitScanResponse> CancelRepetition(Empty request, ServerCallContext context) =>
      Task.FromResult(new SubmitScanResponse { Accepted = _gateway.Scans.CancelRepetition() });

    public override async Task StreamScanControlEvents(Empty request, IServerStreamWriter<ScanControlEvent> responseStream, ServerCallContext context)
    {
      var channel = System.Threading.Channels.Channel.CreateUnbounded<ScanControlEvent>(new System.Threading.Channels.UnboundedChannelOptions
      {
        SingleReader = true,
        SingleWriter = false,
      });

      void OnCanAcceptNext(object? s, EventArgs e) =>
        channel.Writer.TryWrite(new ScanControlEvent { CanAcceptNext = new CanAcceptNextCustomScanEvent() });

      void OnPossibleParametersChanged(object? s, EventArgs e)
      {
        var evt = new PossibleParametersChangedEvent();
        foreach (var p in _gateway.Scans.PossibleParameters)
        {
          evt.Parameters.Add(new Helios.Bridge.Contracts.ScanParameterDescriptor { Name = p.Name, Selection = p.Selection, DefaultValue = p.DefaultValue, Help = p.Help });
        }
        channel.Writer.TryWrite(new ScanControlEvent { PossibleParametersChanged = evt });
      }

      void OnNumOpenSlots(object? s, NumOpenCustomScanSlotsEventArgs e) =>
        channel.Writer.TryWrite(new ScanControlEvent { NumOpenSlots = new NumOpenCustomScanSlotsEvent { NumOpenSlots = e.NumOpenCustomScanSlots } });

      _gateway.Scans.CanAcceptNextCustomScan += OnCanAcceptNext;
      _gateway.Scans.PossibleParametersChanged += OnPossibleParametersChanged;
      _gateway.NumOpenCustomScanSlotsReceived += OnNumOpenSlots;

      try
      {
        while (await channel.Reader.WaitToReadAsync(context.CancellationToken).ConfigureAwait(false))
        {
          while (channel.Reader.TryRead(out var item))
          {
            await responseStream.WriteAsync(item).ConfigureAwait(false);
          }
        }
      }
      finally
      {
        _gateway.Scans.CanAcceptNextCustomScan -= OnCanAcceptNext;
        _gateway.Scans.PossibleParametersChanged -= OnPossibleParametersChanged;
        _gateway.NumOpenCustomScanSlotsReceived -= OnNumOpenSlots;
      }
    }

    public override Task<InstrumentValuesResponse> GetInstrumentValues(GetInstrumentValuesRequest request, ServerCallContext context)
    {
      var response = new InstrumentValuesResponse();
      foreach (var kv in _gateway.GetInstrumentValues(request.Names)) response.Values[kv.Key] = kv.Value;
      return Task.FromResult(response);
    }
  }
}
