using Action = Model.Action;

namespace Client;

public interface IStrategy
{
    string Name { get; }

    IPlayer Player { set; }

    Action Action();
}