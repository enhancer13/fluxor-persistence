// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Blazored.LocalStorage;
using Fluxor.DependencyInjection;
using Fluxor.Persistence.Middlewares;
using Fluxor.Persistence.Services;
using Fluxor.Persistence.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Fluxor.Persistence;

public static class FluxorPersistenceExtensions
{
    /// <summary>
    /// Configures Fluxor to use persistence with the specified configuration.
    /// </summary>
    /// <param name="options">The FluxorOptions instance.</param>
    /// <param name="configure">The action to configure persistence.</param>
    /// <returns>The modified FluxorOptions instance.</returns>
    public static FluxorOptions UsePersist(this FluxorOptions options, Action<PersistConfigurationBuilder> configure) =>
        options.UsePersist(new JsonSerializerOptions(), configure);

    /// <summary>
    /// Configures Fluxor to use persistence with the specified configuration.
    /// </summary>
    /// <param name="options">The FluxorOptions instance.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="configure">The action to configure persistence.</param>
    /// <returns></returns>
    public static FluxorOptions UsePersist(this FluxorOptions options,
                                           JsonSerializerOptions serializerOptions,
                                           Action<PersistConfigurationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PersistConfigurationBuilder(serializerOptions);
        configure(builder);
        PersistConfiguration config = builder.Build();

        ServiceDescriptor? serviceDescriptor = options.Services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceService));
        if (serviceDescriptor is null)
        {
            options.Services.AddScoped<IPersistenceService, PersistenceService>();
        }

        foreach (IPersistenceStrategy strategy in config.Strategies)
        {
            options.Services.AddSingleton(strategy);
        }

        options.AddMiddleware<PersistenceMiddleware>();
        options.Services.AddBlazoredLocalStorage();

        return options;
    }
}
