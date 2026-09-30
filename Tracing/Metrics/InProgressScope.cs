using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Tracing.Metrics;

public readonly struct InProgressScope : IDisposable
{
    private readonly UpDownCounter<long> _counter;
    private readonly TagList _tags;

    internal InProgressScope(UpDownCounter<long> counter, in TagList tags)
    {
        _counter = counter;
        _tags = tags;
        _counter.Add(1, _tags);
    }

    public void Dispose() => _counter?.Add(-1, _tags);
}
