﻿using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Publisher : Socket
{
    private const string Topic = "Table";
    private PublisherSocket? _publisher;
    private string? _publisherAddress;

    public Publisher(NetMQPoller poller, IOptions<ZeroMqConfiguration> configuration, ILogger<Publisher> logger) : base(poller, configuration, logger)
    {
    }

    public void Configure()
    {
        _publisherAddress = Configuration.Value.PublisherAddress;
        ArgumentException.ThrowIfNullOrEmpty(_publisherAddress);

        _publisher = new PublisherSocket();
        _publisher.Bind(_publisherAddress);
        Logger.LogInformation("Publisher address: {PublisherAddress}", _publisherAddress);

        Poller.Add(_publisher ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public void Send(Envelope envelope) => _publisher?.SendMoreFrame(Topic).SendFrame(JsonSerializer.Serialize(envelope));

    public void Unbind() => _publisher?.Unbind(_publisherAddress!);
}