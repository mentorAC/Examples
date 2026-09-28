using Metrix.Common;

namespace Metrix.Features.StandardDelivery;

public sealed record StandardDeliveryCommand(
    decimal WeightKg,
    int DistanceKm,
    bool IsFragile) : IDeliveryCommand;
