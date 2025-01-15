// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Fluxor.Persistence.Blazor.Wasm.Pages.Weather;

[FeatureState]
public sealed record WeatherState
{
    public string SelectedCity { get; init; } = "New York";

    public TemperatureUnit TemperatureUnit { get; init; } = TemperatureUnit.Celsius;

    public WeatherForecast[]? Forecasts { get; init; }

    public bool IsLoading { get; init; }

    public string? ErrorMessage { get; init; }
}


public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}
