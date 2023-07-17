using Microsoft.Extensions.Logging;
using Model;

namespace Logic.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class BeginGameHandler : IEngineEventHandler
{
    private readonly ILogger<BeginGameHandler> _logger;

    public BeginGameHandler(ILogger<BeginGameHandler> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof(BeginGame);

    public OriginFilter OriginFilter => OriginFilter.EngineOnly;

    public void HandleEvent(IEvent @event)
    {
        Engine.InitialisePlayersForNewGame();

        Engine.Game = new Game();

        Engine.SendToAllClients(new GameStarted
        {
            Game = Engine.Game,
            Players = Engine.Players
        });

        _logger.LogDebug("Game '{GameId}' started", Engine.Game.Id);

        Engine.SendToSelf(new BeginHand());
    }
}