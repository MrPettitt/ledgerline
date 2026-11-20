using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Detection;

public class SubscriptionDetector
{
    private const int MinimumCharges = 2;

    /// <summary>Amounts closer than this are treated as the same price (rounding, exchange rate noise).</summary>
    private const decimal SamePriceTolerance = 0.02m;

    /// <summary>A bigger jump than this is more likely a different purchase than a price change.</summary>
    private const decimal MaxPriceChange = 0.5m;

    /// <param name="asOf">The last date the statements cover, so a renewal not yet due is not reported as missed.</param>
    public IReadOnlyList<DetectedSubscription> Detect(IEnumerable<MerchantCharges> merchants, DateOnly asOf) =>
        merchants.Select(m => Detect(m, asOf)).OfType<DetectedSubscription>().ToList();

    private static DetectedSubscription? Detect(MerchantCharges merchant, DateOnly asOf)
    {
        var charges = merchant.Charges.OrderBy(c => c.Date).ToList();
        if (charges.Count < MinimumCharges)
        {
            return null;
        }

        var gaps = charges.Zip(charges.Skip(1), (a, b) => b.Date.DayNumber - a.Date.DayNumber).ToList();
        if (CadenceRules.Match(gaps) is not { } cadence || !HasRecurringAmounts(charges))
        {
            return null;
        }

        var last = charges[^1];
        var next = CadenceRules.Next(cadence, last.Date);

        return new DetectedSubscription(
            merchant.MerchantId,
            cadence,
            last.Amount,
            charges[0].Date,
            last.Date,
            next,
            BuildEvents(charges, cadence, next, asOf));
    }

    /// <summary>
    /// Subscriptions keep one price, with the odd change. Spending that moves on every visit
    /// (groceries, fuel) is rejected here even when the dates happen to look regular.
    /// </summary>
    private static bool HasRecurringAmounts(List<Charge> charges)
    {
        var changes = charges.Zip(charges.Skip(1), (a, b) => Math.Abs(b.Amount - a.Amount) / a.Amount)
            .Where(change => change > SamePriceTolerance)
            .ToList();

        var allowedChanges = charges.Count >= 3 ? Math.Max(1, (charges.Count - 1) / 4) : 0;
        return changes.Count <= allowedChanges && changes.All(change => change <= MaxPriceChange);
    }

    private static List<DetectedEvent> BuildEvents(List<Charge> charges, BillingCadence cadence, DateOnly next, DateOnly asOf)
    {
        var events = new List<DetectedEvent>();

        for (var i = 1; i < charges.Count; i++)
        {
            var (previous, current) = (charges[i - 1], charges[i]);
            var changed = Math.Abs(current.Amount - previous.Amount) / previous.Amount > SamePriceTolerance;

            events.Add(changed
                ? new DetectedEvent(SubscriptionEventType.PriceChange, current.Date, previous.Amount, current.Amount)
                : new DetectedEvent(SubscriptionEventType.Renewal, current.Date, null, current.Amount));
        }

        if (asOf > next.AddDays(CadenceRules.GraceDays(cadence)))
        {
            events.Add(new DetectedEvent(SubscriptionEventType.Missed, next, null, charges[^1].Amount));
        }

        return events;
    }
}
