// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace Fluxor.Persistence;

public static partial class FluxorLogger
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Initializing PersistMiddleware")]
    public static partial void InitializingPersistMiddleware(ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to enqueue persistence task for '{StateType}'.")]
    public static partial void FailedToEnqueuePersistenceTask(ILogger logger, string stateType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "PersistMiddleware initialized successfully.")]
    public static partial void PersistMiddlewareInitialized(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Persisted state '{StateName}'.")]
    public static partial void PersistedState(ILogger logger, string stateName);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "An error occurred while persisting state.")]
    public static partial void ErrorPersistingState(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "No persisted state found for '{StateName}'.")]
    public static partial void NoPersistedStateFound(ILogger logger, string stateName);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "No persisted property found for '{StorageKey}'.")]
    public static partial void NoPersistedPropertyFound(ILogger logger, string storageKey);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Loaded persisted property '{PropertyPath}'.")]
    public static partial void RestoredPersistedProperty(ILogger logger, string propertyPath);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Persisted property '{PropertyPath}'.")]
    public static partial void PersistedProperty(ILogger logger, string propertyPath);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "Restored persisted '{StateName}' state.")]
    public static partial void RestoredPersistedState(ILogger logger, string stateName);
}
