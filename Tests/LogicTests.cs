using Events;
using Logic;

namespace Tests;

public class LogicTests
{
    private const string PlayerName = "Player1";
    private MockZeroMq _mockZeroMq = null!;
    private Engine _engine = null!;
    private Player _player = null!;

    private MockSocket Server => _mockZeroMq.GetServer();

    private MockSocket ClientFor(Player player) => _mockZeroMq.GetClient(player.Id);

    [SetUp]
    public void SetUp()
    {
        _mockZeroMq = new MockZeroMq();
        _engine = new Engine(_mockZeroMq.CreateServer());
        _player = new Player(PlayerName, _mockZeroMq.CreateClient());
    }

    [Test]
    public void Player_can_join_session()
    {
        // Given server is not full
        // When server receives JoinRequest
        // Client receives JoinResponse with success status

        _player.Join();

        Assert.That(Server.Received, Has.Count.EqualTo(1), "Server has not received request");
        Assert.That(Server.Received[0].ExtractEvent(), Is.TypeOf<JoinRequest>());

        Assert.That(ClientFor(_player).Received, Has.Count.EqualTo(1), "Client has not received response");
        Assert.That(ClientFor(_player).Received[0].ExtractEvent(), Is.TypeOf<JoinResponse>());
        var joinResponse = ClientFor(_player).Received[0].ExtractEvent() as JoinResponse;
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.Success));
    }
}