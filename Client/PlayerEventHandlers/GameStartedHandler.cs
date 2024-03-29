﻿using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

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

    public Type TypeHandled => typeof(GameStarted);

    public IPlayer Player { get; set; } = null!;

    public IStrategy Strategy { get; set; } = null!;

    public void HandleEvent(IEvent @event)
    {
        var gameStarted = (GameStarted) @event;
        Player.Game = gameStarted.Game;
        _logger.LogDebug("Game {GameId} started", gameStarted.Game.Id);
    }
}