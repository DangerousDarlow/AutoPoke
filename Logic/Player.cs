using Events;
using ZeroMq;

namespace Logic;

public class Player
{
    private readonly IClient _client;

    public Player(string name, IClient client)
    {
        Name = name;
        _client = client;
    }

    public Guid Id => _client.Id;

    public string Name { get; }

    public void Join() => Send(Envelope.CreateFromEvent(new JoinRequest {PlayerId = Id, PlayerName = Name}));

    private void Send(Envelope envelope) => _client.SendToServer(envelope);
}