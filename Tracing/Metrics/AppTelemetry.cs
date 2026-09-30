using System.Diagnostics;

namespace Tracing.Metrics;

/// <summary>
/// Name shared by the application's Meter and ActivitySource, so metrics and traces are registered with one value.
/// </summary>
public static class AppTelemetry
{
    public const string Name = "Tracing";

    public const string ServiceName = "Tracing";

    // ActivitySource has no factory like IMeterFactory: one static instance for the whole application.
    public static readonly ActivitySource ActivitySource = new(Name);
}
