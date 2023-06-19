using System.Text.Json;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Publisher
{
    private const string Topic = "Table";
    private readonly IConfiguration _configuration;
    private readonly ILogger<Publisher> _logger;
    private PublisherSocket? _publisher;
    private string? _publisherAddress;

    public Publisher(NetMQPoller poller, IConfiguration configuration, ILogger<Publisher> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure()
    {
        _publisherAddress = _configuration.GetValue<string>("PublisherAddress");
        ArgumentException.ThrowIfNullOrEmpty(_publisherAddress);

        _publisher = new PublisherSocket();
        _publisher.Bind(_publisherAddress);
        _logger.LogInformation("Publisher address: {PublisherAddress}", _publisherAddress);
    }

    public void Send(Envelope envelope) => _publisher?.SendMoreFrame(Topic).SendFrame(JsonSerializer.Serialize(envelope));

    public void Unbind() => _publisher?.Unbind(_publisherAddress!);
}