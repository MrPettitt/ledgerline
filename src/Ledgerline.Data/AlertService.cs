using Ledgerline.Core.Alerts;
using Ledgerline.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class AlertService(LedgerlineDbContext context, AlertBuilder builder)
{
    public async Task<AlertList> GetAsync(DateOnly today)
    {
        var subscriptions = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Merchant)
            .Include(s => s.Events)
            .ToListAsync();

        var dismissed = (await context.DismissedAlerts
                .AsNoTracking()
                .Select(d => new { d.Kind, d.SubscriptionId, d.AlertDate })
                .ToListAsync())
            .Select(d => new AlertKey(d.Kind, d.SubscriptionId, d.AlertDate))
            .ToHashSet();

        return AlertFilter.Split(builder.Build(subscriptions, today), dismissed);
    }

    public async Task DismissAsync(AlertKey key)
    {
        var alreadyDismissed = await context.DismissedAlerts
            .AnyAsync(d => d.SubscriptionId == key.SubscriptionId && d.Kind == key.Kind && d.AlertDate == key.Date);
        if (alreadyDismissed)
        {
            return;
        }

        context.DismissedAlerts.Add(new DismissedAlert
        {
            SubscriptionId = key.SubscriptionId,
            Kind = key.Kind,
            AlertDate = key.Date,
            DismissedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }

    public async Task RestoreAsync(AlertKey key)
    {
        var rows = await context.DismissedAlerts
            .Where(d => d.SubscriptionId == key.SubscriptionId && d.Kind == key.Kind && d.AlertDate == key.Date)
            .ToListAsync();

        context.DismissedAlerts.RemoveRange(rows);
        await context.SaveChangesAsync();
    }
}
