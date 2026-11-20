using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Alerts;

public class AlertBuilder
{
    public const int RenewalWindowDays = 7;
    public const int PriceIncreaseLookbackDays = 90;

    /// <summary>Expects subscriptions loaded with their merchant and events.</summary>
    public IReadOnlyList<Alert> Build(IEnumerable<Subscription> subscriptions, DateOnly today)
    {
        var alerts = subscriptions
            .Where(s => s.Status is SubscriptionStatus.Suspected or SubscriptionStatus.Confirmed)
            .SelectMany(s => AlertsFor(s, today))
            .ToList();

        return alerts.OrderBy(a => a.Kind).ThenBy(a => a.Date).ToList();
    }

    private static IEnumerable<Alert> AlertsFor(Subscription subscription, DateOnly today)
    {
        var name = subscription.Merchant?.DisplayName ?? string.Empty;

        // A missed event is stale once a later charge has arrived.
        var missed = subscription.Events
            .Where(e => e.Type == SubscriptionEventType.Missed && e.OccurredOn > subscription.LastChargeDate)
            .MaxBy(e => e.OccurredOn);
        if (missed is not null)
        {
            yield return new Alert(AlertKind.MissedRenewal, subscription.Id, name, missed.OccurredOn, subscription.CurrentAmount, null);
        }

        var increase = subscription.Events
            .Where(e => e.Type == SubscriptionEventType.PriceChange
                        && e.Amount > e.PreviousAmount
                        && e.OccurredOn >= today.AddDays(-PriceIncreaseLookbackDays))
            .MaxBy(e => e.OccurredOn);
        if (increase is not null)
        {
            yield return new Alert(AlertKind.PriceIncrease, subscription.Id, name, increase.OccurredOn, increase.Amount!.Value, increase.PreviousAmount);
        }

        var next = subscription.NextExpectedDate;
        if (next >= today && next <= today.AddDays(RenewalWindowDays))
        {
            yield return new Alert(AlertKind.UpcomingRenewal, subscription.Id, name, next, subscription.CurrentAmount, null);
        }
    }
}
