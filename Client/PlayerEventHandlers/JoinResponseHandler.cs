using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class JoinResponseHandler : IPlayerEventHandler
{
    private readonly ILogger<JoinResponseHandler> _logger;

    public JoinResponseHandler(ILogger<JoinResponseHandler> logger)
    {
        _logger = logger;
    }

    public Type TypeHandled => typeof(JoinResponse);

    public IPlayer Player { get; set; } = null!;

    public IStrategy Strategy { get; set; } = null!;

    public void HandleEvent(IEvent @event)
    {
        var joinResponse = (JoinResponse) @event;

        if (joinResponse.Status == JoinResponseStatus.FailureEngineFull)
        {
            _logger.LogCritical("Failed to join engine; engine is full");
            return;
        }

        _logger.LogInformation("Joined engine");
    }
}