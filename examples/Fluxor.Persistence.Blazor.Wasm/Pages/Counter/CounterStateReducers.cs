// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Counter;

public static class CounterStateReducers
{
    [ReducerMethod]
    public static CounterState ReduceUpdateUserSettingAction(CounterState state, IncrementCounterAction action) => new() { ClickedCount = state.ClickedCount + 1 };
}
