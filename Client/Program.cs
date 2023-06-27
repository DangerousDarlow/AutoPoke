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
        services.AddSingleton<Dealer>();
        services.AddSingleton<Subscriber>();
        services.AddSingleton<Client>();
    })
    .Build();

var client = host.Services.GetService<Client>();
ArgumentNullException.ThrowIfNull(client);
client.Configure();
client.SendToServer(Envelope.CreateFromEvent(new TestEvent {Value = "Hello Server"}));

var poller = host.Services.GetService<NetMQPoller>();
poller?.RunAsync();

await host.RunAsync();