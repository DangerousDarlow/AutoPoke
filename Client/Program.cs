﻿using Microsoft.Extensions.Configuration;
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
    .ConfigureServices((context, services) =>
    {
        services.Configure<ZeroMqConfiguration>(context.Configuration.GetSection("ZeroMq"));
        services.AddSingleton<NetMQPoller>();
        services.AddSingleton<Dealer>();
        services.AddSingleton<Subscriber>();
        services.AddSingleton<IClient, ZeroMq.Client>(provider =>
        {
            var client = new ZeroMq.Client(provider.GetRequiredService<Dealer>(), provider.GetRequiredService<Subscriber>());
            client.Configure();
            return client;
        });
        services.AddSingleton<Client.Player>(provider => new Client.Player("Player 1", provider.GetRequiredService<IClient>()));
    })
    .Build();

var poller = host.Services.GetService<NetMQPoller>();
poller?.RunAsync();

var player = host.Services.GetService<Client.Player>();
player?.Join();

await host.RunAsync();