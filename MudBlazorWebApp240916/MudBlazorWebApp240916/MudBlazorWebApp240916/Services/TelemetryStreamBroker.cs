using System.Collections.Concurrent;
using System.Threading.Channels;
using MudBlazorWebApp240916.Shared.DataModel;

namespace MudBlazorWebApp240916.Services;

public sealed class TelemetryStreamBroker
{
    private readonly ConcurrentDictionary<Guid, Channel<TelemetryEnvelope>> _subscribers = new();

    public (Guid Id, ChannelReader<TelemetryEnvelope> Reader) Subscribe()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<TelemetryEnvelope>(new BoundedChannelOptions(128)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
        _subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
    {
        if (_subscribers.TryRemove(id, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }

    public void Publish(TelemetryEnvelope envelope)
    {
        foreach (var subscriber in _subscribers.Values)
        {
            subscriber.Writer.TryWrite(envelope);
        }
    }
}
