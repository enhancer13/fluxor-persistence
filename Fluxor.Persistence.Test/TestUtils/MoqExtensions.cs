// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Moq;

namespace Fluxor.Persistence.Test.TestUtils;

internal static class MoqExtensions
{
    public static async Task VerifyWithTimeoutAsync<T, TResult>(this Mock<T> mock,
                                                                Expression<Func<T, TResult>> expression,
                                                                Func<Times> times,
                                                                TimeSpan timeout,
                                                                TimeSpan pollInterval = default,
                                                                string failMessage = "Verification failed.")
        where T : class
    {
        pollInterval = pollInterval == TimeSpan.Zero ? TimeSpan.FromMilliseconds(100) : pollInterval;
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        var token = cancellationTokenSource.Token;
        MockException? lastException = null;
        try
        {
            while (true)
            {
                try
                {
                    mock.Verify(expression, times(), failMessage);
                    return;
                }
                catch (MockException ex)
                {
                    lastException = ex;
                    await Task.Delay(pollInterval, token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            if (lastException is not null)
            {
                throw lastException;
            }

            throw new TimeoutException($"Verification for {expression} failed after {timeout.TotalSeconds} seconds.");
        }
    }
}
