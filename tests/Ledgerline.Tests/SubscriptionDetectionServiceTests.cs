using Ledgerline.Core.Detection;
using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;
using Ledgerline.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

public sealed class SubscriptionDetectionServiceTests : IDisposable
{
    private const string Csv = """
        Date,Description,Amount
        2026-08-03,EXAMPLE STREAMING 8841,-149.99
        2026-08-04,PRETEND GROCER 0231,-612.40
        2026-09-03,EXAMPLE STREAMING 8841,-149.99
        2026-09-06,PRETEND GROCER 0231,-701.15
        2026-10-03,EXAMPLE STREAMING 8841,-169.99
        2026-10-07,PRETEND GROCER 0231,-540.60
        """;

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");
    private readonly LedgerlineDbContext _context;

    public SubscriptionDetectionServiceTests()
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        _context = new LedgerlineDbContext(options);
        _context.Database.Migrate();
    }

    private async Task ImportSample()
    {
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Csv));
        await new StatementImporter(_context, new StatementParser()).ImportAsync("test.csv", content, bankProfileId: 1);
    }

    private SubscriptionDetectionService Service() => new(_context, new SubscriptionDetector());

    [Fact]
    public async Task Run_StoresSubscriptionsWithTheirEvents()
    {
        await ImportSample();

        var count = await Service().RunAsync();

        Assert.Equal(1, count);
        var subscription = await _context.Subscriptions.Include(s => s.Merchant).Include(s => s.Events).SingleAsync();
        Assert.Equal("Example Streaming", subscription.Merchant!.DisplayName);
        Assert.Equal(SubscriptionStatus.Suspected, subscription.Status);
        Assert.Contains(subscription.Events, e => e.Type == SubscriptionEventType.PriceChange);
    }

    [Fact]
    public async Task Run_TwiceDoesNotDuplicateSubscriptionsOrEvents()
    {
        await ImportSample();
        await Service().RunAsync();
        var events = await _context.SubscriptionEvents.CountAsync();

        await Service().RunAsync();

        Assert.Equal(1, await _context.Subscriptions.CountAsync());
        Assert.Equal(events, await _context.SubscriptionEvents.CountAsync());
    }

    [Fact]
    public async Task Run_KeepsTheStatusTheUserChose()
    {
        await ImportSample();
        await Service().RunAsync();
        var subscription = await _context.Subscriptions.SingleAsync();
        subscription.Status = SubscriptionStatus.Confirmed;
        await _context.SaveChangesAsync();

        await Service().RunAsync();

        Assert.Equal(SubscriptionStatus.Confirmed, (await _context.Subscriptions.SingleAsync()).Status);
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
