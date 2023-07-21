using Model;

namespace Server;

public interface IEngineEventHandler
{
    IEngine Engine { set; }

    Type TypeHandled { get; }

    OriginFilter OriginFilter { get; }

    void HandleEvent(IEvent @event);
}

public enum OriginFilter
{
    Any,
    EngineOnly
}