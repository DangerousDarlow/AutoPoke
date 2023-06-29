using Events;
using Microsoft.Extensions.Logging;
using ZeroMq;

namespace Logic;

public interface IEngine
{
    void SendToSingleClient(Envelope envelope, Guid playerId);
}

public class Engine : IEngine
{
    private readonly Dictionary<Type, IEngineEventHandler> _eventHandlers;
    private readonly ILogger<Engine> _logger;
    private readonly IServer _server;

    public Engine(IServer server, IEnumerable<IEngineEventHandler> eventHandlers, ILogger<Engine> logger)
    {
        _server = server;
        _logger = logger;
        _server.ReceivedEvent += HandleEvent;

        _eventHandlers = eventHandlers.ToDictionary(x => x.TypeHandled);
        foreach (var handler in _eventHandlers.Values) handler.Engine = this;
    }

    public void SendToSingleClient(Envelope envelope, Guid playerId) => _server.SendToSingleClient(envelope, playerId);

    private void HandleEvent(Envelope envelope)
    {
        var @event = envelope.ExtractEvent();
        if (_eventHandlers.TryGetValue(@event.GetType(), out var handler))
        {
            handler.HandleEvent(@event);
        }
        else
        {
            _logger.LogError("No handler for event type {EventType}", @event.GetType().Name);
            throw new Exception($"No handler for event type '{@event.GetType().Name}'");
        }
    }
}