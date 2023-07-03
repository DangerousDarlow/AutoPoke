using Microsoft.Extensions.DependencyInjection;

namespace Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllImplementationsInNamespace<T>(this IServiceCollection services, string @namespace)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var types = assembly
                .GetTypes()
                .Where(p => typeof(T).IsAssignableFrom(p) &&
                            !p.IsInterface &&
                            string.Equals(p.Namespace, @namespace, StringComparison.Ordinal));

            foreach (var type in types)
                services.AddTransient(typeof(T), type);
        }

        return services;
    }
}