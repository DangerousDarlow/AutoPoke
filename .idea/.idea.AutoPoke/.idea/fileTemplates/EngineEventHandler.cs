#set ($HANDLER = "${Event}Handler")
using Model;
using Microsoft.Extensions.Logging;

namespace Logic.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class $HANDLER : IEngineEventHandler
{
    private readonly ILogger<$HANDLER> _logger;

    public $HANDLER(ILogger<$HANDLER> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof($Event);

    public void HandleEvent(IEvent @event)
    {
        var castEvent = ($Event) @event;
    }
}