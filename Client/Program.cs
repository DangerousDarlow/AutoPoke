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
        services.AddSingleton<Client.Client>();
    })
    .Build();

var client = host.Services.GetService<Client.Client>();
ArgumentNullException.ThrowIfNull(client);
client.Start();

await host.RunAsync();