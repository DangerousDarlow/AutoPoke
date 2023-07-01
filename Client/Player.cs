using Events;
using ZeroMq;

namespace Client;

public class Player
{
    private readonly IClient _client;

    public Player(string name, IClient client)
    {
        Name = name;
        _client = client;
    }

    private Guid Id => _client.Id;

    private string Name { get; }

    public void Join() => Send(Envelope.CreateFromEvent(new JoinRequest {PlayerId = Id, PlayerName = Name}));

    private void Send(Envelope envelope) => _client.SendToServer(envelope);
}