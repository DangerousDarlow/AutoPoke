using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using Model.Events;
using ZeroMq;

namespace Client;

public interface IPlayer
{
    Guid Id { get; }

    PlayerConfiguration Configuration { get; }

    Game? Game { get; set; }

    Hand? Hand { get; set; }

    HoleCards? HoleCards { get; set; }

    IStrategy Strategy { get; set; }

    void Join();

    void Send<T>(T @event) where T : IEvent;
}

public class Player : IPlayer
{
    private readonly IClient _client;
    private readonly Dictionary<Type, IPlayerEventHandler> _eventHandlers;
    private readonly ILogger<Player> _logger;

    public Player(
        IClient client,
        IEnumerable<IPlayerEventHandler> eventHandlers,
        IEnumerable<IStrategy> strategies,
        IOptions<PlayerConfiguration> configuration,
        ILogger<Player> logger)
    {
        _client = client;
        _logger = logger;
        _client.ReceivedEvent += HandleEvent;

        var strategy = strategies.FirstOrDefault(x => x.GetType().Name == configuration.Value.Strategy);
        Strategy = strategy ?? throw new InvalidOperationException($"No strategy found with name {configuration.Value.Strategy}");
        strategy.Player = this;

        _eventHandlers = eventHandlers.ToDictionary(x => x.TypeHandled);
        foreach (var handler in _eventHandlers.Values)
        {
            handler.Player = this;
            handler.Strategy = strategy;
        }

        Configuration = configuration.Value;
    }

    public Guid Id => _client.Id;

    public PlayerConfiguration Configuration { get; }

    public Game? Game { get; set; }

    public Hand? Hand { get; set; }

    public HoleCards? HoleCards { get; set; }

    public IStrategy Strategy { get; set; }

    public void Join() => Send(new JoinRequest {PlayerId = _client.Id, PlayerName = Configuration.Name});

    public void Send<T>(T @event) where T : IEvent
    {
        var envelope = Envelope.CreateFromEvent(@event);
        _client.SendToServer(envelope);
    }

    private void HandleEvent(Envelope envelope)
    {
        var @event = envelope.ExtractEvent();
        if (_eventHandlers.TryGetValue(@event.GetType(), out var handler))
        {
            handler.HandleEvent(@event);
        }
        else
        {
            _logger.LogWarning("No handler for event type {EventType}", @event.GetType().Name);
        }
    }
}