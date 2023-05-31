using Events;
using Microsoft.Extensions.Logging;
using NetMQ;
using ZeroMq;

namespace Engine;

public class Engine
{
    private readonly ILogger<Engine> _logger;
    private readonly Dictionary<Guid, JoinRequest> _players = new();
    private readonly NetMQPoller _poller;
    private readonly Router _router;

    public Engine(NetMQPoller poller, Router router, ILogger<Engine> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _router = router;
        _logger = logger;
    }

    public void Start()
    {
        _router.Configure();
        _router.ReceivedEvent += (from, envelope) =>
        {
            if (envelope.ExtractEvent() is not JoinRequest joinRequest)
                return;

            if (from != joinRequest.PlayerId)
                throw new Exception("Mismatch between player id and envelope id");

            _players.Add(from, joinRequest);
            _logger.LogInformation("Added player {PlayerId}, '{PlayerName}'", from, joinRequest.PlayerName);

            // TODO: Test code sending cards to player
            var holeCards = new HoleCards(new Card(Rank.Ace, Suit.Spades), new Card(Rank.King, Suit.Spades));
            var envelopeToSend = Envelope.CreateFromEvent(holeCards);
            _router.SendToSingle(from, envelopeToSend);
        };

        _poller.RunAsync();
        _logger.LogInformation("Starting Engine");
    }
}