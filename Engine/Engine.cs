using Microsoft.Extensions.Logging;
using NetMQ;
using ZeroMq;

namespace Engine;

public class Engine
{
    private readonly ILogger<Engine> _logger;
    private readonly NetMQPoller _poller;
    private readonly Router _router;

    public Engine(NetMQPoller poller, Router router, ILogger<Engine> logger)
    {
        ArgumentNullException.ThrowIfNull(poller);
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(logger);
        _poller = poller;
        _router = router;
        _logger = logger;
    }

    public void Start()
    {
        _router.Configure();
        _poller.RunAsync();
        _logger.LogInformation("Starting Engine");
    }
}