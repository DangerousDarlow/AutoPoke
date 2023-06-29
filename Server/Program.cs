using Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetMQ;
using Serilog;
using ZeroMq;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}"
    )
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
        services.AddSingleton<IEngine, Engine>();
        services.AddAllImplementations<IEngineEventHandler>();
    })
    .Build();

var poller = host.Services.GetService<NetMQPoller>();
poller?.RunAsync();

// Necessary to cause Engine and dependencies to be instantiated
host.Services.GetService<IEngine>();

await host.RunAsync();