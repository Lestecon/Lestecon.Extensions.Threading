using System.Collections.Concurrent;

namespace Lestecon.Extensions.Threading;

public class KeyedFunctionExecutionQueue<TKey>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, FunctionExecutionQueue> functionQueue = new();

    public event Action<TKey>? OnLastAction;

    public event Action<TKey>? OnActionExecuted;

    public bool IsEmpty => functionQueue.IsEmpty;

    public bool Contains(TKey key) =>
        functionQueue.ContainsKey(key);

    public void Enqueue(TKey key, Func<Task> function)
    {
        var serialQueue = functionQueue.GetOrAdd(key, _ => new FunctionExecutionQueue());

        serialQueue.OnActionExecuted += () => OnActionExecuted?.Invoke(key);
        serialQueue.OnLastAction += () =>
        {
            functionQueue.TryRemove(key, out _);
            OnLastAction?.Invoke(key);
        };

        serialQueue.Enqueue(function);
    }
}
