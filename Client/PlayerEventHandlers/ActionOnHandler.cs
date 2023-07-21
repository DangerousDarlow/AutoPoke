using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class ActionOnHandler : IPlayerEventHandler
{
    private readonly ILogger<ActionOnHandler> _logger;

    public ActionOnHandler(ILogger<ActionOnHandler> logger)
    {
        _logger = logger;
    }

    public Type TypeHandled => typeof(ActionOn);
    
    public IPlayer Player { get; set; } = null!;

    public IStrategy Strategy { get; set; } = null!;

    public void HandleEvent(IEvent @event)
    {
        var actionOn = (ActionOn) @event;

        _logger.LogDebug("ActionOn {ActionOnId} received", actionOn.Id);

        var action = Strategy.Action();

        Player.Send(new ActionOnResponse {ResponseTo = actionOn.Id, Action = action});
        _logger.LogDebug("ActionOnResponse for {ActionOnId} sent: {Action}", actionOn.Id, action);
    }
}