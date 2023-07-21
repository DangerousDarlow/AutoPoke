using Model;

namespace Client;

public interface IPlayerEventHandler
{
    Type TypeHandled { get; }

    IPlayer Player { set; }

    IStrategy Strategy { set; }

    void HandleEvent(IEvent @event);
}