using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllImplementations<T>(this IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var types = assembly
                .GetTypes()
                .Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

            foreach (var type in types)
                services.AddSingleton(typeof(T), type);
        }

        return services;
    }
}