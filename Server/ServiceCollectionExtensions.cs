using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Server;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllImplementations<T>(this IServiceCollection services)
    {
        var types = Assembly.GetAssembly(typeof(T))
            !.GetTypes()
            .Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

        foreach (var type in types)
            services.AddTransient(typeof(T), type);

        return services;
    }
}