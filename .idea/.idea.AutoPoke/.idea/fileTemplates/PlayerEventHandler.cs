#set ($HANDLER = "${Event}Handler")
using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class $HANDLER : IPlayerEventHandler
{
    private readonly ILogger<$HANDLER> _logger;

    public $HANDLER(ILogger<$HANDLER> logger)
    {
        _logger = logger;
    }

    public Type TypeHandled => typeof($Event);

    public IPlayer Player { get; set; } = null!;

    public IStrategy Strategy { get; set; } = null!;

    public void HandleEvent(IEvent @event)
    {
        var castEvent = ($Event) @event;
    }
}