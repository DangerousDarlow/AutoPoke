﻿using Events;
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
        services.AddSingleton<Server>();
    })
    .Build();

var server = host.Services.GetService<Server>();
ArgumentNullException.ThrowIfNull(server);
server.Configure();

server.ReceivedEvent += envelope =>
{
    var testEvent = envelope.ExtractEvent() as TestEvent;
    Log.Information("Received: {Value}", testEvent?.Value);
};

var poller = host.Services.GetService<NetMQPoller>();
poller?.RunAsync();

await host.RunAsync();