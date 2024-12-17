using System.Collections.Concurrent;

namespace Lestecon.Extensions.Threading;

public sealed class FifoSemaphore : IDisposable
{
    private readonly ConcurrentQueue<TaskCompletionSource<bool>> queue = new();
    private readonly SemaphoreSlim semaphore;

    public FifoSemaphore(int initialCount)
    {
        semaphore = new SemaphoreSlim(initialCount);
    }

    public FifoSemaphore(int initialCount, int maxCount)
    {
        semaphore = new SemaphoreSlim(initialCount, maxCount);
    }

    public void Dispose() => semaphore.Dispose();

    public void Release() => semaphore.Release();

    public Task WaitAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        queue.Enqueue(tcs);

        _ = semaphore
            .WaitAsync()
            .ContinueWith(
                t =>
                {
                    if (queue.TryDequeue(out var popped))
                    {
                        popped.SetResult(true);
                    }
                },
                TaskScheduler.Default);

        return tcs.Task;
    }
}
