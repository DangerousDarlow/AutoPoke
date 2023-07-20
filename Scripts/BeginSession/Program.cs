using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model;
using Model.Events;
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
    .ConfigureAppConfiguration(builder => builder.AddJsonFile(args.Length > 0 ? args[0] : "appsettings.json"))
    .ConfigureServices((context, services) =>
    {
        services.Configure<ZeroMqConfiguration>(context.Configuration.GetSection("ZeroMq"));
        services.AddSingleton<NetMQPoller>();
        services.AddSingleton<Dealer>();
    })
    .Build();

var envelope = Envelope.CreateFromEvent(new BeginSession {Games = 1});
envelope.Origin = Guid.NewGuid();

var dealer = host.Services.GetService<Dealer>();
dealer?.Configure(envelope.Origin);
dealer?.Send(envelope);