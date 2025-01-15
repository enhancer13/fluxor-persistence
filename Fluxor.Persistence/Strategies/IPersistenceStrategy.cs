// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Persistence.Services;
using Microsoft.Extensions.Logging;

namespace Fluxor.Persistence.Strategies;

internal interface IPersistenceStrategy
{
    Type StateType { get; }

    ValueTask LoadAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger);

    ValueTask SaveAsync(IFeature feature, IPersistenceService persistenceService, ILogger logger);
}
