using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using ZeroMq;

namespace Logic;

public interface IEngine
{
    EngineConfiguration Configuration { get; }

    ImmutableList<Player> Players { get; }

    EngineSession? EngineSession { get; set; }

    Game? Game { get; set; }

    Hand? Hand { get; set; }

    Deck Deck { get; }

    void SendToSingleClient<T>(T @event, Guid playerId) where T : IEvent;

    void SendToAllClients<T>(T @event) where T : IEvent;

    void SendToSelf<T>(T @event) where T : IEvent;

    void AddPlayer(Guid playerId, string playerName);

    void ResetPlayersForNewGame();

    void MoveFirstPlayerToLast();
}

public class Engine : IEngine
{
    private static readonly Random Random = new();

    private readonly Dictionary<Type, IEngineEventHandler> _eventHandlers;
    private readonly ILogger<Engine> _logger;
    private readonly IServer _server;
    private List<Player> _players = new();

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

    public ImmutableList<Player> Players => _players.ToImmutableList();

    public EngineSession? EngineSession { get; set; }

    public Game? Game { get; set; }

    public Hand? Hand { get; set; }

    public Deck Deck { get; } = new();

    public void AddPlayer(Guid playerId, string playerName) => _players.Add(new Player {Id = playerId, Name = playerName, Stack = Configuration.StartingStack});

    public void ResetPlayersForNewGame()
    {
        _players = _players.Select(player => player.WithStack(Configuration.StartingStack)).ToList();
        _players.Shuffle(Random);
    }

    public void MoveFirstPlayerToLast()
    {
        if (_players.Count < 2) return;

        var player = _players.First();
        _players.Remove(player);
        _players.Add(player);
    }

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
                var player = _players.FirstOrDefault(p => p.Id == envelope.Origin);
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