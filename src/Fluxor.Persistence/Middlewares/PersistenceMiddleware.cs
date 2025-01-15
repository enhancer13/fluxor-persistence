// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Channels;
using Fluxor.Persistence.Services;
using Fluxor.Persistence.Strategies;
using Microsoft.Extensions.Logging;

namespace Fluxor.Persistence.Middlewares;

internal sealed class PersistenceMiddleware : Middleware, IAsyncDisposable
{
    private readonly IEnumerable<IPersistenceStrategy> _Strategies;
    private readonly IPersistenceService _PersistenceService;
    private readonly ILogger<PersistenceMiddleware> _Logger;


    private readonly Channel<IFeature> _PersistenceChannel = Channel.CreateUnbounded<IFeature>();
    private readonly CancellationTokenSource _Cts = new();
    private readonly Task _BackgroundTask;

    private readonly List<IFeature> _SubscribedFeatures = [];
    private bool _Disposed;

    public PersistenceMiddleware(IEnumerable<IPersistenceStrategy> strategies, IPersistenceService persistenceService, ILogger<PersistenceMiddleware> logger)
    {
        _Strategies = strategies;
        _PersistenceService = persistenceService;
        _Logger = logger;
        _BackgroundTask = Task.Run(ProcessPersistenceQueueAsync);
    }

    public override async Task InitializeAsync(IDispatcher dispatcher, IStore store)
    {
        FluxorLogger.InitializingPersistMiddleware(_Logger);

        foreach (IFeature? feature in store.Features.Values)
        {
            List<IPersistenceStrategy> strategies = _Strategies.Where(x => x.StateType == feature.GetStateType()).ToList();
            if (strategies.Count == 0)
            {
                continue;
            }

            foreach (IPersistenceStrategy strategy in strategies)
            {
                await strategy.LoadAsync(feature, _PersistenceService, _Logger).ConfigureAwait(false);
                feature.StateChanged += HandleStateChanged;
                _SubscribedFeatures.Add(feature);
            }
        }

        FluxorLogger.PersistMiddlewareInitialized(_Logger);
        await base.InitializeAsync(dispatcher, store).ConfigureAwait(false);
    }

    private async Task ProcessPersistenceQueueAsync()
    {
        try
        {
            await foreach (IFeature feature in _PersistenceChannel.Reader.ReadAllAsync(_Cts.Token).ConfigureAwait(false))
            {
                foreach (IPersistenceStrategy strategy in _Strategies.Where(x => x.StateType == feature.GetStateType()))
                {

                    await strategy.SaveAsync(feature, _PersistenceService, _Logger).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            FluxorLogger.ErrorPersistingState(_Logger, ex);
            throw;
        }
    }

    private void HandleStateChanged(object? sender, EventArgs e)
    {
        if (sender is IFeature feature && !_PersistenceChannel.Writer.TryWrite(feature))
        {
            FluxorLogger.FailedToEnqueuePersistenceTask(_Logger, feature.GetStateType().Name);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_Disposed)
        {
            return;
        }

        foreach (IFeature feature in _SubscribedFeatures)
        {
            feature.StateChanged -= HandleStateChanged;
        }
        _SubscribedFeatures.Clear();

        await _Cts.CancelAsync().ConfigureAwait(false);
        try
        {
            await _BackgroundTask.ConfigureAwait(false);
        }
        finally
        {
            _Cts.Dispose();
            _PersistenceChannel.Writer.Complete();
            _Disposed = true;
        }
    }
}
