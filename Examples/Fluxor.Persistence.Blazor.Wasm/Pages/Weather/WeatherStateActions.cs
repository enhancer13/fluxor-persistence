// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Weather;

public sealed class FetchWeatherAction;

public sealed class FetchWeatherSuccessAction
{
    public WeatherForecast[] Forecasts { get; }

    public FetchWeatherSuccessAction(WeatherForecast[] forecasts)
    {
        Forecasts = forecasts;
    }
}

public sealed class FetchWeatherFailureAction
{
    public string ErrorMessage { get; }

    public FetchWeatherFailureAction(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

public sealed class UpdateSelectedCityAction
{
    public string NewCity { get; }

    public UpdateSelectedCityAction(string newCity)
    {
        NewCity = newCity;
    }
}

public sealed class UpdateTemperatureUnitAction
{
    public TemperatureUnit NewUnit { get; }

    public UpdateTemperatureUnitAction(TemperatureUnit newUnit)
    {
        NewUnit = newUnit;
    }
}
