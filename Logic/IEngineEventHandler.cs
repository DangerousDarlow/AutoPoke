using Events;

namespace Logic;

public interface IEngineEventHandler
{
    IEngine Engine { set; }

    Type TypeHandled { get; }

    void HandleEvent(IEvent @event);
}