using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NetMQ;
using ZeroMq;

namespace Tests;

public class ClientAndServerTests
{
    private static readonly TimeSpan SleepDuration = TimeSpan.FromMilliseconds(10);

    private Client _client1 = null!;
    private Client _client2 = null!;
    private NetMQPoller _poller = null!;
    private Router _router = null!;
    private Server _server = null!;
    private Publisher _serverPublisher = null!;

    [SetUp]
    public void SetUp()
    {
        _poller = new NetMQPoller();
        _poller.RunAsync();

        var routerLogger = new Mock<ILogger<Router>>();
        var publisherLogger = new Mock<ILogger<Publisher>>();
        var subscriberLogger = new Mock<ILogger<Subscriber>>();

        var serverConfiguration = CreateServerConfiguration();
        _router = new Router(_poller, serverConfiguration, routerLogger.Object);
        _serverPublisher = new Publisher(_poller, serverConfiguration, publisherLogger.Object);
        _server = new Server(_router, _serverPublisher);
        _server.Configure();

        var dealerLogger = new Mock<ILogger<Dealer>>();

        var client1Configuration = CreateClientConfiguration("Client2");
        var client1Dealer = new Dealer(_poller, client1Configuration, dealerLogger.Object);
        var client1Subscriber = new Subscriber(_poller, client1Configuration, subscriberLogger.Object);
        _client1 = new Client(client1Dealer, client1Subscriber);
        _client1.Configure();

        var client2Configuration = CreateClientConfiguration("Client2");
        var client2Dealer = new Dealer(_poller, client2Configuration, dealerLogger.Object);
        var client2Subscriber = new Subscriber(_poller, client1Configuration, subscriberLogger.Object);
        _client2 = new Client(client2Dealer, client2Subscriber);
        _client2.Configure();
    }

    private static IConfiguration CreateServerConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"RouterAddress", "tcp://*:5555"},
            {"PublisherAddress", "tcp://*:5556"}
        })
        .Build();

    private static IConfiguration CreateClientConfiguration(string clientName) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"DealerAddress", "tcp://localhost:5555"},
            {"SubscriberAddress", "tcp://localhost:5556"},
            {"ClientName", clientName}
        })
        .Build();

    [TearDown]
    public void TearDown()
    {
        _poller.StopAsync();
        _poller.Dispose();
        _router.Unbind();
        _serverPublisher.Unbind();
    }

    [Test]
    public void Unicast_event_sent_by_server_is_received_by_single_client_only()
    {
        TestEvent? receivedClient1 = null;
        _client1.ReceivedUnicastEvent += envelope => { receivedClient1 = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedUnicastEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

        var sent = new TestEvent {Value = Guid.NewGuid().ToString()};
        _server.SendToSingleClient(Envelope.CreateFromEvent(sent), _client1.Id);

        // Alas a sleep is necessary to allow zero mq to poll
        Thread.Sleep(SleepDuration);

        Assert.That(receivedClient1, Is.Not.Null);
        Assert.That(receivedClient1!.Value, Is.Not.Null);
        Assert.That(receivedClient1.Value, Is.EqualTo(sent.Value));

        Assert.That(receivedClient2, Is.Null);
    }

    [Test]
    public void Unicast_event_sent_by_client_is_received_by_server_only()
    {
        TestEvent? receivedServer = null;
        _server.ReceivedEvent += envelope => { receivedServer = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedUnicastEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

        var sent = new TestEvent {Value = Guid.NewGuid().ToString()};
        _client1.SendToServer(Envelope.CreateFromEvent(sent));

        // Alas a sleep is necessary to allow zero mq to poll
        Thread.Sleep(SleepDuration);

        Assert.That(receivedServer, Is.Not.Null);
        Assert.That(receivedServer!.Value, Is.Not.Null);
        Assert.That(receivedServer.Value, Is.EqualTo(sent.Value));

        Assert.That(receivedClient2, Is.Null);
    }

    [Test]
    public void Multicast_event_sent_by_server_is_received_by_clients()
    {
        TestEvent? receivedClient1 = null;
        _client1.ReceivedMulticastEvent += envelope => { receivedClient1 = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedMulticastEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

        var sent = new TestEvent {Value = Guid.NewGuid().ToString()};
        _server.SendToAll(Envelope.CreateFromEvent(sent));

        // Alas a sleep is necessary to allow zero mq to poll
        Thread.Sleep(SleepDuration);

        Assert.That(receivedClient1, Is.Not.Null);
        Assert.That(receivedClient1!.Value, Is.Not.Null);
        Assert.That(receivedClient1.Value, Is.EqualTo(sent.Value));

        Assert.That(receivedClient2, Is.Not.Null);
        Assert.That(receivedClient2!.Value, Is.Not.Null);
        Assert.That(receivedClient2.Value, Is.EqualTo(sent.Value));
    }
}