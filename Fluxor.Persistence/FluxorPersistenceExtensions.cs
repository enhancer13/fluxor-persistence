// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// <param name="serviceLifetime">The service lifetime for IPersistenceService.</param>
    /// <returns>The modified FluxorOptions instance.</returns>
    public static FluxorOptions UsePersist(this FluxorOptions options,
                                           Action<PersistConfigurationBuilder> configure,
                                           ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PersistConfigurationBuilder();
        configure(builder);
        PersistConfiguration config = builder.Build();

        ServiceDescriptor? serviceDescriptor = options.Services.FirstOrDefault(sd => sd.ServiceType == typeof(IPersistenceService));
        if (serviceDescriptor is null)
        {
            options.Services.Add(new ServiceDescriptor(typeof(IPersistenceService), typeof(PersistenceService), serviceLifetime));
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
