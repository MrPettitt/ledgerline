using Ledgerline.Core.Domain;
using Ledgerline.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

public sealed class SubscriptionStatusServiceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");
    private readonly LedgerlineDbContext _context;

    public SubscriptionStatusServiceTests()
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        _context = new LedgerlineDbContext(options);
        _context.Database.Migrate();
    }

    [Theory]
    [InlineData(SubscriptionStatus.Confirmed)]
    [InlineData(SubscriptionStatus.Ignored)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public async Task SetStatus_StoresTheNewStatus(SubscriptionStatus status)
    {
        var subscription = new Subscription
        {
            Merchant = new Merchant { Key = "DEMO CLOUD STORAGE", DisplayName = "Demo Cloud Storage" }
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        await new SubscriptionStatusService(_context).SetStatusAsync(subscription.Id, status);

        _context.ChangeTracker.Clear();
        Assert.Equal(status, (await _context.Subscriptions.SingleAsync()).Status);
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
