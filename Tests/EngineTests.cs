using Events;
using Logic;
using Microsoft.Extensions.DependencyInjection;
using ZeroMq;

namespace Tests;

public class EngineTests
{
    private IEngine _engine = null!;
    private MockZeroMq _mockZeroMq = null!;
    private Player _player = null!;

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
        var playerId = _mockZeroMq.CreateClient().Id;
        _player = new Player {Id = playerId, Name = "Player 1"};
        _engine = serviceProvider.GetService<IEngine>()!;
    }

    [Test]
    public void JoinRequest_is_successful_if_engine_is_not_full()
    {
        // Given engine is not full
        // When engine receives JoinRequest
        // Then engine responses with JoinResponse with success status

        var joinRequest = new JoinRequest {PlayerId = _player.Id, PlayerName = _player.Name};
        Server.Handle(Envelope.CreateFromEvent(joinRequest));

        Assert.That(Server.SentToSingleClient, Has.Count.EqualTo(1), "Server has not sent response");
        var joinResponse = Server.SentToSingleClient[0].ExtractEvent() as JoinResponse;
        Assert.That(joinResponse, Is.Not.Null);
        Assert.That(joinResponse!.Status, Is.EqualTo(JoinResponseStatus.Success));
    }
}