using Ledgerline.Core.Detection;
using Ledgerline.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class SubscriptionDetectionService(LedgerlineDbContext context, SubscriptionDetector detector)
{
    /// <summary>Detects subscriptions in all stored transactions and returns how many were found.</summary>
    public async Task<int> RunAsync()
    {
        // Loaded into memory first because SQLite cannot order or compare decimals in SQL.
        var rows = await context.Transactions
            .Where(t => t.MerchantId != null)
            .Select(t => new { MerchantId = t.MerchantId!.Value, t.Date, t.Amount })
            .ToListAsync();

        if (rows.Count == 0)
        {
            return 0;
        }

        var merchants = rows
            .GroupBy(r => r.MerchantId)
            .Select(g => new MerchantCharges(g.Key, g.Select(r => new Charge(r.Date, r.Amount)).ToList()));

        var detected = detector.Detect(merchants, asOf: rows.Max(r => r.Date));
        var existing = await context.Subscriptions.Include(s => s.Events).ToDictionaryAsync(s => s.MerchantId);

        foreach (var found in detected)
        {
            if (!existing.TryGetValue(found.MerchantId, out var subscription))
            {
                subscription = new Subscription { MerchantId = found.MerchantId };
                context.Subscriptions.Add(subscription);
            }

            Update(subscription, found);
        }

        await context.SaveChangesAsync();
        return detected.Count;
    }

    // Status is left alone so a subscription the user confirmed or ignored stays that way.
    private static void Update(Subscription subscription, DetectedSubscription found)
    {
        subscription.Cadence = found.Cadence;
        subscription.CurrentAmount = found.CurrentAmount;
        subscription.FirstChargeDate = found.FirstChargeDate;
        subscription.LastChargeDate = found.LastChargeDate;
        subscription.NextExpectedDate = found.NextExpectedDate;

        var known = subscription.Events.Select(e => (e.Type, e.OccurredOn)).ToHashSet();
        foreach (var detectedEvent in found.Events.Where(e => !known.Contains((e.Type, e.OccurredOn))))
        {
            subscription.Events.Add(new SubscriptionEvent
            {
                Type = detectedEvent.Type,
                OccurredOn = detectedEvent.OccurredOn,
                PreviousAmount = detectedEvent.PreviousAmount,
                Amount = detectedEvent.Amount
            });
        }
    }
}
