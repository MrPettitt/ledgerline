using Ledgerline.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class SubscriptionStatusService(LedgerlineDbContext context)
{
    public async Task SetStatusAsync(int subscriptionId, SubscriptionStatus status)
    {
        var subscription = await context.Subscriptions.SingleAsync(s => s.Id == subscriptionId);
        subscription.Status = status;
        await context.SaveChangesAsync();
    }
}
