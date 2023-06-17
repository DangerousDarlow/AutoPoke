using System.Text.Json;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Dealer
{
    public delegate void ReceivedEventHandler(Envelope envelope);

    private readonly IConfiguration _configuration;
    private readonly ILogger<Dealer> _logger;
    private readonly NetMQPoller _poller;
    private DealerSocket? _dealer;

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
        var dealerAddress = _configuration.GetValue<string>("DealerAddress");
        ArgumentException.ThrowIfNullOrEmpty(dealerAddress, nameof(dealerAddress));

        _dealer = new DealerSocket();
        _dealer.Options.Identity = playerId.ToByteArray();
        _dealer.Connect(dealerAddress);
        _logger.LogInformation("Dealer address: {DealerAddress}", dealerAddress);

        _dealer.ReceiveReady += (sender, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            var envelope = Envelope.CreateFromJson(message[0].ConvertToString());
            _logger.LogInformation("Received: event {Event}", envelope.EventType);

            ReceivedEvent?.Invoke(envelope);
        };

        _poller.Add(_dealer ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public void Send(Envelope envelope) => _dealer?.SendFrame(JsonSerializer.Serialize(envelope));

    public event ReceivedEventHandler? ReceivedEvent;
}