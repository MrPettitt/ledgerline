using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Details;

public record PricePoint(DateOnly From, decimal Amount, decimal? ChangePercent);

public record ChargeRow(DateOnly Date, string Description, decimal Amount);

public record SubscriptionDetail(
    int Id,
    string MerchantName,
    SubscriptionStatus Status,
    BillingCadence Cadence,
    decimal CurrentAmount,
    decimal MonthlyCost,
    DateOnly FirstChargeDate,
    DateOnly LastChargeDate,
    DateOnly NextExpectedDate,
    int ChargeCount,
    decimal TotalPaid,
    IReadOnlyList<PricePoint> PriceHistory,
    IReadOnlyList<ChargeRow> Charges);
