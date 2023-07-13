#set ($HANDLER = "${Event}Handler")
using Microsoft.Extensions.Logging;
using Model;

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

    public IPlayer Player { get; set; } = null!;

    public Type TypeHandled => typeof($Event);

    public void HandleEvent(IEvent @event)
    {
        var castEvent = ($Event) @event;
    }
}