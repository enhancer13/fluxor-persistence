// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Persistence.Strategies;

namespace Fluxor.Persistence;

internal sealed record PersistConfiguration(IReadOnlyList<IPersistenceStrategy> Strategies);
