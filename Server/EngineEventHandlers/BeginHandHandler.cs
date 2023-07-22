using Microsoft.Extensions.Logging;
using Model;
using Model.Events;

namespace Server.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class BeginHandHandler : IEngineEventHandler
{
    private readonly ILogger<BeginHandHandler> _logger;

    public BeginHandHandler(ILogger<BeginHandHandler> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof(BeginHand);

    public OriginFilter OriginFilter => OriginFilter.EngineOnly;

    public void HandleEvent(IEvent @event)
    {
        Engine.ResetForNewHand();

        foreach (var player in Engine.Players)
        {
            Engine.SendToSingleClient(new HoleCards(Engine.Deck.Draw(), Engine.Deck.Draw()), player.Id);
            _logger.LogDebug("Hole cards dealt to player {PlayerId}", player.Id);
        }

        var sequence = Engine.Hand?.Sequence + 1 ?? 1;
        var multiplier = (sequence - 1) / Engine.Configuration.HandsPerBlindLevel + 1;
        var blind = Engine.Configuration.InitialSmallBlind * multiplier;

        Engine.Hand = new Hand
        {
            Players = Engine.Players,
            Sequence = sequence,
            SmallBlind = blind
        };

        Engine.SendToAllClients(new HandStarted
        {
            Hand = Engine.Hand
        });
    }
}