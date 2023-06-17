using Events;

namespace ZeroMq;

public class Client
{
    private readonly Dealer _dealer;

    public Client(Dealer dealer)
    {
        ArgumentNullException.ThrowIfNull(dealer);
        _dealer = dealer;
        ClientId = Guid.NewGuid();
    }

    private Guid ClientId { get; }

    public void Configure()
    {
        _dealer.Configure(ClientId);
        _dealer.ReceivedEvent += envelope => { ReceivedUnicastEvent?.Invoke(envelope); };
    }

    public void SendToServer(Envelope envelope) => _dealer.Send(envelope);

    public event Delegates.EnvelopeHandler? ReceivedUnicastEvent;
}