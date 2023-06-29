using Events;
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
        var joinResponse = new JoinResponse {Status = JoinResponseStatus.Success};
        Engine.SendToSingleClient(Envelope.CreateFromEvent(joinResponse), joinRequest.PlayerId);
        _logger.LogInformation("Player '{PlayerName}' joined", joinRequest.PlayerName);
    }
}