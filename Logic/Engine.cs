using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using ZeroMq;

namespace Logic;

public interface IEngine
{
    EngineConfiguration Configuration { get; }

    ImmutableDictionary<Guid, Player> Players { get; }

    EngineSession? EngineSession { get; set; }

    EngineGame? EngineGame { get; set; }

    Deck Deck { get; }

    void SendToSingleClient<T>(T @event, Guid playerId) where T : IEvent;

    void SendToAllClients<T>(T @event) where T : IEvent;

    void SendToSelf<T>(T @event) where T : IEvent;

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

    public EngineSession? EngineSession { get; set; }

    public EngineGame? EngineGame { get; set; }

    public Deck Deck { get; } = new();
    
    public void AddPlayer(Player player) => _players.Add(player.Id, player);

    public void SendToSingleClient<T>(T @event, Guid playerId) where T : IEvent
    {
        var envelope = Envelope.CreateFromEvent(@event);
        _server.SendToSingleClient(envelope, playerId);
    }

    public void SendToAllClients<T>(T @event) where T : IEvent
    {
        var envelope = Envelope.CreateFromEvent(@event);
        _server.SendToAllClients(envelope);
    }

    public void SendToSelf<T>(T @event) where T : IEvent
    {
        var envelope = Envelope.CreateFromEvent(@event);
        envelope.Origin = _server.Id;
        HandleEvent(envelope);
    }

    private void HandleEvent(Envelope envelope)
    {
        var @event = envelope.ExtractEvent();
        if (_eventHandlers.TryGetValue(@event.GetType(), out var handler))
        {
            if (handler.OriginFilter == OriginFilter.EngineOnly && envelope.Origin != _server.Id)
            {
                _players.TryGetValue(envelope.Origin, out var player);
                _logger.LogError(
                    "Event type {EventType} origin is player ({PlayerId} {PlayerName}) but must be engine",
                    @event.GetType().Name, envelope.Origin, player?.Name ?? "Unknown");

                return;
            }

            handler.HandleEvent(@event);
        }
        else
        {
            _logger.LogError("No handler for event type {EventType}", @event.GetType().Name);
            throw new Exception($"No handler for event type '{@event.GetType().Name}'");
        }
    }
}