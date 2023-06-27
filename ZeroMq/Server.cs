using Events;

namespace ZeroMq;

public class Server
{
    private readonly Publisher _publisher;
    private readonly Router _router;

    public Server(Router router, Publisher publisher)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(publisher);
        _router = router;
        _publisher = publisher;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public void Configure()
    {
        _router.Configure();
        _router.ReceivedEvent += envelope => { ReceivedEvent?.Invoke(envelope); };

        _publisher.Configure();
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

    public event Delegates.EnvelopeHandler? ReceivedEvent;
}