using Events;
using Microsoft.Extensions.Logging;
using ZeroMq;

namespace Engine;

public class Engine
{
    public delegate void JoinRequestEventHandler(Guid from, JoinRequest joinRequest);

    private readonly ILogger<Engine> _logger;
    private readonly Router _router;

    public Engine(Router router, ILogger<Engine> logger)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(logger);
        _router = router;
        _logger = logger;
    }

    public void Configure()
    {
        _router.Configure();
        _router.ReceivedEvent += (from, envelope) =>
        {
            if (envelope.ExtractEvent() is not JoinRequest joinRequest)
                return;

            if (from != joinRequest.PlayerId)
                throw new Exception("Mismatch between player id and envelope id");

            ReceivedJoinRequest?.Invoke(from, joinRequest);

            _logger.LogInformation("Added player {PlayerId}, '{PlayerName}'", from, joinRequest.PlayerName);

            // TODO: Test code sending cards to player
            var holeCards = new HoleCards(new Card(Rank.Ace, Suit.Spades), new Card(Rank.Ace, Suit.Diamonds));
            var envelopeToSend = Envelope.CreateFromEvent(holeCards);
            _router.SendToSingle(from, envelopeToSend);
        };
    }

    public event JoinRequestEventHandler? ReceivedJoinRequest;
}