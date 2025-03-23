// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Text.Json;
using Fluxor.Persistence.Helpers;
using Fluxor.Persistence.Services;
using Microsoft.Extensions.Logging;

namespace Fluxor.Persistence.Strategies;

internal sealed class PropertyPersistenceStrategy<TState, TProperty> : IPersistenceStrategy where TState : class
{
    private readonly PropertyAccessor<TState, TProperty> _PropertyAccessor;
    private readonly string _StorageKey;
    private readonly JsonSerializerOptions _Options;

    public PropertyPersistenceStrategy(Expression<Func<TState, TProperty>> propertySelector, JsonSerializerOptions options)
    {
        _Options = options;
        _PropertyAccessor = new PropertyAccessor<TState, TProperty>(propertySelector);
        _StorageKey = $"{typeof(TState).Name}.{ExpressionHelper.GetPropertyPathString(propertySelector)}";
    }

    public Type StateType => typeof(TState);

    public async ValueTask LoadAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger)
    {
        TState state = (TState)feature.GetState();
        string? serializedProperty = await persistenceService.GetItemAsStringAsync(_StorageKey).ConfigureAwait(false);
        if (string.IsNullOrEmpty(serializedProperty))
        {
            FluxorLogger.NoPersistedPropertyFound(logger, _StorageKey);
            return;
        }

        TProperty? value = JsonSerializer.Deserialize<TProperty>(serializedProperty, _Options);
        _PropertyAccessor.Setter(state, value);
        feature.RestoreState(state);

        FluxorLogger.RestoredPersistedProperty(logger, typeof(TProperty).Name);
    }

    public async ValueTask SaveAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger)
    {
        TState state = (TState)feature.GetState();
        TProperty? value = _PropertyAccessor.Getter(state);
        string serializedProperty = JsonSerializer.Serialize(value, _Options);
        await persistenceService.SetItemAsStringAsync(_StorageKey, serializedProperty).ConfigureAwait(false);

        FluxorLogger.PersistedProperty(logger, typeof(TProperty).Name);
    }
}
