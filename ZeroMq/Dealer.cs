using System.Text.Json;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Dealer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Dealer> _logger;
    private readonly NetMQPoller _poller;
    private DealerSocket? _dealer;
    private Guid _playerId;

    public Dealer(NetMQPoller poller, IConfiguration configuration, ILogger<Dealer> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure(Guid playerId)
    {
        _playerId = playerId;
        var dealerAddress = _configuration.GetValue<string>("DealerAddress");
        ArgumentException.ThrowIfNullOrEmpty(dealerAddress, nameof(dealerAddress));

        _dealer = new DealerSocket();
        _dealer.Options.Identity = playerId.ToByteArray();
        _dealer.Connect(dealerAddress);
        _logger.LogInformation("Dealer address: {RouterAddress}", dealerAddress);

        _dealer.ReceiveReady += (sender, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            _logger.LogInformation("Dealer received from {From}", new Guid(message[0].ToByteArray()));
        };

        _poller.Add(_dealer ?? throw new InvalidOperationException("Router socket not initialized"));
    }

    public void Send(Envelope envelope) =>
        _dealer?.SendMoreFrame(_playerId.ToByteArray()).SendFrame(JsonSerializer.Serialize(envelope));
}