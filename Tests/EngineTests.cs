using Model;
using Logic;
using Microsoft.Extensions.DependencyInjection;
using ZeroMq;

namespace Tests;

public class EngineTests
{
    private IEngine _engine = null!;
    private MockZeroMq _mockZeroMq = null!;
    private Dictionary<Guid, Player> _players = null!;

    private MockSocket Server => _mockZeroMq.GetServer();

    [SetUp]
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MockZeroMq>()
            .AddSingleton<IServer>(provider =>
            {
                var mockZeroMq = provider.GetService<MockZeroMq>()!;
                return mockZeroMq.CreateServer();
            })
            .AddSingleton<IClient>(provider =>
            {
                var mockZeroMq = provider.GetService<MockZeroMq>()!;
                return mockZeroMq.CreateClient();
            })
            .AddSingleton<IEngine, Engine>()
            .AddAllImplementations<IEngineEventHandler>()
            .AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _mockZeroMq = serviceProvider.GetService<MockZeroMq>()!;
        _engine = serviceProvider.GetService<IEngine>()!;
        _players = new Dictionary<Guid, Player>();
    }

    private Player CreatePlayer(string name)
    {
        var playerId = _mockZeroMq.CreateClient().Id;
        var player = new Player {Id = playerId, Name = name};
        _players.Add(player.Id, player);
        return player;
    }

    private void CreateAndJoinMaximumPlayers()
    {
        for (var i = 0; i < _engine.Configuration.MaxPlayers; i++)
        {
            var player = CreatePlayer($"Player {i}");
            Server.Handle(Envelope.CreateFromEvent(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name}));
        }
    }

    [Test]
    public void JoinRequest_is_successful_if_engine_is_not_full()
    {
        // Given engine is not full
        // When engine receives JoinRequest
        // Then engine responds with JoinResponse with success status

        var player = CreatePlayer("Player");
        Server.Handle(Envelope.CreateFromEvent(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name}));

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(1), "Server has not sent response");
        var joinResponse = Server.SentToSingleClient[0].ExtractEvent() as JoinResponse;
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.Success));
    }

    [Test]
    public void JoinRequest_is_unsuccessful_if_engine_is_full()
    {
        // Given engine is full
        // When engine receives JoinRequest
        // Then engine responds with JoinResponse with failure status

        CreateAndJoinMaximumPlayers();

        var player = CreatePlayer("Player");
        Server.Handle(Envelope.CreateFromEvent(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name}));

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(_engine.Configuration.MaxPlayers + 1), "Server has not sent response");
        var joinResponse = Server.SentToSingleClient.Last().ExtractEvent() as JoinResponse;
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.FailureEngineFull));
    }
}