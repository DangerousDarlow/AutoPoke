using Events;
using ZeroMq;

namespace Logic;

public class Player
{
    private readonly string _name;
    private readonly IClient _client;

    public Player(string name, IClient client)
    {
        _name = name;
        _client = client;
    }

    public Guid Id => _client.Id;

    public void Join() => Send(Envelope.CreateFromEvent(new JoinRequest {PlayerId = Id, PlayerName = _name}));

    private void Send(Envelope envelope) => _client.SendToServer(envelope);
}