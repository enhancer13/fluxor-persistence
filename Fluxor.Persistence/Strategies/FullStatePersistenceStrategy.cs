// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Fluxor.Persistence.Services;
using Microsoft.Extensions.Logging;

namespace Fluxor.Persistence.Strategies;

internal sealed class FullStatePersistenceStrategy<TState> : IPersistenceStrategy where TState : class
{
    private readonly string _StorageKey = typeof(TState).Name;

    public Type StateType => typeof(TState);

    public async ValueTask LoadAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger)
    {
        string? serializedState = await persistenceService.GetItemAsStringAsync(_StorageKey).ConfigureAwait(false);
        if (string.IsNullOrEmpty(serializedState))
        {
            FluxorLogger.NoPersistedStateFound(logger, typeof(TState).Name);
            return;
        }

        TState? persistedState =  JsonSerializer.Deserialize<TState>(serializedState);
        if (persistedState is not null)
        {
            feature.RestoreState(persistedState);
        }

        FluxorLogger.RestoredPersistedState(logger, typeof(TState).Name);
    }

    public async ValueTask SaveAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger)
    {
        TState state = (TState)feature.GetState();
        string serializedState = JsonSerializer.Serialize(state);
        await persistenceService.SetItemAsStringAsync(_StorageKey, serializedState).ConfigureAwait(false);

        FluxorLogger.PersistedState(logger, typeof(TState).Name);
    }
}
