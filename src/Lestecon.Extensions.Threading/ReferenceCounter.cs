namespace Lestecon.Extensions.Threading;

internal sealed class ReferenceCounter<T>
{
    public ReferenceCounter(T value)
    {
        Count = 1;
        Value = value;
    }

    public int Count { get; set; }

    public T Value { get; private set; }
}
