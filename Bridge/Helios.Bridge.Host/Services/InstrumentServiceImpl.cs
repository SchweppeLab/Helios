using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helios.Bridge.Contracts;
using Helios.Bridge.Host.Instruments;

namespace Helios.Bridge.Host.Services
{
  internal sealed class InstrumentServiceImpl : InstrumentService.InstrumentServiceBase
  {
    private readonly IInstrumentGateway _gateway;
    private readonly ConnectionWatchdog _watchdog;

    public InstrumentServiceImpl(IInstrumentGateway gateway, ConnectionWatchdog watchdog)
    {
      _gateway = gateway;
      _watchdog = watchdog;
    }

    public override async Task<StatusResponse> Connect(ConnectRequest request, ServerCallContext context)
    {
      await _gateway.ConnectAsync(context.CancellationToken).ConfigureAwait(false);
      return BuildStatus();
    }

    public override Task<StatusResponse> GetStatus(Empty request, ServerCallContext context) =>
      Task.FromResult(BuildStatus());

    public override Task<Empty> DisposeInstrument(Empty request, ServerCallContext context)
    {
      _gateway.Dispose();
      return Task.FromResult(new Empty());
    }

    // Fans out to four gateway events onto one stream, so this is written directly against a
    // Channel<T> rather than through GrpcStreaming.PumpAsync (that helper assumes a single event
    // source; the per-call subscribe/unsubscribe state here must stay local to this call, not a
    // field, since InstrumentServiceImpl is shared across concurrent client streams).
    // Every Helios.Client connection keeps exactly one of these calls open for its whole lifetime
    // (see HeliosClient.PumpServiceEventsAsync), including one it opens right after Connect --
    // that makes this call's start/end a reliable, crash-safe proxy for "a client is connected":
    // when a client process dies, its socket closes immediately even without a graceful dispose,
    // which ends this call the same way a clean disconnect would. ConnectionWatchdog uses that to
    // auto-shut the host down once every client has gone, without needing a separate heartbeat.
    public override async Task StreamServiceEvents(Empty request, IServerStreamWriter<ServiceEvent> responseStream, ServerCallContext context)
    {
      _watchdog.ConnectionOpened();
      var channel = System.Threading.Channels.Channel.CreateUnbounded<ServiceEvent>(new System.Threading.Channels.UnboundedChannelOptions
      {
        SingleReader = true,
        SingleWriter = false,
      });

      void OnServiceConnectionChanged(object? s, EventArgs e) =>
        channel.Writer.TryWrite(new ServiceEvent { ServiceConnectionChanged = new ServiceConnectionChangedEvent { ServiceConnected = _gateway.ServiceConnected } });

      void OnInstrumentConnectionChanged(object? s, EventArgs e) =>
        channel.Writer.TryWrite(new ServiceEvent { InstrumentConnectionChanged = new InstrumentConnectionChangedEvent { Connected = _gateway.InstrumentConnected } });

      void OnMessagesArrived(object? s, MessagesArrivedEventArgs e)
      {
        var evt = new MessagesArrivedEvent();
        foreach (var m in e.Messages)
        {
          var msg = new Helios.Bridge.Contracts.InstrumentMessage
          {
            InstrumentId = m.InstrumentId,
            InstrumentName = m.InstrumentName,
            MessageId = m.MessageId,
            CreationTimeUnixMs = new DateTimeOffset(m.CreationTimeUtc).ToUnixTimeMilliseconds(),
            Status = m.Status,
            Message = m.Message,
          };
          msg.MessageArgs.Add(m.MessageArgs);
          evt.Messages.Add(msg);
        }
        channel.Writer.TryWrite(new ServiceEvent { MessagesArrived = evt });
      }

      void OnContactClosureChanged(object? s, ContactClosureChangedEventArgs e) =>
        channel.Writer.TryWrite(new ServiceEvent { ContactClosureChanged = new ContactClosureChangedEvent { RisingEdges = e.RisingEdges, FallingEdges = e.FallingEdges } });

      _gateway.ServiceConnectionChanged += OnServiceConnectionChanged;
      _gateway.InstrumentConnectionChanged += OnInstrumentConnectionChanged;
      _gateway.MessagesArrived += OnMessagesArrived;
      _gateway.ContactClosureChanged += OnContactClosureChanged;

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
        _gateway.ServiceConnectionChanged -= OnServiceConnectionChanged;
        _gateway.InstrumentConnectionChanged -= OnInstrumentConnectionChanged;
        _gateway.MessagesArrived -= OnMessagesArrived;
        _gateway.ContactClosureChanged -= OnContactClosureChanged;
        _watchdog.ConnectionClosed();
      }
    }

    private StatusResponse BuildStatus() => new()
    {
      Family = EnumConversions.ToProto(_gateway.InstrumentFamily),
      ServiceConnected = _gateway.ServiceConnected,
      InstrumentConnected = _gateway.InstrumentConnected,
      InstrumentId = _gateway.InstrumentId,
      InstrumentName = _gateway.InstrumentName,
      CountMsDetectors = _gateway.CountMsDetectors,
      CountAnalogChannels = _gateway.CountAnalogChannels,
      HasSyringePump = _gateway.HasSyringePump,
      HasAnalogChannels = _gateway.CountAnalogChannels > 0,
      DetectorClasses = { _gateway.DetectorClasses },
    };
  }
}
