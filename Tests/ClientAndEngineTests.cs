using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NetMQ;
using ZeroMq;

namespace Tests;

public class ClientAndEngineTests
{
    [Test]
    public void Join_request_sent_by_client_is_received_by_engine()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"RouterAddress", "tcp://*:5555"},
                {"DealerAddress", "tcp://localhost:5555"},
                {"PlayerName", "Test Player"}
            })
            .Build();

        using var poller = new NetMQPoller();
        poller.RunAsync();

        var routerLogger = new Mock<ILogger<Router>>();
        var engineLogger = new Mock<ILogger<Engine.Engine>>();
        var router = new Router(poller, configuration, routerLogger.Object);
        var engine = new Engine.Engine(router, engineLogger.Object);
        engine.Configure();

        var dealerLogger = new Mock<ILogger<Dealer>>();
        var clientLogger = new Mock<ILogger<Client.Client>>();
        var dealer = new Dealer(poller, configuration, dealerLogger.Object);
        var client = new Client.Client(dealer, configuration, clientLogger.Object);
        client.Configure();

        JoinRequest? joinRequest = null;
        engine.ReceivedJoinRequest += (_, request) => { joinRequest = request; };

        client.SendJoinRequest();

        // Alas a sleep is necessary to allow zero mq to poll
        Thread.Sleep(10);

        Assert.That(joinRequest, Is.Not.Null);
        Assert.That(joinRequest?.PlayerName, Is.EqualTo("Test Player"));
    }
}