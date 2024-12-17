namespace Lestecon.Extensions.Threading;

public sealed class KeyedFifoSemaphore
{
    private readonly Dictionary<string, ReferenceCounter<FifoSemaphore>> semaphoreCounters = [];

    public event EventHandler<string>? LastProcessForKey;

    public bool IsEmpty => semaphoreCounters.Count == 0;

    public bool ContainsKey(string key) => semaphoreCounters.ContainsKey(key);

    public async Task<IDisposable> WaitAsync(string key)
    {
        await GetOrCreate(key).WaitAsync();

        var releaser = new KeyedFifoSemaphoreReleaser(key, semaphoreCounters);

        releaser.LastProcess += (s, a) => LastProcessForKey?.Invoke(this, key);

        return releaser;
    }

    private FifoSemaphore GetOrCreate(string key)
    {
        ReferenceCounter<FifoSemaphore>? semaphoreCounter;

        lock (semaphoreCounters)
        {
            if (semaphoreCounters.TryGetValue(key, out semaphoreCounter))
            {
                ++semaphoreCounter.Count;
            }
            else
            {
                semaphoreCounter = new ReferenceCounter<FifoSemaphore>(new FifoSemaphore(1, 1));
                semaphoreCounters[key] = semaphoreCounter;
            }
        }

        return semaphoreCounter.Value;
    }
}
