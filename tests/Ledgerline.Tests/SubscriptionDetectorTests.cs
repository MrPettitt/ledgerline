using Ledgerline.Core.Detection;
using Ledgerline.Core.Domain;

namespace Ledgerline.Tests;

public class SubscriptionDetectorTests
{
    private static readonly DateOnly AsOf = new(2026, 10, 28);

    private static DetectedSubscription? DetectOne(params (string Date, decimal Amount)[] charges)
    {
        var merchant = new MerchantCharges(1, charges.Select(c => new Charge(DateOnly.Parse(c.Date), c.Amount)).ToList());
        return new SubscriptionDetector().Detect([merchant], AsOf).SingleOrDefault();
    }

    [Fact]
    public void Detect_FindsMonthlyChargesWithAFixedPrice()
    {
        var found = DetectOne(("2026-08-05", 399m), ("2026-09-05", 399m), ("2026-10-05", 399m));

        Assert.NotNull(found);
        Assert.Equal(BillingCadence.Monthly, found.Cadence);
        Assert.Equal(399m, found.CurrentAmount);
        Assert.Equal(new DateOnly(2026, 11, 5), found.NextExpectedDate);
        Assert.Equal(2, found.Events.Count(e => e.Type == SubscriptionEventType.Renewal));
    }

    [Fact]
    public void Detect_AcceptsTwoMatchingCharges()
    {
        Assert.NotNull(DetectOne(("2026-09-12", 59m), ("2026-10-12", 59m)));
    }

    [Fact]
    public void Detect_IgnoresASingleCharge()
    {
        Assert.Null(DetectOne(("2026-10-12", 59m)));
    }

    [Fact]
    public void Detect_FlagsAPriceIncrease()
    {
        var found = DetectOne(("2026-08-03", 149.99m), ("2026-09-03", 149.99m), ("2026-10-03", 169.99m));

        var change = Assert.Single(found!.Events, e => e.Type == SubscriptionEventType.PriceChange);
        Assert.Equal(new DateOnly(2026, 10, 3), change.OccurredOn);
        Assert.Equal(149.99m, change.PreviousAmount);
        Assert.Equal(169.99m, change.Amount);
        Assert.Equal(169.99m, found.CurrentAmount);
    }

    [Fact]
    public void Detect_FlagsARenewalThatNeverArrived()
    {
        var found = DetectOne(("2026-08-18", 79.99m), ("2026-09-18", 79.99m));

        var missed = Assert.Single(found!.Events, e => e.Type == SubscriptionEventType.Missed);
        Assert.Equal(new DateOnly(2026, 10, 18), missed.OccurredOn);
    }

    [Fact]
    public void Detect_DoesNotFlagARenewalThatIsNotDueYet()
    {
        var found = DetectOne(("2026-08-25", 349m), ("2026-09-25", 349m), ("2026-10-25", 349m));

        Assert.DoesNotContain(found!.Events, e => e.Type == SubscriptionEventType.Missed);
    }

    [Fact]
    public void Detect_IgnoresRegularDatesWhenTheAmountKeepsChanging()
    {
        Assert.Null(DetectOne(("2026-08-09", 820m), ("2026-09-10", 795.50m), ("2026-10-11", 842.25m)));
    }

    [Fact]
    public void Detect_IgnoresIrregularDates()
    {
        Assert.Null(DetectOne(("2026-08-04", 50m), ("2026-08-15", 50m), ("2026-10-02", 50m)));
    }

    [Fact]
    public void Detect_RecognisesYearlyCharges()
    {
        var found = DetectOne(("2025-03-14", 1200m), ("2026-03-14", 1200m));

        Assert.Equal(BillingCadence.Yearly, found!.Cadence);
    }

    [Fact]
    public void Detect_TreatsABigJumpAsADifferentPurchaseNotAPriceChange()
    {
        Assert.Null(DetectOne(("2026-08-05", 100m), ("2026-09-05", 100m), ("2026-10-05", 400m)));
    }
}
