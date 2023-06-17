using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Subscriber
{
    // Server and clients subscribe to the Table topic. Events published to this topic are public and seen by everyone.
    private const string Topic = "Table";
    private readonly IConfiguration _configuration;
    private readonly ILogger<Subscriber> _logger;
    private readonly NetMQPoller _poller;
    private SubscriberSocket? _subscriber;

    public Subscriber(NetMQPoller poller, IConfiguration configuration, ILogger<Subscriber> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure()
    {
        var subscriptionAddress = _configuration.GetValue<string>("SubscriptionAddress");
        ArgumentException.ThrowIfNullOrEmpty(subscriptionAddress, nameof(subscriptionAddress));

        _subscriber = new SubscriberSocket();
        _subscriber.Connect(subscriptionAddress);
        _logger.LogInformation("Subscription address: {SubscriptionAddress}", subscriptionAddress);

        _subscriber.Subscribe(Topic);
        _subscriber.ReceiveReady += (sender, args) =>
        {
            var topic = args.Socket.ReceiveFrameString();
            if (topic != Topic)
                throw new Exception($"Unexpected topic '{topic}' received");

            var payload = args.Socket.ReceiveFrameString();
            _logger.LogInformation("Received {Message}", payload);
        };
        _logger.LogInformation("Subscribed to topic: {Topic}", Topic);

        _poller.Add(_subscriber ?? throw new InvalidOperationException("Subscriber socket not initialized"));

        _logger.LogInformation("Started poller");
    }
}