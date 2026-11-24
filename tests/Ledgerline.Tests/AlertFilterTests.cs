using Ledgerline.Core.Alerts;

namespace Ledgerline.Tests;

public class AlertFilterTests
{
    private static Alert PriceIncrease(string date) =>
        new(AlertKind.PriceIncrease, 1, "Example Streaming", DateOnly.Parse(date), 169.99m, 149.99m);

    private static Alert Renewal(string date) =>
        new(AlertKind.UpcomingRenewal, 1, "Example Streaming", DateOnly.Parse(date), 169.99m, null);

    [Fact]
    public void Split_SeparatesDismissedAlertsFromActiveOnes()
    {
        var increase = PriceIncrease("2026-10-03");
        var renewal = Renewal("2026-11-03");

        var list = AlertFilter.Split([increase, renewal], new HashSet<AlertKey> { increase.Key });

        Assert.Equal([renewal], list.Active);
        Assert.Equal([increase], list.Dismissed);
    }

    [Fact]
    public void Split_ShowsTheSameKindOfAlertAgainWhenItsDateIsDifferent()
    {
        var dismissed = PriceIncrease("2026-10-03");
        var newer = PriceIncrease("2026-11-03");

        var list = AlertFilter.Split([newer], new HashSet<AlertKey> { dismissed.Key });

        Assert.Equal([newer], list.Active);
        Assert.Empty(list.Dismissed);
    }

    [Fact]
    public void Split_ReturnsEverythingActiveWhenNothingWasDismissed()
    {
        var list = AlertFilter.Split([Renewal("2026-11-03")], new HashSet<AlertKey>());

        Assert.Single(list.Active);
        Assert.Empty(list.Dismissed);
    }
}
