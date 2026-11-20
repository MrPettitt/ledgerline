using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Summary;

public static class MonthlyCost
{
    /// <summary>What one charge costs per month, so subscriptions billed at different intervals can be added up.</summary>
    public static decimal Of(decimal amount, BillingCadence cadence) => cadence switch
    {
        BillingCadence.Weekly => amount * 52 / 12,
        BillingCadence.Monthly => amount,
        BillingCadence.Quarterly => amount / 3,
        BillingCadence.Yearly => amount / 12,
        _ => throw new ArgumentOutOfRangeException(nameof(cadence))
    };
}
