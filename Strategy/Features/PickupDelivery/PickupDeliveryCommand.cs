using Strategy.Common;

namespace Strategy.Features.PickupDelivery;

public sealed record PickupDeliveryCommand(
    string PickupPointId,
    decimal WeightKg) : IDeliveryCommand;
