using Microsoft.Extensions.Logging;
using Model;
using Action = Model.Action;

namespace Client.Strategies;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class $Strategy : IStrategy
{
    private readonly ILogger<$Strategy> _logger;

    public $Strategy(ILogger<$Strategy> logger)
    {
        _logger = logger;
    }

    public string Name => nameof($Strategy);

    public IPlayer Player { get; set; } = null!;

    public Action Action() => new() {Type = ActionType.Fold};
}