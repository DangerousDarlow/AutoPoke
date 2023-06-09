﻿using Model;

namespace ZeroMq;

public interface IServer
{
    Guid Id { get; }
    void SendToSingleClient(Envelope envelope, Guid client);
    void SendToAllClients(Envelope envelope);
    event Socket.EnvelopeHandler? ReceivedEvent;
}

public class Server : IServer
{
    private readonly Publisher _publisher;
    private readonly Router _router;

    public Server(Router router, Publisher publisher)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(publisher);
        _router = router;
        _publisher = publisher;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public void SendToSingleClient(Envelope envelope, Guid client)
    {
        envelope.Origin = Id;
        _router.Send(client, envelope);
    }

    public void SendToAllClients(Envelope envelope)
    {
        envelope.Origin = Id;
        _publisher.Send(envelope);
    }

    public event Socket.EnvelopeHandler? ReceivedEvent;

    public void Configure()
    {
        _router.Configure();
        _router.ReceivedEvent += envelope => { ReceivedEvent?.Invoke(envelope); };

        _publisher.Configure();
    }
}