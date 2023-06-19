using Events;

namespace ZeroMq;

public class Client
{
    private readonly Dealer _dealer;
    private readonly Publisher _publisher;
    private readonly Subscriber _subscriber;

    public Client(Dealer dealer, Publisher publisher, Subscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(subscriber);
        _dealer = dealer;
        _publisher = publisher;
        _subscriber = subscriber;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public void Configure()
    {
        _dealer.Configure(Id);
        _dealer.ReceivedEvent += envelope => { ReceivedUnicastEvent?.Invoke(envelope); };
        
        _publisher.Configure();

        _subscriber.Configure();
        _subscriber.ReceivedEvent += envelope => { ReceivedMulticastEvent?.Invoke(envelope); };
    }

    public void SendToServer(Envelope envelope) => _dealer.Send(envelope);

    public event Delegates.EnvelopeHandler? ReceivedUnicastEvent;

    public event Delegates.EnvelopeHandler? ReceivedMulticastEvent;
}