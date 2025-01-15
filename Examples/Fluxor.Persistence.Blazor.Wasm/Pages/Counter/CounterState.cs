// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Counter;

[FeatureState]
public sealed record CounterState
{
    public int ClickedCount { get; init; }
}
