using Ledgerline.Core.Domain;
using Ledgerline.Core.Summary;

namespace Ledgerline.Core.Details;

public class SubscriptionDetailBuilder
{
    /// <summary>Expects the subscription loaded with its merchant and events.</summary>
    public SubscriptionDetail Build(Subscription subscription, IEnumerable<ChargeRow> charges)
    {
        var newestFirst = charges.OrderByDescending(c => c.Date).ToList();

        return new SubscriptionDetail(
            subscription.Id,
            subscription.Merchant?.DisplayName ?? string.Empty,
            subscription.Status,
            subscription.Cadence,
            subscription.CurrentAmount,
            MonthlyCost.Of(subscription.CurrentAmount, subscription.Cadence),
            subscription.FirstChargeDate,
            subscription.LastChargeDate,
            subscription.NextExpectedDate,
            newestFirst.Count,
            newestFirst.Sum(c => c.Amount),
            PriceHistory(subscription),
            newestFirst);
    }

    private static List<PricePoint> PriceHistory(Subscription subscription)
    {
        var changes = subscription.Events
            .Where(e => e.Type == SubscriptionEventType.PriceChange && e.Amount is not null && e.PreviousAmount is not null)
            .OrderBy(e => e.OccurredOn)
            .ToList();

        var startingPrice = changes.Count > 0 ? changes[0].PreviousAmount!.Value : subscription.CurrentAmount;
        var history = new List<PricePoint> { new(subscription.FirstChargeDate, startingPrice, null) };

        foreach (var change in changes)
        {
            var percent = Math.Round((change.Amount!.Value - change.PreviousAmount!.Value) / change.PreviousAmount.Value * 100, 1);
            history.Add(new PricePoint(change.OccurredOn, change.Amount.Value, percent));
        }

        return history;
    }
}
