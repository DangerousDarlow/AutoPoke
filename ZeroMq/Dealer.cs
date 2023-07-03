using System.Text.Json;
using Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Dealer : Socket
{
    private DealerSocket? _dealer;

    public Dealer(NetMQPoller poller, IOptions<ZeroMqConfiguration> configuration, ILogger<Dealer> logger) : base(poller, configuration, logger)
    {
    }

    public void Configure(Guid playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        var dealerAddress = Configuration.Value.DealerAddress;
        ArgumentException.ThrowIfNullOrEmpty(dealerAddress, nameof(dealerAddress));

        _dealer = new DealerSocket();
        _dealer.Options.Identity = playerId.ToByteArray();
        _dealer.Connect(dealerAddress);
        Logger.LogInformation("Dealer address: {DealerAddress}", dealerAddress);

        _dealer.ReceiveReady += (sender, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            var envelope = Envelope.CreateFromJson(message[0].ConvertToString());
            ReceivedEvent?.Invoke(envelope);
        };

        Poller.Add(_dealer ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public void Send(Envelope envelope) => _dealer?.SendFrame(JsonSerializer.Serialize(envelope));

    public event EnvelopeHandler? ReceivedEvent;
}