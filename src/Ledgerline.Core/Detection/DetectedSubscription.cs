using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Detection;

public record DetectedSubscription(
    int MerchantId,
    BillingCadence Cadence,
    decimal CurrentAmount,
    DateOnly FirstChargeDate,
    DateOnly LastChargeDate,
    DateOnly NextExpectedDate,
    IReadOnlyList<DetectedEvent> Events);
