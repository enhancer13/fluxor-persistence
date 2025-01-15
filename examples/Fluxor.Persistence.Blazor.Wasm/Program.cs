using Fluxor;
using Fluxor.Persistence;
using Fluxor.Persistence.Blazor.Wasm;
using Fluxor.Persistence.Blazor.Wasm.Pages.Counter;
using Fluxor.Persistence.Blazor.Wasm.Pages.Weather;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
