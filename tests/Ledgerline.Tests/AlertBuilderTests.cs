using Ledgerline.Core.Alerts;
using Ledgerline.Core.Domain;

namespace Ledgerline.Tests;

public class AlertBuilderTests
{
    private static readonly DateOnly Today = new(2026, 11, 20);

    private static Subscription Subscription(
        string next,
        string last = "2026-10-25",
        SubscriptionStatus status = SubscriptionStatus.Suspected,
        params SubscriptionEvent[] events) => new()
    {
        Id = 1,
        Status = status,
        CurrentAmount = 349m,
        LastChargeDate = DateOnly.Parse(last),
        NextExpectedDate = DateOnly.Parse(next),
        Merchant = new Merchant { Key = "TESTCO MOBILE CONTRACT", DisplayName = "Testco Mobile Contract" },
        Events = [.. events]
    };

    private static SubscriptionEvent Event(SubscriptionEventType type, string date, decimal? previous = null, decimal amount = 349m) =>
        new() { Type = type, OccurredOn = DateOnly.Parse(date), PreviousAmount = previous, Amount = amount };

    private static IReadOnlyList<Alert> Build(params Subscription[] subscriptions) =>
        new AlertBuilder().Build(subscriptions, Today);

    [Theory]
    [InlineData("2026-11-20", true)]
    [InlineData("2026-11-27", true)]
    [InlineData("2026-11-28", false)]
    [InlineData("2026-11-19", false)]
    public void Build_WarnsAboutRenewalsInsideTheWindow(string next, bool expected)
    {
        var alerts = Build(Subscription(next));

        Assert.Equal(expected, alerts.Any(a => a.Kind == AlertKind.UpcomingRenewal));
    }

    [Fact]
    public void Build_ReportsAMissedRenewal()
    {
        var missed = Event(SubscriptionEventType.Missed, "2026-10-18");

        var alert = Assert.Single(Build(Subscription("2026-10-18", last: "2026-09-18", events: missed)));

        Assert.Equal(AlertKind.MissedRenewal, alert.Kind);
    }

    [Fact]
    public void Build_DropsAMissedRenewalOnceALaterChargeArrived()
    {
        var missed = Event(SubscriptionEventType.Missed, "2026-10-18");

        Assert.Empty(Build(Subscription("2027-01-01", last: "2026-12-01", events: missed)));
    }

    [Fact]
    public void Build_ReportsARecentPriceIncrease()
    {
        var change = Event(SubscriptionEventType.PriceChange, "2026-10-03", previous: 149.99m, amount: 169.99m);

        var alert = Assert.Single(Build(Subscription("2026-12-25", events: change)));

        Assert.Equal(AlertKind.PriceIncrease, alert.Kind);
        Assert.Equal(149.99m, alert.PreviousAmount);
        Assert.Equal(169.99m, alert.Amount);
    }

    [Fact]
    public void Build_IgnoresPriceDecreasesAndOldIncreases()
    {
        var decrease = Event(SubscriptionEventType.PriceChange, "2026-11-01", previous: 169.99m, amount: 149.99m);
        var old = Event(SubscriptionEventType.PriceChange, "2026-06-01", previous: 100m, amount: 120m);

        Assert.Empty(Build(Subscription("2026-12-25", events: [decrease, old])));
    }

    [Theory]
    [InlineData(SubscriptionStatus.Ignored)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public void Build_SkipsSubscriptionsTheUserIsDoneWith(SubscriptionStatus status)
    {
        Assert.Empty(Build(Subscription("2026-11-22", status: status)));
    }

    [Fact]
    public void Build_ListsMostPressingAlertsFirst()
    {
        var missed = Event(SubscriptionEventType.Missed, "2026-10-18");
        var renewing = Subscription("2026-11-22");
        var lapsed = Subscription("2026-10-18", last: "2026-09-18", events: missed);

        var kinds = Build(renewing, lapsed).Select(a => a.Kind);

        Assert.Equal([AlertKind.MissedRenewal, AlertKind.UpcomingRenewal], kinds);
    }
}
