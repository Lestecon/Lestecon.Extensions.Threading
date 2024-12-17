namespace Lestecon.Extensions.Threading;

internal sealed class KeyedFifoSemaphoreReleaser(
    string key,
    Dictionary<string, ReferenceCounter<FifoSemaphore>> semaphoreCounters)
    : IDisposable
{
    public event EventHandler? LastProcess;

    public void Dispose()
    {
        ReferenceCounter<FifoSemaphore> semaphoreCounter;
        bool disposeSemaphore = false;

        lock (semaphoreCounters)
        {
            semaphoreCounter = semaphoreCounters[key];

            --semaphoreCounter.Count;

            if (semaphoreCounter.Count == 0)
            {
                semaphoreCounters.Remove(key);
                LastProcess?.Invoke(this, EventArgs.Empty);
                disposeSemaphore = true;
            }
        }

        semaphoreCounter.Value.Release();

        if (disposeSemaphore)
        {
            semaphoreCounter.Value.Dispose();
        }
    }
}
