// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Weather;

public partial class Weather : FluxorComponent
{
    private readonly IState<WeatherState> _WeatherState;
    private readonly IDispatcher _Dispatcher;

    public Weather(IState<WeatherState> weatherState, IDispatcher dispatcher)
    {
        _WeatherState = weatherState;
        _Dispatcher = dispatcher;
    }

    protected override void OnInitialized()
    {
        _Dispatcher.Dispatch(new FetchWeatherAction());
        base.OnInitialized();
    }

    private void OnCityChanged(ChangeEventArgs e)
    {
        string newCity = e.Value?.ToString() ?? "New York";
        _Dispatcher.Dispatch(new UpdateSelectedCityAction(newCity));
        _Dispatcher.Dispatch(new FetchWeatherAction());
    }

    private void OnTemperatureUnitChanged(ChangeEventArgs e)
    {
        var selectedUnit = e.Value?.ToString() == "Fahrenheit" ? TemperatureUnit.Fahrenheit : TemperatureUnit.Celsius;
        _Dispatcher.Dispatch(new UpdateTemperatureUnitAction(selectedUnit));
    }
}
