using System.Collections.Immutable;
using Model;
using Microsoft.Extensions.Logging;

namespace Logic.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class BeginSessionHandler : IEngineEventHandler
{
    private readonly ILogger<BeginSessionHandler> _logger;

    public BeginSessionHandler(ILogger<BeginSessionHandler> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof(BeginSession);

    public void HandleEvent(IEvent @event)
    {
        var beginSession = (BeginSession) @event;

        if (Engine.EngineSession != null)
        {
            _logger.LogWarning("Failed to start session: session already started");
            return;
        }

        Engine.EngineSession = new EngineSession
        {
            Session = new Session {Games = beginSession.Games}
        };

        Engine.SendToAllClients(new SessionStarted {Session = Engine.EngineSession.Session});
        _logger.LogInformation("Session '{SessionId}' started: {Games} games", Engine.EngineSession.Session.SessionId, Engine.EngineSession.Session.Games);

        Engine.EngineGame = new EngineGame
        {
            Game = new Game
            {
                Sequence = 1,
                Players = Engine.Players.Values.ToImmutableList(),
                StartingStack = Engine.Configuration.StartingStack
            }
        };

        Engine.SendToAllClients(new GameStarted {Game = Engine.EngineGame.Game});
        _logger.LogDebug("Game '{GameId}' started", Engine.EngineGame.Game.Id);
    }
}