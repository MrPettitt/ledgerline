using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Summary;

public class OverviewSummaryBuilder
{
    public const int ListLength = 5;
    public const int UpcomingWindowDays = 30;

    /// <summary>Expects subscriptions loaded with their merchant. Ignored and cancelled ones are left out.</summary>
    public OverviewSummary Build(IEnumerable<Subscription> subscriptions, DateOnly today)
    {
        var active = subscriptions
            .Where(s => s.Status is SubscriptionStatus.Suspected or SubscriptionStatus.Confirmed)
            .ToList();

        var costs = active
            .Select(s => new SubscriptionCost(NameOf(s), MonthlyCost.Of(s.CurrentAmount, s.Cadence)))
            .ToList();
        var monthlyTotal = costs.Sum(c => c.MonthlyCost);

        var upcoming = active
            .Where(s => s.NextExpectedDate >= today && s.NextExpectedDate <= today.AddDays(UpcomingWindowDays))
            .OrderBy(s => s.NextExpectedDate)
            .Take(ListLength)
            .Select(s => new UpcomingRenewal(NameOf(s), s.NextExpectedDate, s.CurrentAmount))
            .ToList();

        return new OverviewSummary(
            monthlyTotal,
            monthlyTotal * 12,
            active.Count(s => s.Status == SubscriptionStatus.Confirmed),
            active.Count(s => s.Status == SubscriptionStatus.Suspected),
            costs.OrderByDescending(c => c.MonthlyCost).Take(ListLength).ToList(),
            upcoming);
    }

    private static string NameOf(Subscription subscription) => subscription.Merchant?.DisplayName ?? string.Empty;
}
