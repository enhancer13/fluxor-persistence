// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Blazor.Web.Components;

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Counter;

public partial class Counter : FluxorComponent
{
    private readonly IState<CounterState> _CounterState;

    private readonly IDispatcher _Dispatcher;

    public Counter(IState<CounterState> counterState, IDispatcher dispatcher)
    {
        _CounterState = counterState;
        _Dispatcher = dispatcher;
    }

    private void IncrementCount()
    {
        _Dispatcher.Dispatch(new IncrementCounterAction());
    }
}
