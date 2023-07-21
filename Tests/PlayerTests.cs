using Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Model.Events;
using Shared;
using ZeroMq;
using Action = Model.Action;

namespace Tests;

public class PlayerTests
{
    private IPlayer _player = null!;

    private MockSocket Client { get; set; } = null!;

    [SetUp]
    public void SetUp()
    {
        var configuration = new OptionsWrapper<PlayerConfiguration>(
            new PlayerConfiguration
            {
                Strategy = "TestStrategy"
            });

        var serviceCollection = new ServiceCollection()
            .AddSingleton<MockZeroMq>()
            .AddSingleton<IClient>(provider =>
            {
                var mockZeroMq = provider.GetService<MockZeroMq>()!;
                Client = mockZeroMq.CreateClient();
                return Client;
            })
            .AddSingleton<IOptions<PlayerConfiguration>>(configuration)
            .AddSingleton<IPlayer, Player>()
            .AddAllImplementations<IPlayerEventHandler>()
            .AddAllImplementations<IStrategy>()
            .AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var mockZeroMq = serviceProvider.GetService<MockZeroMq>()!;
        mockZeroMq.CreateServer();

        _player = serviceProvider.GetService<IPlayer>()!;
    }

    [Test]
    public void Player_action_is_delegated_to_strategy_named_in_configuration()
    {
        // Given player with strategy named in configuration
        // When player receives ActionOn for them
        // Then action is determined by strategy
        // And player responds with ActionOnResponse

        Client.Handle(new ActionOn {Player = _player.Id});

        var strategy = _player.Strategy as TestStrategy;
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy!.ActionCalled, Is.True);

        Assert.That(Client.SentToServer, Has.Count.EqualTo(1));
        var actionOnResponse = Client.SentToServer[0] as ActionOnResponse;
        Assert.That(actionOnResponse, Is.Not.Null);
    }

    [Test]
    public void ActionOn_is_not_actioned_if_it_is_not_for_the_player()
    {
        // When the player receives an ActionOn event not for them
        // Then the strategy's Action method is not called

        Client.Handle(new ActionOn());

        var strategy = _player.Strategy as TestStrategy;
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy!.ActionCalled, Is.False);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal class TestStrategy : IStrategy
{
    public bool ActionCalled { get; private set; }

    public string Name => nameof(TestStrategy);

    public IPlayer Player { get; set; } = null!;

    public Action Action()
    {
        ActionCalled = true;
        return Model.Action.Fold;
    }
}