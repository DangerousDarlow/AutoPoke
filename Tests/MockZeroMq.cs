﻿using Events;
using ZeroMq;

namespace Tests;

public class MockSocket : IClient, IServer
{
    private readonly MockZeroMq _mockZeroMq;

    public MockSocket(MockZeroMq mockZeroMq)
    {
        _mockZeroMq = mockZeroMq;
    }

    public IList<Envelope> Received { get; } = new List<Envelope>();

    public IList<Envelope> SentToAllClients { get; } = new List<Envelope>();

    public IList<Envelope> SentToSingleClient { get; } = new List<Envelope>();

    public IList<Envelope> SentToServer { get; } = new List<Envelope>();

    public Guid Id { get; } = Guid.NewGuid();

    public event Socket.EnvelopeHandler? ReceivedEvent;

    public void SendToServer(Envelope envelope)
    {
        envelope.Origin = Id;
        SentToServer.Add(envelope);
        _mockZeroMq.SendToServer(envelope);
    }

    public void SendToSingleClient(Envelope envelope, Guid client)
    {
        envelope.Origin = Id;
        SentToSingleClient.Add(envelope);
        _mockZeroMq.SendToSingleClient(envelope, client);
    }

    public void SendToAllClients(Envelope envelope)
    {
        envelope.Origin = Id;
        SentToAllClients.Add(envelope);
        _mockZeroMq.SendToAllClients(envelope);
    }

    public void Handle(Envelope envelope)
    {
        Received.Add(envelope);
        ReceivedEvent?.Invoke(envelope);
    }
}

public class MockZeroMq
{
    private readonly Dictionary<Guid, MockSocket> _clients = new();
    private MockSocket? _server;

    public MockSocket CreateServer()
    {
        _server = new MockSocket(this);
        return _server;
    }

    public MockSocket GetServer() => _server ?? throw new Exception("Server not found");

    public MockSocket CreateClient()
    {
        var client = new MockSocket(this);
        _clients.Add(client.Id, client);
        return client;
    }

    public MockSocket GetClient(Guid clientId) => _clients[clientId] ?? throw new Exception($"Client {clientId} not found");

    public void SendToAllClients(Envelope envelope) => _clients.Values.ToList().ForEach(client => client.Handle(envelope));

    public void SendToSingleClient(Envelope envelope, Guid client) => _clients[client].Handle(envelope);

    public void SendToServer(Envelope envelope) => _server!.Handle(envelope);
}