using Ledgerline.Core.Details;
using Ledgerline.Core.Domain;

namespace Ledgerline.Tests;

public class SubscriptionDetailBuilderTests
{
    private static readonly ChargeRow[] Charges =
    [
        new(new DateOnly(2026, 9, 3), "EXAMPLE STREAMING 8841", 149.99m),
        new(new DateOnly(2026, 10, 3), "EXAMPLE STREAMING 9120", 169.99m),
        new(new DateOnly(2026, 8, 3), "EXAMPLE STREAMING 8841", 149.99m)
    ];

    private static Subscription Streaming(params SubscriptionEvent[] events) => new()
    {
        Id = 7,
        Status = SubscriptionStatus.Confirmed,
        Cadence = BillingCadence.Monthly,
        CurrentAmount = 169.99m,
        FirstChargeDate = new DateOnly(2026, 8, 3),
        LastChargeDate = new DateOnly(2026, 10, 3),
        NextExpectedDate = new DateOnly(2026, 11, 3),
        Merchant = new Merchant { Key = "EXAMPLE STREAMING", DisplayName = "Example Streaming" },
        Events = [.. events]
    };

    private static SubscriptionEvent PriceChange(string date, decimal previous, decimal amount) => new()
    {
        Type = SubscriptionEventType.PriceChange,
        OccurredOn = DateOnly.Parse(date),
        PreviousAmount = previous,
        Amount = amount
    };

    private static SubscriptionDetail Build(Subscription subscription) =>
        new SubscriptionDetailBuilder().Build(subscription, Charges);

    [Fact]
    public void Build_ListsChargesNewestFirstAndAddsThemUp()
    {
        var detail = Build(Streaming(PriceChange("2026-10-03", 149.99m, 169.99m)));

        Assert.Equal(3, detail.ChargeCount);
        Assert.Equal(469.97m, detail.TotalPaid);
        Assert.Equal(new DateOnly(2026, 10, 3), detail.Charges[0].Date);
        Assert.Equal(new DateOnly(2026, 8, 3), detail.Charges[^1].Date);
    }

    [Fact]
    public void Build_StartsThePriceHistoryAtTheOriginalPrice()
    {
        var detail = Build(Streaming(PriceChange("2026-10-03", 149.99m, 169.99m)));

        Assert.Equal(2, detail.PriceHistory.Count);
        Assert.Equal(new PricePoint(new DateOnly(2026, 8, 3), 149.99m, null), detail.PriceHistory[0]);
        Assert.Equal(new PricePoint(new DateOnly(2026, 10, 3), 169.99m, 13.3m), detail.PriceHistory[1]);
    }

    [Fact]
    public void Build_ShowsASinglePricePointWhenThePriceNeverChanged()
    {
        var detail = Build(Streaming());

        Assert.Equal(new PricePoint(new DateOnly(2026, 8, 3), 169.99m, null), Assert.Single(detail.PriceHistory));
    }

    [Fact]
    public void Build_RecordsADecreaseAsANegativePercent()
    {
        var detail = Build(Streaming(PriceChange("2026-10-03", 200m, 180m)));

        Assert.Equal(-10.0m, detail.PriceHistory[1].ChangePercent);
    }

    [Fact]
    public void Build_CarriesTheSubscriptionFacts()
    {
        var detail = Build(Streaming());

        Assert.Equal("Example Streaming", detail.MerchantName);
        Assert.Equal(SubscriptionStatus.Confirmed, detail.Status);
        Assert.Equal(169.99m, detail.MonthlyCost);
        Assert.Equal(new DateOnly(2026, 11, 3), detail.NextExpectedDate);
    }
}
