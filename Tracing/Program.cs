using System.Text.Json.Serialization;
using FluentValidation;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Tracing.Behaviors;
using Tracing.Common;
using Tracing.Infrastructure;
using Tracing.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

// Strategies: every IDeliveryStrategy<TCommand> in this assembly; the command type uniquely determines the implementation.
builder.Services.AddDeliveryStrategies(typeof(Program).Assembly);

// Metrics and traces: IAppMetrix<TCommand> names its metrics and spans after the command type.
builder.Services.AddSingleton(typeof(IAppMetrix<>), typeof(AppMetrix<>));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(AppTelemetry.ServiceName)
        .AddAttributes([new("deployment.environment.name", builder.Environment.EnvironmentName)]))
    .WithMetrics(metrics => metrics
        .AddMeter(AppTelemetry.Name)
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Runtime")
        // Measurements taken inside a sampled span carry its trace_id as an exemplar: metrics → traces in Grafana.
        .SetExemplarFilter(ExemplarFilterType.TraceBased)
        .AddPrometheusExporter())
    .WithTracing(tracing => tracing
        .AddSource(AppTelemetry.Name)
        .AddAspNetCoreInstrumentation(options =>
            // Prometheus scrapes every 5 s; those requests would flood Tempo.
            options.Filter = context => !context.Request.Path.StartsWithSegments("/metrics"))
        // OTLP exporters read OTEL_EXPORTER_OTLP_ENDPOINT/PROTOCOL from configuration and append /v1/traces, /v1/logs;
        // otel-collector behind that one endpoint routes traces to Tempo and logs to Loki.
        .AddOtlpExporter())
    .WithLogging(
        logging => logging.AddOtlpExporter(),
        options =>
        {
            // Loki shows the rendered message as the log line; the template arguments stay as attributes.
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(MetricsBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapPrometheusScrapingEndpoint();

app.MapControllers();

app.Run();
