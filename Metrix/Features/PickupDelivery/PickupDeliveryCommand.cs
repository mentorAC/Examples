using Metrix.Common;

namespace Metrix.Features.PickupDelivery;

public sealed record PickupDeliveryCommand(
    string PickupPointId,
    decimal WeightKg) : IDeliveryCommand;
