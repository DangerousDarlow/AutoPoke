using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Model;
using Model.Events;
using Server;
using Shared;
using ZeroMq;

namespace Tests;

public class EngineTests
{
    private IEngine _engine = null!;
    private MockZeroMq _mockZeroMq = null!;
    private Dictionary<Guid, Player> _players = null!;
    private IServer _server = null!;

    private MockSocket Server => _mockZeroMq.GetServer();

    [SetUp]
    public void SetUp()
    {
        var configuration = new OptionsWrapper<EngineConfiguration>(
            new EngineConfiguration
            {
                HandsPerBlindLevel = 2
            });

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
            .AddSingleton<IOptions<EngineConfiguration>>(configuration)
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
        var player = new Player {Id = playerId, Name = name, Stack = _engine.Configuration.InitialStack};
        _players.Add(player.Id, player);
        return player;
    }

    private void CreateAndJoinMaximumPlayers()
    {
        for (var i = 0; i < _engine.Configuration.MaximumNumberOfPlayers; i++)
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

        HandleFromSelf(new BeginGame());

        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(2), "Engine has not sent responses");

        var gameStarted = Server.SentToAllClients[0] as GameStarted;
        Assert.That(gameStarted, Is.Not.Null);

        var handStarted = Server.SentToAllClients[1] as HandStarted;
        Assert.That(handStarted, Is.Not.Null);
    }

    private void HandleFromSelf<T>(T @event) where T : IEvent
    {
        var envelope = Envelope.CreateFromEvent(@event);
        envelope.Origin = _server.Id;
        Server.Handle(envelope);
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

        HandleFromSelf(new BeginHand());

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
        Assert.Multiple(() =>
        {
            Assert.That(handStarted!.Hand.Players, Is.EquivalentTo(_players.Values));
            Assert.That(handStarted.Hand.Sequence, Is.EqualTo(1));
            Assert.That(handStarted.Hand.SmallBlind, Is.EqualTo(_engine.Configuration.InitialSmallBlind));
            Assert.That(handStarted.Hand.BigBlind, Is.EqualTo(handStarted.Hand.SmallBlind * 2));
        });
    }

    [Test]
    public void BeginHand_is_ignored_if_origin_is_not_engine()
    {
        // When engine receives BeginHand and origin is not engine
        // Then BeginHand is ignored

        Server.Handle(new BeginHand());
        Assert.That(Server.SentToAllClients, Is.Empty, "Engine has not ignored BeginHand");
    }

    [Test]
    public void Players_rotate_with_every_hand()
    {
        // When engine receives BeginHand
        // Players are rotated so the first player is now the last player

        CreateAndJoinMaximumPlayers();

        HandleFromSelf(new BeginHand());

        var firstHandStarted = Server.SentToAllClients.LastOrDefault(@event => @event.GetType() == typeof(HandStarted)) as HandStarted;
        Assert.That(firstHandStarted, Is.Not.Null);
        var firstPlayers = firstHandStarted!.Hand.Players;
        Assert.That(firstPlayers, Is.Not.Null);
        Assert.That(firstPlayers, Has.Count.EqualTo(_players.Count));

        HandleFromSelf(new BeginHand());

        var secondHandStarted = Server.SentToAllClients.LastOrDefault(@event => @event.GetType() == typeof(HandStarted)) as HandStarted;
        Assert.That(secondHandStarted, Is.Not.Null);
        var secondPlayers = secondHandStarted!.Hand.Players;
        Assert.That(secondPlayers, Is.Not.Null);
        Assert.That(secondPlayers, Has.Count.EqualTo(_players.Count));

        Assert.Multiple(() =>
        {
            Assert.That(_players, Has.Count.EqualTo(10));
            Assert.That(firstPlayers[0].Id, Is.EqualTo(secondPlayers[9].Id));
            Assert.That(firstPlayers[1].Id, Is.EqualTo(secondPlayers[0].Id));
            Assert.That(firstPlayers[2].Id, Is.EqualTo(secondPlayers[1].Id));
            Assert.That(firstPlayers[3].Id, Is.EqualTo(secondPlayers[2].Id));
            Assert.That(firstPlayers[4].Id, Is.EqualTo(secondPlayers[3].Id));
            Assert.That(firstPlayers[5].Id, Is.EqualTo(secondPlayers[4].Id));
            Assert.That(firstPlayers[6].Id, Is.EqualTo(secondPlayers[5].Id));
            Assert.That(firstPlayers[7].Id, Is.EqualTo(secondPlayers[6].Id));
            Assert.That(firstPlayers[8].Id, Is.EqualTo(secondPlayers[7].Id));
            Assert.That(firstPlayers[9].Id, Is.EqualTo(secondPlayers[8].Id));
        });
    }

    [Test]
    public void Blinds_change_after_a_configurable_number_of_hands()
    {
        // When engine receives BeginHand
        // Hand sequence is incremented
        // And small blind is incremented after a configurable number of hands

        CreateAndJoinMaximumPlayers();

        Assert.That(_engine.Configuration.HandsPerBlindLevel, Is.EqualTo(2));

        // First hand
        HandleFromSelf(new BeginHand());
        AssertLastHandSequenceAndBlinds(1, _engine.Configuration.InitialSmallBlind);

        // Second hand
        HandleFromSelf(new BeginHand());
        AssertLastHandSequenceAndBlinds(2, _engine.Configuration.InitialSmallBlind);

        // Third hand
        HandleFromSelf(new BeginHand());
        AssertLastHandSequenceAndBlinds(3, _engine.Configuration.InitialSmallBlind * 2);
    }

    private void AssertLastHandSequenceAndBlinds(int sequence, int smallBlind)
    {
        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(sequence));
        var handStarted = Server.SentToAllClients.Last() as HandStarted;
        Assert.That(handStarted, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handStarted!.Hand.Sequence, Is.EqualTo(sequence));
            Assert.That(handStarted.Hand.SmallBlind, Is.EqualTo(smallBlind));
            Assert.That(handStarted.Hand.BigBlind, Is.EqualTo(handStarted.Hand.SmallBlind * 2));
        });
    }
}