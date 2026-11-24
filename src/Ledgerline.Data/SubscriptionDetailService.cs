using Ledgerline.Core.Details;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class SubscriptionDetailService(LedgerlineDbContext context, SubscriptionDetailBuilder builder)
{
    /// <summary>Returns null when no subscription has the given id.</summary>
    public async Task<SubscriptionDetail?> GetAsync(int subscriptionId)
    {
        var subscription = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Merchant)
            .Include(s => s.Events)
            .SingleOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription is null)
        {
            return null;
        }

        // Loaded into memory first because SQLite cannot order or add up decimals in SQL.
        var charges = (await context.Transactions
                .AsNoTracking()
                .Where(t => t.MerchantId == subscription.MerchantId)
                .Select(t => new { t.Date, t.Description, t.Amount })
                .ToListAsync())
            .Select(t => new ChargeRow(t.Date, t.Description, t.Amount));

        return builder.Build(subscription, charges);
    }
}
