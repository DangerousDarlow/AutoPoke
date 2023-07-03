using Model;

namespace Client;

public interface IPlayerEventHandler
{
    IPlayer Player { set; }

    Type TypeHandled { get; }

    void HandleEvent(IEvent @event);
}