using Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

using var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json"))
    .ConfigureServices(services => { services.AddSingleton<ZeroMq>(); })
    .Build();

var zeroMq = host.Services.GetService<ZeroMq>();
zeroMq?.Start();

await host.RunAsync();