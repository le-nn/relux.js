using Memento.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Memento.Blazor;

/// <summary>
/// Extension methods for configuring store-related services.
/// </summary>
public static class StoreConfigExtension {
    /// <summary>
    /// Adds the Memento service to the IServiceCollection.
    /// </summary>
    public static IServiceCollection AddMemento(this IServiceCollection services, bool isScoped = false) {
        if (isScoped) {
            services.AddScoped<StoreProvider>();
        }
        else {
            services.AddSingleton<StoreProvider>();
        }
        return services;
    }

    /// <summary>
    /// Adds a custom store to the IServiceCollection.
    /// </summary>
    public static IServiceCollection AddStore<TStore>(this IServiceCollection collection, bool isScoped = false)
        where TStore : class, IStore {
        if (isScoped) {
            collection.AddScoped<TStore>()
                .AddScoped<IStore>(p => p.GetRequiredService<TStore>());
        }
        else {
            collection.AddSingleton<TStore>()
                .AddSingleton<IStore>(p => p.GetRequiredService<TStore>());
        }
        return collection;
    }

    /// <summary>
    /// Scans the assembly and adds all stores that implements <see cref="IStore"/> to the IServiceCollection.
    /// </summary>
    public static void ScanAssemblyAndAddStores(this IServiceCollection services, Assembly assembly, bool isScoped = false) {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IStore)))) {
            if (isScoped) {
                services.AddScoped(type)
                    .AddScoped(p => (IStore)p.GetRequiredService(type));
            }
            else {
                services.AddSingleton(type)
                    .AddSingleton(p => (IStore)p.GetRequiredService(type));
            }
        }
    }

    /// <summary>
    /// Adds a custom middleware to the IServiceCollection.
    /// </summary>
    public static IServiceCollection AddMiddleware<TMiddleware>(
        this IServiceCollection collection,
        Func<TMiddleware> middlewareSelector,
              bool isScoped = false
    ) where TMiddleware : Middleware {
        if (isScoped) {
            collection.AddScoped<Middleware>(p => middlewareSelector());
        }
        else {
            collection.AddSingleton<Middleware>(p => middlewareSelector());
        }
        return collection;
    }

    /// <summary>
    /// Adds a custom middleware to the IServiceCollection.
    /// </summary>
    public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection collection, bool isScoped = false)
        where TMiddleware : Middleware {
        if (isScoped) {
            collection.AddScoped<TMiddleware>();
            collection.AddScoped<Middleware, TMiddleware>(p => p.GetRequiredService<TMiddleware>());
        }
        else {
            collection.AddSingleton<TMiddleware>();
            collection.AddSingleton<Middleware, TMiddleware>(p => p.GetRequiredService<TMiddleware>());
        }
        return collection;
    }

    /// <summary>
    /// Gets the StoreProvider from the IServiceProvider.
    /// </summary>
    public static StoreProvider GetStoreProvider(this IServiceProvider provider) {
        return provider.GetRequiredService<StoreProvider>();
    }
}
