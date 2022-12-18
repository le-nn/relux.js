using Memento.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Memento.Blazor;

public static class StoreConfigExtension {
    public static IServiceCollection AddMemento(this IServiceCollection services) {
        services.AddScoped(p => new StoreProvider(p));
        return services;
    }

    public static IServiceCollection AddStore<TStore>(this IServiceCollection collection)
        where TStore : class, IStore {
        collection.AddScoped<TStore>()
            .AddScoped<IStore>(p => p.GetRequiredService<TStore>());
        return collection;
    }

    static void ScanAssembyAndAddStores(this IServiceCollection services, Assembly assembly) {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IStore)))) {
            services.AddScoped(type)
                .AddScoped(p => (IStore)p.GetRequiredService(type));
        }
    }

    public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection collection)
        where TMiddleware : Middleware {
        collection.AddScoped<TMiddleware>()
            .AddScoped<Middleware>(p => p.GetRequiredService<TMiddleware>());
        return collection;
    }

    public static StoreProvider GetStoreProvider(this IServiceProvider provider) {
        return provider.GetRequiredService<StoreProvider>();
    }

    public static TMiddleware GetMiddlewarer<TMiddleware>(this IServiceProvider provider)
        where TMiddleware : Middleware {
        return provider.GetRequiredService<TMiddleware>();
    }

    public static TStore GetStore<TStore>(this IServiceProvider provider)
        where TStore : class, IStore {
        return provider.GetRequiredService<TStore>();
    }

    public static WebAssemblyHost UseStores(this WebAssemblyHost builder) {
        _ = builder.Services.GetStoreProvider();
        return builder;
    }
}