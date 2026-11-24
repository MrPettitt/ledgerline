using Ledgerline.Core.Alerts;
using Ledgerline.Core.Detection;
using Ledgerline.Core.Importing;
using Ledgerline.Data;

namespace Ledgerline.Tests;

public sealed class AlertServiceTests : IDisposable
{
    // On 2026-11-02 this gives a price increase alert and a renewal alert.
    private const string Csv = """
        Date,Description,Amount
        2026-08-03,EXAMPLE STREAMING 8841,-149.99
        2026-09-03,EXAMPLE STREAMING 8841,-149.99
        2026-10-03,EXAMPLE STREAMING 8841,-169.99
        """;

    private static readonly DateOnly Today = new(2026, 11, 2);

    private readonly TestDatabase _database = new();

    private async Task<AlertService> SeededService()
    {
        await using var context = _database.CreateDbContext();
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Csv));
        await new StatementImporter(context, new StatementParser()).ImportAsync("test.csv", content, bankProfileId: 1);
        await new SubscriptionDetectionService(context, new SubscriptionDetector()).RunAsync();

        return new AlertService(_database.CreateDbContext(), new AlertBuilder());
    }

    [Fact]
    public async Task Get_ReturnsActiveAlertsBeforeAnythingIsDismissed()
    {
        var service = await SeededService();

        var list = await service.GetAsync(Today);

        Assert.Equal([AlertKind.PriceIncrease, AlertKind.UpcomingRenewal], list.Active.Select(a => a.Kind));
        Assert.Empty(list.Dismissed);
    }

    [Fact]
    public async Task Dismiss_MovesTheAlertToTheDismissedList()
    {
        var service = await SeededService();
        var increase = (await service.GetAsync(Today)).Active.First(a => a.Kind == AlertKind.PriceIncrease);

        await service.DismissAsync(increase.Key);

        var list = await service.GetAsync(Today);
        Assert.Equal([AlertKind.UpcomingRenewal], list.Active.Select(a => a.Kind));
        Assert.Equal([increase], list.Dismissed);
    }

    [Fact]
    public async Task Dismiss_TwiceStoresOnlyOneRow()
    {
        var service = await SeededService();
        var key = (await service.GetAsync(Today)).Active[0].Key;

        await service.DismissAsync(key);
        await service.DismissAsync(key);

        Assert.Single((await service.GetAsync(Today)).Dismissed);
    }

    [Fact]
    public async Task Restore_BringsTheAlertBack()
    {
        var service = await SeededService();
        var key = (await service.GetAsync(Today)).Active[0].Key;
        await service.DismissAsync(key);

        await service.RestoreAsync(key);

        var list = await service.GetAsync(Today);
        Assert.Equal(2, list.Active.Count);
        Assert.Empty(list.Dismissed);
    }

    public void Dispose() => _database.Dispose();
}
