using Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;

namespace ZeroMq;

public abstract class Socket
{
    public delegate void EnvelopeHandler(Envelope envelope);

    protected readonly IOptions<ZeroMqConfiguration> Configuration;
    protected readonly ILogger<Socket> Logger;
    protected readonly NetMQPoller Poller;

    protected Socket(NetMQPoller poller, IOptions<ZeroMqConfiguration> configuration, ILogger<Socket> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        Poller = poller;
        Configuration = configuration;
        Logger = logger;
    }
}