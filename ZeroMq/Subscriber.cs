using Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Subscriber : Socket
{
    private const string Topic = "Table";
    private readonly TimeSpan _afterConnectSleepDuration = TimeSpan.FromMilliseconds(200);
    private SubscriberSocket? _subscriber;

    public Subscriber(NetMQPoller poller, IOptions<ZeroMqConfiguration> configuration, ILogger<Subscriber> logger) : base(poller, configuration, logger)
    {
    }

    public void Configure()
    {
        var subscriberAddress = Configuration.Value.SubscriberAddress;
        ArgumentException.ThrowIfNullOrEmpty(subscriberAddress, nameof(subscriberAddress));

        _subscriber = new SubscriberSocket();
        _subscriber.Connect(subscriberAddress);

        // Without this sleep the subscription can silently fail (dependant on timing)
        Thread.Sleep(_afterConnectSleepDuration);

        _subscriber.Subscribe(Topic);
        Logger.LogInformation("Subscriber address: {SubscriberAddress}", subscriberAddress);

        _subscriber.ReceiveReady += (sender, args) =>
        {
            var topic = args.Socket.ReceiveFrameString();
            if (topic != Topic)
                throw new Exception($"Unexpected topic '{topic}' received");

            var message = args.Socket.ReceiveFrameString();
            var envelope = Envelope.CreateFromJson(message);

            ReceivedEvent?.Invoke(envelope);
        };

        Poller.Add(_subscriber ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public event EnvelopeHandler? ReceivedEvent;
}