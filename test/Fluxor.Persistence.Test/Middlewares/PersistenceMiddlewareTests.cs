// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Persistence.Middlewares;
using Fluxor.Persistence.Services;
using Fluxor.Persistence.Strategies;
using Fluxor.Persistence.Test.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fluxor.Persistence.Test.Middlewares;

public class PersistenceMiddlewareTests
{
    private readonly Mock<IPersistenceService> _PersistenceServiceMock = new();
    private readonly Mock<ILogger<PersistenceMiddleware>> _LoggerMock = new();

    [Fact]
    public async Task GivenStoreWithMultipleFeaturesAndStrategies_WhenInitializingMiddleware_ExpectAllRelevantStrategiesLoaded()
    {
        // Arrange
        var featureMock1 = new Mock<IFeature>();
        featureMock1.Setup(x => x.GetName()).Returns("Feature1");
        featureMock1.Setup(x => x.GetStateType()).Returns(typeof(SampleState1));

        var featureMock2 = new Mock<IFeature>();
        featureMock2.Setup(x => x.GetName()).Returns("Feature2");
        featureMock2.Setup(x => x.GetStateType()).Returns(typeof(SampleState2));

        var storeMock = new Mock<IStore>();
        storeMock.Setup(x => x.Features).Returns(new Dictionary<string, IFeature>
        {
            { featureMock1.Object.GetName(), featureMock1.Object },
            { featureMock2.Object.GetName(), featureMock2.Object }
        });

        var dispatcherMock = Mock.Of<IDispatcher>();

        var strategyMock1 = new Mock<IPersistenceStrategy>();
        strategyMock1.Setup(x => x.StateType).Returns(typeof(SampleState1));
        strategyMock1.Setup(x => x.LoadAsync(featureMock1.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);

        var strategyMock2 = new Mock<IPersistenceStrategy>();
        strategyMock2.Setup(x => x.StateType).Returns(typeof(SampleState2));
        strategyMock2.Setup(x => x.LoadAsync(featureMock2.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);

        var strategies = new List<IPersistenceStrategy> { strategyMock1.Object, strategyMock2.Object };

        await using var middleware = new PersistenceMiddleware(strategies, _PersistenceServiceMock.Object, _LoggerMock.Object);

        // Act
        await middleware.InitializeAsync(dispatcherMock, storeMock.Object);

        // Assert
        strategyMock1.Verify(x => x.LoadAsync(featureMock1.Object, _PersistenceServiceMock.Object, _LoggerMock.Object), Times.Once);
        strategyMock2.Verify(x => x.LoadAsync(featureMock2.Object, _PersistenceServiceMock.Object, _LoggerMock.Object), Times.Once);
    }

    [Fact]
    public async Task GivenMultipleFeaturesWithPersistence_WhenStateChanged_ExpectSaveAsyncCalledForEachFeature()
    {
        // Arrange
        var featureMock1 = new Mock<IFeature>();
        featureMock1.Setup(x => x.GetName()).Returns("Feature1");
        featureMock1.Setup(x => x.GetStateType()).Returns(typeof(SampleState1));

        var featureMock2 = new Mock<IFeature>();
        featureMock2.Setup(x => x.GetName()).Returns("Feature2");
        featureMock2.Setup(x => x.GetStateType()).Returns(typeof(SampleState2));

        var storeMock = new Mock<IStore>();
        storeMock.Setup(x => x.Features).Returns(new Dictionary<string, IFeature>
        {
            { featureMock1.Object.GetName(), featureMock1.Object },
            { featureMock2.Object.GetName(), featureMock2.Object }
        });

        var dispatcherMock = Mock.Of<IDispatcher>();

        var strategyMock1 = new Mock<IPersistenceStrategy>();
        strategyMock1.Setup(x => x.StateType).Returns(typeof(SampleState1));
        strategyMock1.Setup(x => x.LoadAsync(featureMock1.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);
        strategyMock1.Setup(x => x.SaveAsync(featureMock1.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);

        var strategyMock2 = new Mock<IPersistenceStrategy>();
        strategyMock2.Setup(x => x.StateType).Returns(typeof(SampleState2));
        strategyMock2.Setup(x => x.LoadAsync(featureMock2.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);
        strategyMock2.Setup(x => x.SaveAsync(featureMock2.Object, _PersistenceServiceMock.Object, _LoggerMock.Object)).Returns(ValueTask.CompletedTask);

        var strategies = new List<IPersistenceStrategy> { strategyMock1.Object, strategyMock2.Object };

        await using var middleware = new PersistenceMiddleware(strategies, _PersistenceServiceMock.Object, _LoggerMock.Object);
        await middleware.InitializeAsync(dispatcherMock, storeMock.Object);

        // Act
        featureMock1.Raise(x => x.StateChanged += null, featureMock1.Object, EventArgs.Empty);
        featureMock2.Raise(x => x.StateChanged += null, featureMock2.Object, EventArgs.Empty);

        // Assert
        await strategyMock1.VerifyWithTimeoutAsync(
            x => x.SaveAsync(featureMock1.Object, _PersistenceServiceMock.Object, _LoggerMock.Object),
            Times.Once,
            timeout: TimeSpan.FromSeconds(2),
            failMessage: "SaveAsync was not called once for Feature1 state change."
        );

        await strategyMock2.VerifyWithTimeoutAsync(
            x => x.SaveAsync(featureMock2.Object, _PersistenceServiceMock.Object, _LoggerMock.Object),
            Times.Once,
            timeout: TimeSpan.FromSeconds(2),
            failMessage: "SaveAsync was not called once for Feature2 state change."
        );
    }

    private class SampleState1
    {
        public int Value { get; init; }
    }

    private class SampleState2
    {
        public int Value { get; init; }
    }
}
