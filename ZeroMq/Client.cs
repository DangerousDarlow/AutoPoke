using Events;

namespace ZeroMq;

public class Client
{
    private readonly Dealer _dealer;
    private readonly Subscriber _subscriber;

    public Client(Dealer dealer, Subscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(dealer);
        ArgumentNullException.ThrowIfNull(subscriber);
        _dealer = dealer;
        _subscriber = subscriber;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public void Configure()
    {
        _dealer.Configure(Id);
        _dealer.ReceivedEvent += envelope => { ReceivedUnicastEvent?.Invoke(envelope); };

        _subscriber.Configure();
        _subscriber.ReceivedEvent += envelope => { ReceivedMulticastEvent?.Invoke(envelope); };
    }

    public void SendToServer(Envelope envelope)
    {
        envelope.Origin = Id;
        _dealer.Send(envelope);
    }

    public event Socket.EnvelopeHandler? ReceivedUnicastEvent;

    public event Socket.EnvelopeHandler? ReceivedMulticastEvent;
}