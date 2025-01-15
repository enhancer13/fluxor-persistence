// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Blazored.LocalStorage;
using Fluxor.Persistence.Middlewares;
using Fluxor.Persistence.Test.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Fluxor.Persistence.Test;

public class FluxorPersistenceTest
{
    private readonly Mock<ILocalStorageService> _LocalStorageServiceMock = new();

    [Fact]
    public async Task GivenPersistedFullState_WhenApplicationInitialized_ExpectStateIsRestored()
    {
        // Arrange
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddFluxor(options =>
            {
                options.ScanTypes(typeof(PersistenceMiddleware), typeof(SampleState));
                options.UsePersist(builder =>
                {
                    builder.ForState<SampleState>().AddFullStatePersistence();
                });
            })
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddScoped<ILocalStorageService>(_ => _LocalStorageServiceMock.Object)
            .BuildServiceProvider(validateScopes: true);
        _LocalStorageServiceMock.Setup(x => x.GetItemAsStringAsync("SampleState", It.IsAny<CancellationToken>()))
            .ReturnsAsync("{\"Name\":\"Test\",\"Age\":42,\"Address\":\"Test Address\"}");

        // Act
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        IStore store = serviceScope.ServiceProvider.GetRequiredService<IStore>();
        await store.InitializeAsync();

        // Assert
        IState<SampleState> state = serviceScope.ServiceProvider.GetRequiredService<IState<SampleState>>();
        state.Value.Name.ShouldBe("Test");
        state.Value.Age.ShouldBe(42);
        state.Value.Address.ShouldBe("Test Address");
    }

    [Fact]
    public async Task GivenPersistedProperties_WhenApplicationInitialized_ExpectPersistedPropertiesAreRestored()
    {
        // Arrange
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddFluxor(options =>
            {
                options.ScanTypes(typeof(PersistenceMiddleware), typeof(SampleState));
                options.UsePersist(builder =>
                {
                    builder.ForState<SampleState>()
                        .AddPropertyPersistence(x => x.Name)
                        .AddPropertyPersistence(x => x.Age);
                });
            })
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddScoped<ILocalStorageService>(_ => _LocalStorageServiceMock.Object)
            .BuildServiceProvider(validateScopes: true);
        _LocalStorageServiceMock.Setup(x => x.GetItemAsStringAsync("SampleState.Name", It.IsAny<CancellationToken>()))
            .ReturnsAsync("\"Test\"");
        _LocalStorageServiceMock.Setup(x => x.GetItemAsStringAsync("SampleState.Age", It.IsAny<CancellationToken>()))
            .ReturnsAsync("42");

        // Act
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        IStore store = serviceScope.ServiceProvider.GetRequiredService<IStore>();
        await store.InitializeAsync();

        // Assert
        IState<SampleState> state = serviceScope.ServiceProvider.GetRequiredService<IState<SampleState>>();
        state.Value.Name.ShouldBe("Test");
        state.Value.Age.ShouldBe(42);
        state.Value.Address.ShouldBe("Initial Address");
    }

    [Fact]
    public async Task GivenFullStatePersistence_WhenStateChanged_ExpectFullStateIsSaved()
    {
        // Arrange
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddFluxor(options =>
            {
                options.ScanTypes(typeof(PersistenceMiddleware), typeof(SampleState), typeof(SampleReducers));
                options.UsePersist(builder =>
                {
                    builder.ForState<SampleState>().AddFullStatePersistence();
                });
            })
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddScoped<ILocalStorageService>(_ => _LocalStorageServiceMock.Object)
            .BuildServiceProvider(validateScopes: true);
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        IStore store = serviceScope.ServiceProvider.GetRequiredService<IStore>();
        IDispatcher dispatcher = serviceScope.ServiceProvider.GetRequiredService<IDispatcher>();
        await store.InitializeAsync();

        // Act
        dispatcher.Dispatch(new UpdateNameAction("NewName"));
        dispatcher.Dispatch(new UpdateAgeAction(30));

        // Assert
        await _LocalStorageServiceMock.VerifyWithTimeoutAsync(
            x => x.SetItemAsStringAsync("SampleState", "{\"Name\":\"NewName\",\"Age\":30,\"Address\":\"Initial Address\"}", It.IsAny<CancellationToken>()),
            times: Times.AtLeastOnce,
            timeout: TimeSpan.FromSeconds(2),
            failMessage: "Full state was not persisted on state change.");
    }

    [Fact]
    public async Task GivenPropertiesPersistence_WhenStateChanged_ExpectPropertiesAreSaved()
    {
        // Arrange
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddFluxor(options =>
            {
                options.ScanTypes(typeof(PersistenceMiddleware), typeof(SampleState), typeof(SampleReducers));
                options.UsePersist(builder =>
                {
                    builder.ForState<SampleState>()
                        .AddPropertyPersistence(x => x.Name)
                        .AddPropertyPersistence(x => x.Age);
                });
            })
            .AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>))
            .AddScoped<ILocalStorageService>(_ => _LocalStorageServiceMock.Object)
            .BuildServiceProvider(validateScopes: true);
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        IStore store = serviceScope.ServiceProvider.GetRequiredService<IStore>();
        IDispatcher dispatcher = serviceScope.ServiceProvider.GetRequiredService<IDispatcher>();
        await store.InitializeAsync();

        // Act
        dispatcher.Dispatch(new UpdateNameAction("PropertyName"));
        dispatcher.Dispatch(new UpdateAgeAction(25));

        // Assert
        await _LocalStorageServiceMock.VerifyWithTimeoutAsync(
            x => x.SetItemAsStringAsync("SampleState.Name", "\"PropertyName\"", It.IsAny<CancellationToken>()),
            times: Times.AtLeastOnce,
            timeout: TimeSpan.FromSeconds(2),
            failMessage: "Property 'Name' was not persisted on state change.");

        await _LocalStorageServiceMock.VerifyWithTimeoutAsync(
            x => x.SetItemAsStringAsync("SampleState.Age", "25", It.IsAny<CancellationToken>()),
            times: Times.AtLeastOnce,
            timeout: TimeSpan.FromSeconds(2),
            failMessage: "Property 'Age' was not persisted on state change.");
    }
}

[FeatureState]
public sealed record SampleState
{
    public string? Name { get; init; }

    public int Age { get; init; }

    public string? Address { get; init; }

    public SampleState()
    {
        Name = "InitialName";
        Age = 18;
        Address = "Initial Address";
    }
}

public record UpdateNameAction(string Name);
public record UpdateAgeAction(int Age);

public static class SampleReducers
{
    [ReducerMethod]
    public static SampleState OnUpdateName(SampleState state, UpdateNameAction action) =>
        state with { Name = action.Name };

    [ReducerMethod]
    public static SampleState OnUpdateAge(SampleState state, UpdateAgeAction action) =>
        state with { Age = action.Age };
}
