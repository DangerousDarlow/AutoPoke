using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZeroMq;

namespace Client;

public class Client
{
    private readonly Dealer _dealer;
    private readonly ILogger<Client> _logger;
    private readonly Guid _playerId;
    private readonly string? _playerName;

    private HoleCards? _playerCards;

    public Client(Dealer dealer, IConfiguration configuration, ILogger<Client> logger)
    {
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(logger);
        _dealer = dealer;
        _logger = logger;
        _playerId = Guid.NewGuid();
        _playerName = configuration.GetValue<string>("PlayerName");
    }

    public void Configure()
    {
        _dealer.Configure(_playerId);
        _dealer.ReceivedEvent += envelope =>
        {
            // The only event received directly from the engine should be the players hole cards
            if (envelope.ExtractEvent() is not HoleCards playerCards)
                return;

            _playerCards = playerCards;
            _logger.LogInformation("Received cards {PlayerCards}", _playerCards);
        };
    }

    public void SendJoinRequest()
    {
        _logger.LogInformation("Sending join request for player player: id '{PlayerId}', name '{PlayerName}'", _playerId, _playerName);
        var joinRequest = new JoinRequest {PlayerId = _playerId, PlayerName = _playerName};
        var joinRequestEnvelope = Envelope.CreateFromEvent(joinRequest);
        _dealer.Send(joinRequestEnvelope);
    }
}