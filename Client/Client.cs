using Events;
using Microsoft.Extensions.Logging;
using NetMQ;
using Serilog;
using ZeroMq;

namespace Client;

public class Client
{
    private const string PlayerName = "Client";

    private readonly Dealer _dealer;
    private readonly ILogger<Client> _logger;
    private readonly Guid _playerId;
    private readonly NetMQPoller _poller;

    public Client(NetMQPoller poller, Dealer dealer, ILogger<Client> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _dealer = dealer;
        _logger = logger;
        _playerId = Guid.NewGuid();
    }

    public void Start()
    {
        _dealer.Configure(_playerId);
        _poller.RunAsync();

        _logger.LogInformation("Starting player: id '{PlayerId}', name '{PlayerName}'", _playerId, PlayerName);

        // Sleep to avoid ZeroMQ slow joiner syndrome where a send immediately after a bind is lost
        Thread.Sleep(200);
        
        var joinRequest = new JoinRequest {PlayerId = _playerId, PlayerName = PlayerName};
        var joinRequestEnvelope = Envelope.CreateFromEvent(joinRequest);

        Log.Information("Sending {MessageType}", joinRequestEnvelope.EventTypeStr);
        _dealer.Send(joinRequestEnvelope);
    }
}