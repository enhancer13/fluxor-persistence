// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Weather;

public static class WeatherStateReducers
{
    [ReducerMethod]
    public static WeatherState ReduceFetchWeatherAction(WeatherState state, FetchWeatherAction stateActions) =>
        state with { IsLoading = true, ErrorMessage = null, Forecasts = null };

    [ReducerMethod]
    public static WeatherState ReduceFetchWeatherSuccessAction(WeatherState state, FetchWeatherSuccessAction action) =>
        state with { Forecasts = action.Forecasts, IsLoading = false };

    [ReducerMethod]
    public static WeatherState ReduceFetchWeatherFailureAction(WeatherState state, FetchWeatherFailureAction action) =>
        state with { IsLoading = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static WeatherState ReduceUpdateSelectedCityAction(WeatherState state, UpdateSelectedCityAction action) =>
        state with { SelectedCity = action.NewCity };

    [ReducerMethod]
    public static WeatherState ReduceUpdateTemperatureUnitAction(WeatherState state, UpdateTemperatureUnitAction action) =>
        state with { TemperatureUnit = action.NewUnit };
}
