using Events;

namespace ZeroMq;

public class Server
{
    private readonly Router _router;
    private readonly Publisher _publisher;
    private readonly Subscriber _subscriber;

    public Server(Router router, Publisher publisher, Subscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(subscriber);
        _router = router;
        _publisher = publisher;
        _subscriber = subscriber;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public void Configure()
    {
        _router.Configure();
        _router.ReceivedEvent += envelope => { ReceivedUnicastEvent?.Invoke(envelope); };

        _publisher.Configure();

        _subscriber.Configure();
        _subscriber.ReceivedEvent += envelope =>
        {
            if (envelope.Origin != Id)
                ReceivedMulticastEvent?.Invoke(envelope);
        };
    }

    public void SendToSingleClient(Envelope envelope, Guid client)
    {
        envelope.Origin = Id;
        _router.Send(client, envelope);
    }

    public void SendToAll(Envelope envelope)
    {
        envelope.Origin = Id;
        _publisher.Send(envelope);
    }

    public event Delegates.EnvelopeHandler? ReceivedUnicastEvent;

    public event Delegates.EnvelopeHandler? ReceivedMulticastEvent;
}