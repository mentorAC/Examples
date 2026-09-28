using System.Text.Json.Serialization;
using FluentValidation;
using Metrix.Behaviors;
using Metrix.Common;
using Metrix.Infrastructure;
using Metrix.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

// Strategies: every IDeliveryStrategy<TCommand> in this assembly; the command type uniquely determines the implementation.
builder.Services.AddDeliveryStrategies(typeof(Program).Assembly);

// Metrics: IAppMetrix<TCommand> names its metrics after the command type in snake_case.
builder.Services.AddSingleton(typeof(IAppMetrix<>), typeof(AppMetrix<>));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Metrix"))
    .WithMetrics(metrics => metrics
        .AddMeter(MetrixMeter.Name)
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Runtime")
        .AddPrometheusExporter());

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
