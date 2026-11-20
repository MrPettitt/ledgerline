using Ledgerline.Core.Domain;
using Ledgerline.Core.Summary;

namespace Ledgerline.Tests;

public class OverviewSummaryBuilderTests
{
    private static readonly DateOnly Today = new(2026, 11, 20);

    private static Subscription Subscription(
        string name,
        decimal amount,
        BillingCadence cadence = BillingCadence.Monthly,
        string next = "2027-06-01",
        SubscriptionStatus status = SubscriptionStatus.Suspected) => new()
    {
        Status = status,
        Cadence = cadence,
        CurrentAmount = amount,
        NextExpectedDate = DateOnly.Parse(next),
        Merchant = new Merchant { Key = name.ToUpperInvariant(), DisplayName = name }
    };

    private static OverviewSummary Build(params Subscription[] subscriptions) =>
        new OverviewSummaryBuilder().Build(subscriptions, Today);

    [Theory]
    [InlineData(BillingCadence.Monthly, 100, 100)]
    [InlineData(BillingCadence.Quarterly, 300, 100)]
    [InlineData(BillingCadence.Yearly, 1200, 100)]
    [InlineData(BillingCadence.Weekly, 12, 52)]
    public void MonthlyCost_NormalisesEveryCadence(BillingCadence cadence, decimal amount, decimal expected)
    {
        Assert.Equal(expected, MonthlyCost.Of(amount, cadence));
    }

    [Fact]
    public void Build_AddsUpMonthlyAndYearlySpendAcrossCadences()
    {
        var summary = Build(
            Subscription("Sample Gym", 399m),
            Subscription("Demo Cloud", 1200m, BillingCadence.Yearly));

        Assert.Equal(499m, summary.MonthlyTotal);
        Assert.Equal(5988m, summary.YearlyTotal);
    }

    [Fact]
    public void Build_LeavesOutIgnoredAndCancelledSubscriptions()
    {
        var summary = Build(
            Subscription("Sample Gym", 399m, status: SubscriptionStatus.Confirmed),
            Subscription("Fake Music", 80m, status: SubscriptionStatus.Ignored),
            Subscription("Old Service", 50m, status: SubscriptionStatus.Cancelled));

        Assert.Equal(399m, summary.MonthlyTotal);
        Assert.Equal(1, summary.ConfirmedCount);
        Assert.Equal(0, summary.SuspectedCount);
    }

    [Fact]
    public void Build_ListsTheMostExpensiveFirstAndCapsTheList()
    {
        var subscriptions = Enumerable.Range(1, 7).Select(i => Subscription($"Service {i}", i * 10m)).ToArray();

        var summary = Build(subscriptions);

        Assert.Equal(OverviewSummaryBuilder.ListLength, summary.MostExpensive.Count);
        Assert.Equal("Service 7", summary.MostExpensive[0].MerchantName);
    }

    [Fact]
    public void Build_ListsRenewalsInTheNext30DaysInDateOrder()
    {
        var summary = Build(
            Subscription("Later", 10m, next: "2026-12-15"),
            Subscription("Sooner", 10m, next: "2026-11-22"),
            Subscription("Too far", 10m, next: "2026-12-21"),
            Subscription("Past", 10m, next: "2026-11-19"));

        Assert.Equal(["Sooner", "Later"], summary.UpcomingRenewals.Select(r => r.MerchantName));
    }

    [Fact]
    public void Build_ReturnsAnEmptySummaryWhenThereAreNoSubscriptions()
    {
        var summary = Build();

        Assert.Equal(0, summary.ActiveCount);
        Assert.Equal(0m, summary.MonthlyTotal);
    }
}
