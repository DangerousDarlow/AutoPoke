using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using NetMQ;
using NetMQ.Sockets;

namespace ZeroMq;

public class Router : Socket
{
    private RouterSocket? _router;
    private string? _routerAddress;

    public Router(NetMQPoller poller, IOptions<ZeroMqConfiguration> configuration, ILogger<Router> logger) : base(poller, configuration, logger)
    {
    }

    public void Configure()
    {
        _routerAddress = Configuration.Value.RouterAddress;
        ArgumentException.ThrowIfNullOrEmpty(_routerAddress);

        _router = new RouterSocket();
        _router.Bind(_routerAddress);
        Logger.LogInformation("Router address: {RouterAddress}", _routerAddress);

        _router.ReceiveReady += (_, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            var from = new Guid(message[0].ToByteArray());
            var envelope = Envelope.CreateFromJson(message[1].ConvertToString());

            if (envelope.Origin == from)
                ReceivedEvent?.Invoke(envelope);
            else
                Logger.LogWarning("Router received message from {From} but envelope origin is {Origin}", from, envelope.Origin);
        };

        Poller.Add(_router ?? throw new InvalidOperationException("Socket not initialized"));
    }

    public void Send(Guid to, Envelope envelope) => _router?.SendMoreFrame(to.ToByteArray()).SendFrame(JsonSerializer.Serialize(envelope));

    public void Unbind() => _router?.Unbind(_routerAddress!);

    public event EnvelopeHandler? ReceivedEvent;
}