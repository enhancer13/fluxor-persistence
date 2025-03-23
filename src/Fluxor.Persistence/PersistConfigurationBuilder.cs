// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Fluxor.Persistence.Strategies;

namespace Fluxor.Persistence;

public sealed class PersistConfigurationBuilder
{
    private readonly List<IPersistenceStrategy> _Strategies = [];
    private readonly JsonSerializerOptions _SerializerOptions;

    public PersistConfigurationBuilder(JsonSerializerOptions serializerOptions)
    {
        _SerializerOptions = serializerOptions;
    }

    /// <summary>
    /// Configures persistence for a specific state.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <returns>A StatePersistenceBuilder instance for configuring the specified state.</returns>
    public StatePersistenceBuilder<TState> ForState<TState>() where TState : class
    {
        return new StatePersistenceBuilder<TState>(this, _SerializerOptions);
    }

    internal void AddStrategy(IPersistenceStrategy strategy)
    {
        _Strategies.Add(strategy);
    }

    internal PersistConfiguration Build()
    {
        return new PersistConfiguration(_Strategies.AsReadOnly());
    }
}
