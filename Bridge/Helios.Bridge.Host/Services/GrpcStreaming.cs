using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;

namespace Helios.Bridge.Host.Services
{
  // Bridges a synchronous, arbitrary-thread C# event (IInstrumentGateway's events fire off
  // whatever thread IAPI/Helios calls back on) to a gRPC server-streaming call, which requires
  // writes to happen one at a time. A Channel<T> is the standard, allocation-light way to do that
  // hand-off without blocking the event-raising thread on network I/O.
  internal static class GrpcStreaming
  {
    // `subscribe`/`unsubscribe` are plain callbacks rather than something that hands a wrapped
    // delegate back and forth -- callers close over their own local EventHandler in both, which
    // keeps all per-call state in the caller's stack frame instead of on this (shared,
    // concurrently-called) service instance.
    public static async Task PumpAsync<T>(
      Action<Action<T>> subscribe,
      Action unsubscribe,
      IServerStreamWriter<T> responseStream,
      ServerCallContext context)
    {
      var channel = System.Threading.Channels.Channel.CreateUnbounded<T>(new UnboundedChannelOptions
      {
        SingleReader = true,
        SingleWriter = false,
      });

      subscribe(item => channel.Writer.TryWrite(item));
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
        unsubscribe();
      }
    }
  }
}
