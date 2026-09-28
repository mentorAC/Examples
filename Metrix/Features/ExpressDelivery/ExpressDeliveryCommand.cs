using Metrix.Common;

namespace Metrix.Features.ExpressDelivery;

public sealed record ExpressDeliveryCommand(
    decimal WeightKg,
    int DistanceKm,
    bool IsFragile,
    decimal DeclaredValue) : IDeliveryCommand;
