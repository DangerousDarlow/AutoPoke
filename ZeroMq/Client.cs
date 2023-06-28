using Events;

namespace ZeroMq;

public interface IClient
{
    Guid Id { get; }
    void SendToServer(Envelope envelope);
    event Socket.EnvelopeHandler? ReceivedEvent;
}

public class Client : IClient
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
        _dealer.ReceivedEvent += envelope => { ReceivedEvent?.Invoke(envelope); };

        _subscriber.Configure();
        _subscriber.ReceivedEvent += envelope => { ReceivedEvent?.Invoke(envelope); };
    }

    public void SendToServer(Envelope envelope)
    {
        envelope.Origin = Id;
        _dealer.Send(envelope);
    }

    public event Socket.EnvelopeHandler? ReceivedEvent;
}