namespace BrasilCompete.Worker.Http;

/// <summary>
/// Garante uma requisição por vez por fonte e um intervalo mínimo entre elas.
/// </summary>
public sealed class PolitenessGate(TimeSpan minInterval, TimeProvider timeProvider)
{
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private DateTimeOffset lastRequestFinishedAt = DateTimeOffset.MinValue;

    public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var wait = lastRequestFinishedAt + minInterval - timeProvider.GetUtcNow();

            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, timeProvider, cancellationToken);
            }

            return await action();
        }
        finally
        {
            lastRequestFinishedAt = timeProvider.GetUtcNow();
            semaphore.Release();
        }
    }
}
