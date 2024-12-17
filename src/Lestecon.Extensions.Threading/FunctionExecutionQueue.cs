namespace Lestecon.Extensions.Threading;

public class FunctionExecutionQueue
{
    private readonly Lock locker = new();

    private WeakReference<Task>? lastTaskWeak;

    private int referenceCount;

    public event Action? OnLastAction;

    public event Action? OnActionExecuted;

    public void Enqueue(Func<Task> asyncAction)
    {
        Task resultTask;

        locker.Enter();

        ++referenceCount;

        resultTask = lastTaskWeak != null && lastTaskWeak.TryGetTarget(out var lastTask)
            ? lastTask
                .ContinueWith(
                    _ => asyncAction(),
                    default,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default)
                .Unwrap()
                .ContinueWith(
                    _ => SubtractCounter(),
                    default,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default)
            : Task
                .Run(asyncAction)
                .ContinueWith(
                    _ => SubtractCounter(),
                    default,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);

        lastTaskWeak = new WeakReference<Task>(resultTask);

        locker.Exit();
    }

    private void SubtractCounter()
    {
        locker.Enter();

        --referenceCount;

        OnActionExecuted?.Invoke();

        if (referenceCount == 0)
        {
            OnLastAction?.Invoke();
        }

        locker.Exit();
    }
}
