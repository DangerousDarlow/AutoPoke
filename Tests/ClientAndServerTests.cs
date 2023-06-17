using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NetMQ;
using ZeroMq;

namespace Tests;

public class ClientAndServerTests
{
    private Client _client1 = null!;
    private Client _client2 = null!;
    private NetMQPoller? _poller;
    private Server _server = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _poller = new NetMQPoller();
        _poller.RunAsync();

        var engineConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"RouterAddress", "tcp://*:5555"}
            })
            .Build();

        var routerLogger = new Mock<ILogger<Router>>();
        var router = new Router(_poller, engineConfiguration, routerLogger.Object);
        _server = new Server(router);
        _server.Configure();

        var client1Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"DealerAddress", "tcp://localhost:5555"},
                {"ClientName", "Client1"}
            })
            .Build();

        var dealerLogger = new Mock<ILogger<Dealer>>();
        var dealer1 = new Dealer(_poller, client1Configuration, dealerLogger.Object);
        _client1 = new Client(dealer1);
        _client1.Configure();

        var client2Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"DealerAddress", "tcp://localhost:5555"},
                {"ClientName", "Client2"}
            })
            .Build();

        var dealer2 = new Dealer(_poller, client2Configuration, dealerLogger.Object);
        _client2 = new Client(dealer2);
        _client2.Configure();
    }

    [OneTimeTearDown]
    public void TearDown() => NetMQConfig.Cleanup(false);

    [Test]
    public void Unicast_event_sent_by_client_is_received_by_server_only()
    {
        TestEvent? receivedServer = null;
        _server.ReceivedUnicastEvent += envelope => { receivedServer = envelope.ExtractEvent() as TestEvent; };
        
        TestEvent? receivedClient2 = null;
        _client2.ReceivedUnicastEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

        var sent = new TestEvent {Value = "test"};
        _client1.SendToServer(Envelope.CreateFromEvent(sent));

        // Alas a sleep is necessary to allow zero mq to poll
        Thread.Sleep(10);

        Assert.That(receivedServer, Is.Not.Null);
        Assert.That(receivedServer!.Value, Is.EqualTo(sent.Value));
        
        Assert.That(receivedClient2, Is.Null);
    }
}