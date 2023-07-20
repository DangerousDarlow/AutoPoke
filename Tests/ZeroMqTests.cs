using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using Model.Events;
using Moq;
using NetMQ;
using ZeroMq;

namespace Tests;

public class ZeroMqTests
{
    private static readonly TimeSpan SleepDuration = TimeSpan.FromMilliseconds(10);

    private static readonly IOptions<ZeroMqConfiguration> ClientConfiguration = CreateClientConfiguration();
    private static readonly IOptions<ZeroMqConfiguration> ServerConfiguration = CreateServerConfiguration();

    private ZeroMq.Client _client1 = null!;
    private ZeroMq.Client _client2 = null!;
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

        _router = new Router(_poller, ServerConfiguration, routerLogger.Object);
        _serverPublisher = new Publisher(_poller, ServerConfiguration, publisherLogger.Object);
        _server = new Server(_router, _serverPublisher);
        _server.Configure();

        var dealerLogger = new Mock<ILogger<Dealer>>();

        var client1Dealer = new Dealer(_poller, ClientConfiguration, dealerLogger.Object);
        var client1Subscriber = new Subscriber(_poller, ClientConfiguration, subscriberLogger.Object);
        _client1 = new ZeroMq.Client(client1Dealer, client1Subscriber);
        _client1.Configure();

        var client2Dealer = new Dealer(_poller, ClientConfiguration, dealerLogger.Object);
        var client2Subscriber = new Subscriber(_poller, ClientConfiguration, subscriberLogger.Object);
        _client2 = new ZeroMq.Client(client2Dealer, client2Subscriber);
        _client2.Configure();
    }

    private static IOptions<ZeroMqConfiguration> CreateServerConfiguration() => new OptionsWrapper<ZeroMqConfiguration>(
        new ZeroMqConfiguration
        {
            RouterAddress = "tcp://*:5555",
            PublisherAddress = "tcp://*:5556"
        });

    private static IOptions<ZeroMqConfiguration> CreateClientConfiguration() => new OptionsWrapper<ZeroMqConfiguration>(
        new ZeroMqConfiguration
        {
            DealerAddress = "tcp://localhost:5555",
            SubscriberAddress = "tcp://localhost:5556"
        });

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
        _client1.ReceivedEvent += envelope => { receivedClient1 = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

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
        _client2.ReceivedEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

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
        _client1.ReceivedEvent += envelope => { receivedClient1 = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

        var sent = new TestEvent {Value = Guid.NewGuid().ToString()};
        _server.SendToAllClients(Envelope.CreateFromEvent(sent));

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