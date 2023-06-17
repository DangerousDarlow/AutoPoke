using Events;

namespace ZeroMq;

public class Server
{
    private readonly Router _router;

    public Server(Router router)
    {
        ArgumentNullException.ThrowIfNull(router);
        _router = router;
    }

    public void Configure()
    {
        _router.Configure();
        _router.ReceivedEvent += envelope => { ReceivedUnicastEvent?.Invoke(envelope); };
    }

    public void Send(Envelope envelope, Guid client) => _router.Send(client, envelope);

    public event Delegates.EnvelopeHandler? ReceivedUnicastEvent;
}