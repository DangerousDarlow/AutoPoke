using System.Collections.Immutable;
using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZeroMq;

namespace Logic;

public interface IEngine
{
    EngineConfiguration Configuration { get; }
    ImmutableDictionary<Guid, Player> Players { get; }
    void SendToSingleClient(Envelope envelope, Guid playerId);
    void AddPlayer(Player player);
}

public class Engine : IEngine
{
    private readonly Dictionary<Type, IEngineEventHandler> _eventHandlers;
    private readonly ILogger<Engine> _logger;
    private readonly Dictionary<Guid, Player> _players = new();
    private readonly IServer _server;

    public Engine(IServer server, IEnumerable<IEngineEventHandler> eventHandlers, IOptions<EngineConfiguration> configuration, ILogger<Engine> logger)
    {
        _server = server;
        _server.ReceivedEvent += HandleEvent;

        _eventHandlers = eventHandlers.ToDictionary(x => x.TypeHandled);
        foreach (var handler in _eventHandlers.Values) handler.Engine = this;

        Configuration = configuration.Value;
        _logger = logger;
    }

    public EngineConfiguration Configuration { get; }

    public ImmutableDictionary<Guid, Player> Players => _players.ToImmutableDictionary();

    public void AddPlayer(Player player) => _players.Add(player.Id, player);

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