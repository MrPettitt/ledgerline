namespace Ledgerline.Core.Summary;

public record SubscriptionCost(string MerchantName, decimal MonthlyCost);

public record UpcomingRenewal(string MerchantName, DateOnly Date, decimal Amount);

public record OverviewSummary(
    decimal MonthlyTotal,
    decimal YearlyTotal,
    int ConfirmedCount,
    int SuspectedCount,
    IReadOnlyList<SubscriptionCost> MostExpensive,
    IReadOnlyList<UpcomingRenewal> UpcomingRenewals)
{
    public int ActiveCount => ConfirmedCount + SuspectedCount;
}
