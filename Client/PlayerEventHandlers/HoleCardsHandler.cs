using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Client.PlayerEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class HoleCardsHandler : IPlayerEventHandler
{
    private readonly ILogger<HoleCardsHandler> _logger;

    public HoleCardsHandler(ILogger<HoleCardsHandler> logger)
    {
        _logger = logger;
    }

    public Type TypeHandled => typeof(HoleCards);

    public IPlayer Player { get; set; } = null!;

    public IStrategy Strategy { get; set; } = null!;

    public void HandleEvent(IEvent @event)
    {
        Player.HoleCards = (HoleCards) @event;
        _logger.LogDebug("Hole cards {HoleCards} received", Player.HoleCards);
    }
}