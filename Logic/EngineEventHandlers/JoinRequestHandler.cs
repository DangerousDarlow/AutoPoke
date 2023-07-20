using Microsoft.Extensions.Logging;
using Model;

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

    public OriginFilter OriginFilter => OriginFilter.Any;

    public void HandleEvent(IEvent @event)
    {
        var joinRequest = (JoinRequest) @event;

        if (Engine.Players.Count >= Engine.Configuration.MaximumNumberOfPlayers)
        {
            Engine.SendToSingleClient(new JoinResponse {Status = JoinResponseStatus.FailureEngineFull}, joinRequest.PlayerId);
            _logger.LogInformation("Player '{PlayerName}' tried to join, but engine is full", joinRequest.PlayerName);
            return;
        }

        Engine.AddPlayer(joinRequest.PlayerId, joinRequest.PlayerName);

        Engine.SendToSingleClient(new JoinResponse {Status = JoinResponseStatus.Success}, joinRequest.PlayerId);
        _logger.LogInformation("Player '{PlayerName}' joined", joinRequest.PlayerName);
    }
}