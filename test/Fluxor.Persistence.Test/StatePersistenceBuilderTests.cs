// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Fluxor.Persistence.Strategies;
using Shouldly;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Fluxor.Persistence.Test;

public class StatePersistenceBuilderTests
{
    [Fact]
    public void GivenState_WhenAddingPropertyPersistence_ExpectPropertyPersistenceStrategyIsAdded()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();

        // Act
        statePersistenceBuilder.AddPropertyPersistence(x => x.Name);
        PersistConfiguration config = persistConfigurationBuilder.Build();

        // Assert
        config.Strategies.Count.ShouldBe(1);
        config.Strategies[0].ShouldBeOfType<PropertyPersistenceStrategy<SampleState, string>>();
    }

    [Fact]
    public void GivenState_WhenAddingFullStatePersistence_ExpectFullStatePersistenceIsAdded()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();

        // Act
        statePersistenceBuilder.AddFullStatePersistence();
        PersistConfiguration config = persistConfigurationBuilder.Build();

        // Assert
        config.Strategies.Count.ShouldBe(1);
        config.Strategies[0].ShouldBeOfType<FullStatePersistenceStrategy<SampleState>>();
    }

    [Fact]
    public void GivenStateWithFullStatePersistence_WhenAddingPropertyPersistenceForTheSameState_ExpectInvalidOperationException()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();
        statePersistenceBuilder.AddFullStatePersistence();

        // Act & Assert
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => statePersistenceBuilder.AddPropertyPersistence(x => x.Name));
        exception.Message.ShouldBe("Cannot add property persistence for 'SampleState' because full state persistence has already been configured.");
    }

    [Fact]
    public void GivenStateWithPropertyPersistence_WhenAddingFullStatePersistenceForTheSameState_ExpectInvalidOperationException()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();
        statePersistenceBuilder.AddPropertyPersistence(x => x.Name);

        // Act & Assert
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => statePersistenceBuilder.AddFullStatePersistence());
        exception.Message.ShouldBe("Cannot add full state persistence for 'SampleState' because specific property persistences have already been configured.");
    }

    [Fact]
    public void GivenStateWithParentPropertyPersistence_WhenAddingChildPropertyPersistenceForTheSameState_ExpectInvalidOperationException()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();
        statePersistenceBuilder.AddPropertyPersistence(x => x.Nested);

        // Act & Assert
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => statePersistenceBuilder.AddPropertyPersistence(x => x.Nested.InnerNested.Name));
        exception.Message.ShouldBe("Cannot add persistence for 'Nested.InnerNested.Name' because it causes overlapping with an existing property path 'Nested'.");
    }

    [Fact]
    public void GivenStateWithChildPropertyPersistence_WhenAddingParentPropertyPersistenceForTheSameState_ExpectInvalidOperationException()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();
        statePersistenceBuilder.AddPropertyPersistence(x => x.Nested.InnerNested.Name);

        // Act & Assert
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => statePersistenceBuilder.AddPropertyPersistence(x => x.Nested));
        exception.Message.ShouldBe("Cannot add persistence for 'Nested' because it causes overlapping with an existing property path 'Nested.InnerNested.Name'.");
    }

    [Fact]
    public void GivenState_WhenAddingMultipleNonOverlappingPropertyPersistences_ExpectAllStrategiesAreAdded()
    {
        // Arrange
        var serializerOptions = new JsonSerializerOptions();
        var persistConfigurationBuilder = new PersistConfigurationBuilder(serializerOptions);
        StatePersistenceBuilder<SampleState> statePersistenceBuilder = persistConfigurationBuilder.ForState<SampleState>();

        // Act
        statePersistenceBuilder.AddPropertyPersistence(x => x.Name);
        statePersistenceBuilder.AddPropertyPersistence(x => x.Age);
        PersistConfiguration config = persistConfigurationBuilder.Build();

        // Assert
        config.Strategies.Count.ShouldBe(2);
        config.Strategies[0].ShouldBeOfType<PropertyPersistenceStrategy<SampleState, string>>();
        config.Strategies[1].ShouldBeOfType<PropertyPersistenceStrategy<SampleState, int>>();
    }

    private class SampleState
    {
        public string? Name { get; init; }
        public string? Address { get; init; }
        public int Age { get; init; }
        public NestedState Nested { get; init; }
    }

    private class NestedState
    {
        public InnerNestedState InnerNested { get; init; }
    }

    private class InnerNestedState
    {
        public string? Name { get; init; }
    }

    private class AnotherState
    {
        public string? SomeProperty { get; init; }
    }
}
