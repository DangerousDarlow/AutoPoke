using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Subscriber
{
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
        var subscriberAddress = _configuration.GetValue<string>("SubscriberAddress");
        ArgumentException.ThrowIfNullOrEmpty(subscriberAddress, nameof(subscriberAddress));

        _subscriber = new SubscriberSocket();
        _subscriber.Connect(subscriberAddress);
        _subscriber.Subscribe(Topic);
        _logger.LogInformation("Subscriber address: {SubscriberAddress}", subscriberAddress);

        _subscriber.ReceiveReady += (sender, args) =>
        {
            var topic = args.Socket.ReceiveFrameString();
            if (topic != Topic)
                throw new Exception($"Unexpected topic '{topic}' received");

            var message = args.Socket.ReceiveFrameString();
            var envelope = Envelope.CreateFromJson(message);
            _logger.LogInformation("Received: event {Event}", envelope.EventType);

            ReceivedEvent?.Invoke(envelope);
        };

        _poller.Add(_subscriber ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public event Delegates.EnvelopeHandler? ReceivedEvent;
}