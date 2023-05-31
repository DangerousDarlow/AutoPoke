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

    public Router(NetMQPoller poller, IConfiguration configuration, ILogger<Router> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        _poller = poller;
        _configuration = configuration;
        _logger = logger;
    }

    public void Configure()
    {
        var routerAddress = _configuration.GetValue<string>("RouterAddress");
        ArgumentException.ThrowIfNullOrEmpty(routerAddress, nameof(routerAddress));

        _router = new RouterSocket();
        _router.Bind(routerAddress);
        _logger.LogInformation("Router address: {RouterAddress}", routerAddress);

        _router.ReceiveReady += (sender, args) =>
        {
            var message = args.Socket.ReceiveMultipartMessage();
            _logger.LogInformation("Router from {From}", new Guid(message[0].ToByteArray()));
        };

        _poller.Add(_router ?? throw new InvalidOperationException("Router socket not initialized"));
    }
}