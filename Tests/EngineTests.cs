﻿using Model;
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

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(1), "Server has not sent response");
        var joinResponse = Server.SentToSingleClient[0] as JoinResponse;
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

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(_engine.Configuration.MaxPlayers + 1), "Server has not sent response");
        var joinResponse = Server.SentToSingleClient.Last() as JoinResponse;
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.FailureEngineFull));
    }

    [Test]
    public void Session_can_be_started()
    {
        // When engine receives BeginSession
        // Then engine responds to all clients with SessionStarted

        Server.Handle(new BeginSession {Games = 1});
        
        Assert.That(Server.SentToAllClients, Has.Count.EqualTo(1), "Server has not sent response");
        var sessionStarted = Server.SentToAllClients[0] as SessionStarted;
        Assert.That(sessionStarted, Is.Not.Null);
        Assert.That(sessionStarted!.Session.Games, Is.EqualTo(1));
    }
}