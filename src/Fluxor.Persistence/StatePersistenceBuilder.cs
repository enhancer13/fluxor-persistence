// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Text.Json;
using Fluxor.Persistence.Helpers;
using Fluxor.Persistence.Strategies;

namespace Fluxor.Persistence;

public class StatePersistenceBuilder<TState> where TState : class
{
    private readonly PersistConfigurationBuilder _ParentBuilder;
    private readonly JsonSerializerOptions _SerializerOptions;
    private bool _IsFullStatePersistenceConfigured;
    private readonly HashSet<string> _PersistedPropertyPaths = [];

    internal StatePersistenceBuilder(PersistConfigurationBuilder parentBuilder, JsonSerializerOptions serializerOptions)
    {
        _ParentBuilder = parentBuilder;
        _SerializerOptions = serializerOptions;
    }

    /// <summary>
    /// Configures full state persistence for the current state.
    /// </summary>
    /// <returns>The PersistConfigurationBuilder instance for chaining.</returns>
    public PersistConfigurationBuilder AddFullStatePersistence()
    {
        if (_PersistedPropertyPaths.Count > 0)
        {
            throw new InvalidOperationException($"Cannot add full state persistence for '{typeof(TState).Name}' because specific property persistences have already been configured.");
        }

        if (_IsFullStatePersistenceConfigured)
        {
            throw new InvalidOperationException($"Full state persistence for '{typeof(TState).Name}' has already been configured.");
        }

        _ParentBuilder.AddStrategy(new FullStatePersistenceStrategy<TState>(_SerializerOptions));
        _IsFullStatePersistenceConfigured = true;

        return _ParentBuilder;
    }

    /// <summary>
    /// Configures property persistence for the current state.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <returns>The StatePersistenceBuilder instance for chaining.</returns>
    public StatePersistenceBuilder<TState> AddPropertyPersistence<TProperty>(Expression<Func<TState, TProperty>> propertySelector)
    {
        ArgumentNullException.ThrowIfNull(propertySelector);

        if (_IsFullStatePersistenceConfigured)
        {
            throw new InvalidOperationException($"Cannot add property persistence for '{typeof(TState).Name}' because full state persistence has already been configured.");
        }

        string newPropertyPath = ExpressionHelper.GetPropertyPathString(propertySelector);
        foreach (string existingPath in _PersistedPropertyPaths)
        {
            if (newPropertyPath.StartsWith(existingPath, StringComparison.Ordinal) || existingPath.StartsWith(newPropertyPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Cannot add persistence for '{newPropertyPath}' because it causes overlapping with an existing property path '{existingPath}'.");
            }
        }

        _PersistedPropertyPaths.Add(newPropertyPath);
        _ParentBuilder.AddStrategy(new PropertyPersistenceStrategy<TState, TProperty>(propertySelector, _SerializerOptions));

        return this;
    }
}
