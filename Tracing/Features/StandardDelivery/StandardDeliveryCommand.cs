using Tracing.Common;

namespace Tracing.Features.StandardDelivery;

public sealed record StandardDeliveryCommand(
    decimal WeightKg,
    int DistanceKm,
    bool IsFragile) : IDeliveryCommand;
