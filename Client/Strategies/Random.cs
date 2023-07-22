using Microsoft.Extensions.Logging;
using Model;
using Action = Model.Action;

namespace Client.Strategies;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class Random : IStrategy
{
    private static readonly System.Random SystemRandom = new();

    private readonly ILogger<Random> _logger;

    public Random(ILogger<Random> logger)
    {
        _logger = logger;
    }

    public string Name => nameof(Random);

    public IPlayer Player { get; set; } = null!;

    public Action Action()
    {
        // TODO: Check, Call and Raise depend on the current bet
        var actionTypes = Enum.GetValues(typeof(ActionType));
        var actionType = (ActionType) (actionTypes.GetValue(SystemRandom.Next(actionTypes.Length)) ?? ActionType.Fold);
        var me = Player.MeInModel();
        int? amount = actionType == ActionType.Raise ? SystemRandom.Next(Player.Hand!.BigBlind, me.Stack) : null;
        return new Action {Type = ActionType.Fold, Amount = amount};
    }
}