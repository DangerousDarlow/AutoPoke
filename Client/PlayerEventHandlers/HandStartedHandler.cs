using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class HandStartedHandler : IPlayerEventHandler
{
    private readonly ILogger<HandStartedHandler> _logger;

    public HandStartedHandler(ILogger<HandStartedHandler> logger)
    {
        _logger = logger;
    }

    public IPlayer Player { get; set; } = null!;
    
    public IStrategy Strategy { get; set; } = null!;

    public Type TypeHandled => typeof(HandStarted);

    public void HandleEvent(IEvent @event)
    {
        var handStarted = (HandStarted) @event;
        Player.Hand = handStarted.Hand;
        _logger.LogDebug("Hand '{HandId}' started", handStarted.Hand.Id);
    }
}