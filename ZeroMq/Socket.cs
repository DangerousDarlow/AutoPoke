using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;

namespace ZeroMq;

public abstract class Socket
{
    public delegate void EnvelopeHandler(Envelope envelope);

    protected readonly IConfiguration Configuration;
    protected readonly ILogger<Socket> Logger;
    protected readonly NetMQPoller Poller;

    protected Socket(NetMQPoller poller, IConfiguration configuration, ILogger<Socket> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        Poller = poller;
        Configuration = configuration;
        Logger = logger;
    }
}