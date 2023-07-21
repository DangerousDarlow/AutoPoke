#set ($HANDLER = "${Event}Handler")
using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

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

    public OriginFilter OriginFilter => OriginFilter.Any;

    public void HandleEvent(IEvent @event)
    {
        var castEvent = ($Event) @event;
    }
}