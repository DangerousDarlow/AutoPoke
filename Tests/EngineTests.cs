using Logic;
using Microsoft.Extensions.DependencyInjection;
using Model;
using ZeroMq;

namespace Tests;

public class EngineTests
{
    private IEngine _engine = null!;
    private IServer _server = null!;
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
                _server = mockZeroMq.CreateServer();
                return _server;
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
            Server.Handle(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name});
        }
    }

    [Test]
    public void Player_can_join_if_not_full()
    {
        // Given engine is not full
        // When engine receives JoinRequest from client
        // Then engine responds to client with JoinResponse with success status

        var player = CreatePlayer("Player");
        Server.Handle(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name});

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(1), "Engine has not sent response");
        var (@event, client) = Server.SentToSingleClient[0];
        var joinResponse = @event as JoinResponse;
        Assert.That(client, Is.EqualTo(player.Id));
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.Success));
    }

    [Test]
    public void Player_cannot_join_if_full()
    {
        // Given engine is full
        // When engine receives JoinRequest from client
        // Then engine responds to client with JoinResponse with failure status

        CreateAndJoinMaximumPlayers();

        var player = CreatePlayer("Player");
        Server.Handle(new JoinRequest {PlayerId = player.Id, PlayerName = player.Name});

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(_players.Count));
        var (@event, client) = Server.SentToSingleClient.Last();
        var joinResponse = @event as JoinResponse;
        Assert.That(client, Is.EqualTo(player.Id));
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.FailureEngineFull));
    }

    [Test]
    public void Session_can_be_started()
    {
        // When engine receives BeginSession
        // Then engine responds to all clients with SessionStarted and GameStarted

        CreateAndJoinMaximumPlayers();

        Server.Handle(new BeginSession {Games = 1});

        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(3), "Engine has not sent responses");

        var sessionStarted = Server.SentToAllClients[0] as SessionStarted;
        Assert.That(sessionStarted, Is.Not.Null);
        Assert.That(sessionStarted!.Session.Games, Is.EqualTo(1));

        var gameStarted = Server.SentToAllClients[1] as GameStarted;
        Assert.That(gameStarted, Is.Not.Null);

        var handStarted = Server.SentToAllClients[2] as HandStarted;
        Assert.That(handStarted, Is.Not.Null);
    }

    [Test]
    public void Session_cannot_be_started_if_already_started()
    {
        // Given a session has started
        // When engine receives BeginSession
        // Then engine does not respond

        Server.Handle(new BeginSession {Games = 1});
        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(3), "Engine has not sent responses");

        Server.Handle(new BeginSession {Games = 1});
        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(3), "Additional responses sent");
    }

    [Test]
    public void Game_can_be_started()
    {
        // When engine receives BeginGame and origin is engine
        // Then engine responds to all clients with GameStarted

        CreateAndJoinMaximumPlayers();

        var envelope = Envelope.CreateFromEvent(new BeginGame());
        envelope.Origin = _server.Id;
        Server.Handle(envelope);

        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(2), "Engine has not sent responses");

        var gameStarted = Server.SentToAllClients[0] as GameStarted;
        Assert.That(gameStarted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(gameStarted!.Game.Players, Is.EquivalentTo(_players.Values));
            Assert.That(gameStarted.Game.StartingStack, Is.EqualTo(_engine.Configuration.StartingStack));
        });

        var handStarted = Server.SentToAllClients[1] as HandStarted;
        Assert.That(handStarted, Is.Not.Null);
    }

    [Test]
    public void BeginGame_is_ignored_if_origin_is_not_engine()
    {
        // When engine receives BeginGame and origin is not engine
        // Then BeginGame is ignored

        Server.Handle(new BeginGame());
        Assert.That(Server.SentToAllClients, Is.Empty, "Engine has not ignored BeginGame");
    }

    [Test]
    public void Hand_can_be_started()
    {
        // When engine receives BeginHand and origin is engine
        // Then engine responds to each client with HoleCards
        // And engine responds to all clients with HandStarted

        CreateAndJoinMaximumPlayers();

        var envelope = Envelope.CreateFromEvent(new BeginHand());
        envelope.Origin = _server.Id;
        Server.Handle(envelope);

        // JoinResponse & HoleCards for each player
        var expectedNumberOfEventsSendToSingleClients = _players.Count * 2;
        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(expectedNumberOfEventsSendToSingleClients));

        Assert.Multiple(() =>
        {
            var allHoleCards = new HashSet<Card>();

            foreach (var player in _players.Values)
            {
                var playerEvents = Server.SentToSingleClient.Where(tuple => tuple.Item2 == player.Id).ToList();
                Assert.That(playerEvents, Has.Count.EqualTo(2));
                var holeCards = playerEvents[1].Item1 as HoleCards;
                Assert.That(holeCards, Is.Not.Null, $"Player {player.Name} has not received HoleCards");
                allHoleCards.Add(holeCards!.Card1);
                allHoleCards.Add(holeCards!.Card2);
            }

            Assert.That(allHoleCards, Has.Count.EqualTo(_players.Count * 2), "HoleCards are not all unique");
        });

        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(1), "Engine has not sent responses");
        var handStarted = Server.SentToAllClients[0] as HandStarted;
        Assert.That(handStarted, Is.Not.Null);
    }

    [Test]
    public void BeginHand_is_ignored_if_origin_is_not_engine()
    {
        // When engine receives BeginHand and origin is not engine
        // Then BeginHand is ignored

        Server.Handle(new BeginHand());
        Assert.That(Server.SentToAllClients, Is.Empty, "Engine has not ignored BeginHand");
    }
}