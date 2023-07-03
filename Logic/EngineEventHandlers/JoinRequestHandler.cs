using Model;
using Microsoft.Extensions.Logging;

namespace Logic.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class JoinRequestHandler : IEngineEventHandler
{
    private readonly ILogger<JoinRequestHandler> _logger;

    public JoinRequestHandler(ILogger<JoinRequestHandler> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof(JoinRequest);

    public void HandleEvent(IEvent @event)
    {
        var joinRequest = (JoinRequest) @event;

        if (Engine.Players.Count >= Engine.Configuration.MaxPlayers)
        {
            Engine.SendToSingleClient(new JoinResponse {Status = JoinResponseStatus.FailureEngineFull}, joinRequest.PlayerId);
            _logger.LogInformation("Player '{PlayerName}' tried to join, but engine is full", joinRequest.PlayerName);
            return;
        }

        Engine.AddPlayer(new Player {Id = joinRequest.PlayerId, Name = joinRequest.PlayerName});

        Engine.SendToSingleClient(new JoinResponse {Status = JoinResponseStatus.Success}, joinRequest.PlayerId);
        _logger.LogInformation("Player '{PlayerName}' joined", joinRequest.PlayerName);
    }
}