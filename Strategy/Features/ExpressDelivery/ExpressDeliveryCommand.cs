using Strategy.Common;

namespace Strategy.Features.ExpressDelivery;

public sealed record ExpressDeliveryCommand(
    decimal WeightKg,
    int DistanceKm,
    bool IsFragile,
    decimal DeclaredValue) : IDeliveryCommand;
