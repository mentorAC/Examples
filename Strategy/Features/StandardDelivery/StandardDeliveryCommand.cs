using Strategy.Common;

namespace Strategy.Features.StandardDelivery;

public sealed record StandardDeliveryCommand(
    decimal WeightKg,
    int DistanceKm,
    bool IsFragile) : IDeliveryCommand;
