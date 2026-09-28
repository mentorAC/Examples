using System.Text.Json.Serialization;
using FluentValidation;
using Strategy.Behaviors;
using Strategy.Common;
using Strategy.Features.ExpressDelivery;
using Strategy.Features.PickupDelivery;
using Strategy.Features.StandardDelivery;
using Scalar.AspNetCore;
using Strategy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);

// Strategies: the command type uniquely determines the implementation.
builder.Services.AddSingleton<IDeliveryStrategy<StandardDeliveryCommand>, StandardDeliveryStrategy>();
builder.Services.AddSingleton<IDeliveryStrategy<ExpressDeliveryCommand>, ExpressDeliveryStrategy>();
builder.Services.AddSingleton<IDeliveryStrategy<PickupDeliveryCommand>, PickupDeliveryStrategy>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
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

app.MapControllers();

app.Run();
