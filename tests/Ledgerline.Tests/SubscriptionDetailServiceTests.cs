using Ledgerline.Core.Details;
using Ledgerline.Core.Detection;
using Ledgerline.Core.Importing;
using Ledgerline.Data;

namespace Ledgerline.Tests;

public sealed class SubscriptionDetailServiceTests : IDisposable
{
    private const string Csv = """
        Date,Description,Amount
        2026-08-03,EXAMPLE STREAMING 8841,-149.99
        2026-08-04,PRETEND GROCER 0231,-612.40
        2026-09-03,EXAMPLE STREAMING 8841,-149.99
        2026-10-03,EXAMPLE STREAMING 8841,-169.99
        """;

    private readonly TestDatabase _database = new();

    private async Task<(SubscriptionDetailService Service, int SubscriptionId)> SeededService()
    {
        await using var context = _database.CreateDbContext();
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Csv));
        await new StatementImporter(context, new StatementParser()).ImportAsync("test.csv", content, bankProfileId: 1);
        await new SubscriptionDetectionService(context, new SubscriptionDetector()).RunAsync();

        var service = new SubscriptionDetailService(_database.CreateDbContext(), new SubscriptionDetailBuilder());
        return (service, context.Subscriptions.Single().Id);
    }

    [Fact]
    public async Task Get_ReturnsOnlyTheChargesOfThatMerchant()
    {
        var (service, id) = await SeededService();

        var detail = await service.GetAsync(id);

        Assert.NotNull(detail);
        Assert.Equal("Example Streaming", detail.MerchantName);
        Assert.Equal(3, detail.ChargeCount);
        Assert.Equal(469.97m, detail.TotalPaid);
        Assert.DoesNotContain(detail.Charges, c => c.Description.Contains("GROCER"));
    }

    [Fact]
    public async Task Get_IncludesThePriceHistoryFromStoredEvents()
    {
        var (service, id) = await SeededService();

        var detail = await service.GetAsync(id);

        Assert.Equal([149.99m, 169.99m], detail!.PriceHistory.Select(p => p.Amount));
    }

    [Fact]
    public async Task Get_ReturnsNullForAnUnknownSubscription()
    {
        var (service, _) = await SeededService();

        Assert.Null(await service.GetAsync(9999));
    }

    public void Dispose() => _database.Dispose();
}
