using System.Text.Json;
using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Router
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Router> _logger;
    private readonly NetMQPoller _poller;
    private RouterSocket? _router;
    private string? _routerAddress;

    public Router(NetMQPoller poller, IConfiguration configuration, ILogger<Router> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure()
    {
        _routerAddress = _configuration.GetValue<string>("RouterAddress");
        ArgumentException.ThrowIfNullOrEmpty(_routerAddress);

        _router = new RouterSocket();
        _router.Bind(_routerAddress);
        _logger.LogInformation("Router address: {RouterAddress}", _routerAddress);

        _router.ReceiveReady += (_, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            var from = new Guid(message[0].ToByteArray());
            var envelope = Envelope.CreateFromJson(message[1].ConvertToString());
            _logger.LogInformation("Received: from {From}, event {Event}", from, envelope.EventType);

            ReceivedEvent?.Invoke(envelope);
        };

        _poller.Add(_router ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public void Send(Guid to, Envelope envelope) => _router?.SendMoreFrame(to.ToByteArray()).SendFrame(JsonSerializer.Serialize(envelope));

    public void Unbind() => _router?.Unbind(_routerAddress!);

    public event Delegates.EnvelopeHandler? ReceivedEvent;
}