using System.Diagnostics;

namespace Tracing.Metrics;

/// <summary>
/// Metrics and traces for processing of <typeparamref name="TCommand"/>.
/// Metric names are derived from the command type name in snake_case,
/// e.g. StandardDeliveryCommand → standard_delivery_command_*;
/// span names use the type name, e.g. StandardDeliveryCommand, StandardDeliveryCommand.validate.
/// </summary>
public interface IAppMetrix<TCommand> where TCommand : notnull
{
    /// <summary>Base metric name, e.g. "standard_delivery_command".</summary>
    string Name { get; }

    /// <summary>Counter {name}_total: adds a non-negative value.</summary>
    void Increment(long value = 1, in TagList tags = default);

    /// <summary>Up-down counter {name}_in_progress: adds a positive or negative delta.</summary>
    void Add(long delta, in TagList tags = default);

    /// <summary>Histogram {name}_value: records an arbitrary value.</summary>
    void Record(double value, in TagList tags = default);

    /// <summary>Histogram {name}_duration_seconds: records the elapsed time when disposed.</summary>
    MetricTimer StartTimer(in TagList tags = default);

    /// <summary>Up-down counter {name}_in_progress: adds 1 now and subtracts 1 when disposed.</summary>
    InProgressScope TrackInProgress(in TagList tags = default);

    /// <summary>
    /// Starts span "{CommandName}" or, with <paramref name="operation"/>, "{CommandName}.{operation}" as a child
    /// of <see cref="Activity.Current"/>; the span gets a "command" tag with <see cref="Name"/>.
    /// Returns null when no listener (tracer provider) samples it, so use the null-conditional operator.
    /// </summary>
    Activity? StartActivity(string? operation = null, ActivityKind kind = ActivityKind.Internal, in TagList tags = default);

    /// <summary>Adds an event to <see cref="Activity.Current"/>, if there is one.</summary>
    void AddEvent(string name, in TagList tags = default);

    /// <summary>Marks <see cref="Activity.Current"/> as failed and records <paramref name="exception"/> on it.</summary>
    void RecordException(Exception exception);
}
