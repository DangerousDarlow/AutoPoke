using Microsoft.Extensions.Logging;
using Model;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class GameStartedHandler : IPlayerEventHandler
{
    private readonly ILogger<GameStartedHandler> _logger;

    public GameStartedHandler(ILogger<GameStartedHandler> logger)
    {
        _logger = logger;
    }

    public IPlayer Player { get; set; } = null!;

    public Type TypeHandled => typeof(GameStarted);

    public void HandleEvent(IEvent @event)
    {
        var gameStarted = (GameStarted) @event;
        Player.CurrentGame = gameStarted.Game;
        _logger.LogDebug("Game '{GameId}' started", gameStarted.Game.Id);
    }
}