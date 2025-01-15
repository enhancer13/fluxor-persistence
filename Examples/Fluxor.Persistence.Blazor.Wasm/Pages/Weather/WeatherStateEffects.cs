// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Json;

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Weather;

public sealed class WeatherStateEffects
{
    private readonly HttpClient _HttpClient;
    private readonly IState<WeatherState> _WeatherState;

    public WeatherStateEffects(HttpClient httpClient, IState<WeatherState> weatherState)
    {
        _HttpClient = httpClient;
        _WeatherState = weatherState;
    }

    [EffectMethod]
    public async Task HandleFetchWeatherAction(FetchWeatherAction action, IDispatcher dispatcher)
    {
        string city = _WeatherState.Value.SelectedCity;
        if (string.IsNullOrWhiteSpace(city))
        {
            dispatcher.Dispatch(new FetchWeatherFailureAction("City is not selected."));
            return;
        }

        try
        {
            WeatherForecast[]? forecasts = await _HttpClient.GetFromJsonAsync<WeatherForecast[]>($"sample-data/weather.json");
            if (forecasts is not null)
            {
                dispatcher.Dispatch(new FetchWeatherSuccessAction(forecasts));
            }
            else
            {
                dispatcher.Dispatch(new FetchWeatherFailureAction("No data received."));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new FetchWeatherFailureAction(ex.Message));
        }
    }
}
