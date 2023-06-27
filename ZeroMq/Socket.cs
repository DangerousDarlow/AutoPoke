using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;

namespace ZeroMq;

public abstract class Socket
{
    protected readonly NetMQPoller Poller;
    protected readonly IConfiguration Configuration;
    protected readonly ILogger<Socket> Logger;

    public delegate void EnvelopeHandler(Envelope envelope);

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