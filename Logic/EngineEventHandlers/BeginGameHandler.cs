using System.Collections.Immutable;
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
        var currentGame = Engine.EngineGame;

        Engine.EngineGame = new EngineGame
        {
            Game = new Game
            {
                Players = Engine.Players.Values.ToImmutableList(),
                StartingStack = Engine.Configuration.StartingStack
            }
        };

        Engine.SendToAllClients(new GameStarted {Game = Engine.EngineGame.Game});
        _logger.LogDebug("Game '{GameId}' started", Engine.EngineGame.Game.Id);

        Engine.SendToSelf(new BeginHand());
    }
}