using Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetMQ;
using Serilog;
using ZeroMq;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

using var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json"))
    .ConfigureServices(services =>
    {
        services.AddSingleton<NetMQPoller>();
        services.AddSingleton<Router>();
        services.AddSingleton<Publisher>();
        services.AddSingleton<IServer, Server>(provider =>
        {
            var server = new Server(provider.GetRequiredService<Router>(), provider.GetRequiredService<Publisher>());
            server.Configure();
            return server;
        });
    })
    .Build();

var server = host.Services.GetService<IServer>();
ArgumentNullException.ThrowIfNull(server);

server.ReceivedEvent += envelope =>
{
    Log.Information("Received: {Value}", envelope.EventType);
};

var poller = host.Services.GetService<NetMQPoller>();
poller?.RunAsync();

await host.RunAsync();