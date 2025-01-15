// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Services;

/// <summary>
/// Defines a wrapper interface for persistence operations, allowing the persistence mechanism in the library to be overridden.
/// </summary>
public interface IPersistenceService
{
    /// <summary>
    /// Asynchronously retrieves a serialized item from the storage.
    /// </summary>
    /// <param name="storageKey">The key of the item to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the serialized state item.</returns>
    ValueTask<string?> GetItemAsStringAsync(string storageKey);

    /// <summary>
    /// Asynchronously saves a serialized item to the storage.
    /// </summary>
    /// <param name="storageKey">The key of the item to save.</param>
    /// <param name="value">The serialized state item to save.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask SetItemAsStringAsync(string storageKey, string value);
}
