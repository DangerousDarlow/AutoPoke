using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Engine;

public class ZeroMq : IDisposable
{
    // Engine and clients subscribe to the Table topic. Events published to this topic are public and seen by everyone.
    private const string Topic = "Table";

    private readonly IConfiguration _configuration;
    private readonly ILogger<ZeroMq> _logger;
    private NetMQPoller? _poller;
    private SubscriberSocket? _subscriber;

    public ZeroMq(IConfiguration configuration, ILogger<ZeroMq> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Dispose()
    {
        _poller?.Stop();
        _subscriber?.Close();
        _poller?.Dispose();
        _subscriber?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        _logger.LogInformation("Starting ZeroMQ");

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

        _poller = new NetMQPoller {_subscriber};
        _poller.RunAsync();
        _logger.LogInformation("Started poller");
    }
}