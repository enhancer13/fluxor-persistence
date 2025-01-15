# Fluxor.Persistence.Extensions

![Logo](docs/images/enhancer13_logo_inverted.png)

Fluxor.Persistence.Extensions is a seamless extension to [Fluxor](https://github.com/mrpmorris/Fluxor) that introduces state persistence. It allows you to save and restore your application's state using your preferred storage mechanism.

[![android build](https://github.com/enhancer13/fluxor-persistence/actions/workflows/dotnet_build.yml/badge.svg?branch=main)](https://github.com/enhancer13/fluxor-persistence/actions/workflows/dotnet_build.yml?branch=main)

[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=bugs)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=enhancer13_fluxor-persistence)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=enhancer13_fluxor-persistence&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=enhancer13_navi-home-client)

## Getting Started

### Installation

You can install the package via NuGet:

```bash
dotnet add package Fluxor.Persistence.Extensions
```
[![NuGet version (Fluxor.Persistence)](https://img.shields.io/nuget/v/Fluxor.Persistence.svg?style=flat-square)](https://www.nuget.org/packages/Fluxor.Persistence/)

### Configuration

Integrate the persistence mechanism into your project by configuring Fluxor in your `Program.cs`:

```csharp
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UsePersist(cfg =>
    {
        cfg.ForState<CounterState>().AddFullStatePersistence();

        cfg.ForState<WeatherState>()
            .AddPropertyPersistence(x => x.SelectedCity)
            .AddPropertyPersistence(x => x.TemperatureUnit);
    });
});
```

#### Handling Nested Properties

Fluxor.Persistence.Extensions also supports nested properties. Here's how you can configure it:

```csharp
cfg.ForState<UserSettings>()
    .AddPropertyPersistence(x => x.Theme.Colours.Pallette)
    .AddPropertyPersistence(x => x.Theme.IsDarkTheme);
```

### Custom Persistence Service

By default, the extension uses [Blazored.LocalStorage](https://github.com/Blazored/LocalStorage) for storing state. If you need a different storage solution, implement the `IPersistenceService` interface:

```csharp
internal sealed class InMemoryPersistenceService : IPersistenceService
{
    private readonly ConcurrentDictionary<string, string> _storage = new();

    public ValueTask<string?> GetItemAsStringAsync(string storageKey)
    {
        _storage.TryGetValue(storageKey, out var value);
        return ValueTask.FromResult<string?>(value);
    }

    public ValueTask SetItemAsStringAsync(string storageKey, string value)
    {
        _storage[storageKey] = value;
        return ValueTask.CompletedTask;
    }
}
```

Then, register your custom service:

```csharp
builder.Services.AddScoped<IPersistenceService, InMemoryPersistenceService>();
```

## Examples

For practical implementations, check out the Blazor WASM example:

- **Blazor WASM Example:** [FluxorPersistence.Blazor.Wasm](Examples/Fluxor.Persistence.Blazor.Wasm/Fluxor.Persistence.Blazor.Wasm.csproj)

## Demonstration
Check out the following GIF to see state persistence in action within a Blazor WASM application:

![Persistence Demo](docs/images/fluxor_persistence_demo.gif)

## Contributing

Contributions are welcome! Feel free to open issues or submit pull requests for improvements and new features.

## Releasing a New Library Version

To publish an updated version of the library to the NuGet registry, follow these steps:

1. **Prepare the Release:**

   Navigate to the `Fluxor.Persistence` directory and prepare the release using the following command:

   ```bash
   cd src/Fluxor.Persistence
   nbgv prepare-release
   ```

2. **Push the Release Branch:**

    Pushing the release branch will initiate the GitHub Actions workflow, which will build and publish the package to NuGet. Execute the push command with the appropriate version tag:

    ```bash
    git push origin release/vx.x
    ```

    Replace vx.x with the specific version number you are releasing.

## License

This project is licensed under the [MIT License](LICENSE).
