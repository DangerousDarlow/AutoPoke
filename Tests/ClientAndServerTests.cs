﻿using Events;
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
    private Publisher _client1Publisher = null!;
    private Client _client2 = null!;
    private Publisher _client2Publisher = null!;
    private NetMQPoller _poller = null!;
    private Router _router = null!;
    private Server _server = null!;
    private Publisher _serverPublisher = null!;

    [SetUp]
    public void Setup()
    {
        _poller = new NetMQPoller();
        _poller.RunAsync();

        var serverConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"RouterAddress", "tcp://*:5555"},
                {"PublisherAddress", "tcp://*:5556"},
                {"SubscriberAddress", "tcp://localhost:5556"}
            })
            .Build();

        var routerLogger = new Mock<ILogger<Router>>();
        var publisherLogger = new Mock<ILogger<Publisher>>();
        var subscriberLogger = new Mock<ILogger<Subscriber>>();

        _router = new Router(_poller, serverConfiguration, routerLogger.Object);
        _serverPublisher = new Publisher(_poller, serverConfiguration, publisherLogger.Object);
        var serverSubscriber = new Subscriber(_poller, serverConfiguration, subscriberLogger.Object);
        _server = new Server(_router, _serverPublisher, serverSubscriber);
        _server.Configure();

        var client1Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"DealerAddress", "tcp://localhost:5555"},
                {"PublisherAddress", "tcp://*:5557"},
                {"SubscriberAddress", "tcp://localhost:5556"},
                {"ClientName", "Client1"}
            })
            .Build();

        var dealerLogger = new Mock<ILogger<Dealer>>();

        var client1Dealer = new Dealer(_poller, client1Configuration, dealerLogger.Object);
        _client1Publisher = new Publisher(_poller, client1Configuration, publisherLogger.Object);
        var client1Subscriber = new Subscriber(_poller, client1Configuration, subscriberLogger.Object);
        _client1 = new Client(client1Dealer, _client1Publisher, client1Subscriber);
        _client1.Configure();

        var client2Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"DealerAddress", "tcp://localhost:5555"},
                {"PublisherAddress", "tcp://*:5558"},
                {"SubscriberAddress", "tcp://localhost:5556"},
                {"ClientName", "Client2"}
            })
            .Build();

        var client2Dealer = new Dealer(_poller, client2Configuration, dealerLogger.Object);
        _client2Publisher = new Publisher(_poller, client2Configuration, publisherLogger.Object);
        var client2Subscriber = new Subscriber(_poller, client1Configuration, subscriberLogger.Object);
        _client2 = new Client(client2Dealer, _client2Publisher, client2Subscriber);
        _client2.Configure();
    }

    [TearDown]
    public void TearDown()
    {
        _poller.StopAsync();
        _poller.Dispose();
        _router.Unbind();
        _client1Publisher.Unbind();
        _client2Publisher.Unbind();
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
        _server.ReceivedUnicastEvent += envelope => { receivedServer = envelope.ExtractEvent() as TestEvent; };

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
        TestEvent? receivedServer = null;
        _server.ReceivedMulticastEvent += envelope => { receivedServer = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient1 = null;
        _client1.ReceivedMulticastEvent += envelope => { receivedClient1 = envelope.ExtractEvent() as TestEvent; };

        TestEvent? receivedClient2 = null;
        _client2.ReceivedMulticastEvent += envelope => { receivedClient2 = envelope.ExtractEvent() as TestEvent; };

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

        Assert.That(receivedServer, Is.Null);
    }

    [Test]
    public void Multicast_event_sent_by_client_is_received_by_server_and_other_clients()
    {
    }
}