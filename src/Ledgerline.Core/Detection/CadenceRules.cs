using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Detection;

public static class CadenceRules
{
    // Windows are wider than the calendar because charges slip over weekends and public holidays.
    private static readonly (BillingCadence Cadence, int MinDays, int MaxDays)[] Windows =
    [
        (BillingCadence.Weekly, 6, 8),
        (BillingCadence.Monthly, 26, 35),
        (BillingCadence.Quarterly, 84, 97),
        (BillingCadence.Yearly, 355, 375)
    ];

    public static BillingCadence? Match(IReadOnlyList<int> gapsInDays)
    {
        foreach (var (cadence, min, max) in Windows)
        {
            if (gapsInDays.All(gap => gap >= min && gap <= max))
            {
                return cadence;
            }
        }

        return null;
    }

    public static DateOnly Next(BillingCadence cadence, DateOnly from) => cadence switch
    {
        BillingCadence.Weekly => from.AddDays(7),
        BillingCadence.Monthly => from.AddMonths(1),
        BillingCadence.Quarterly => from.AddMonths(3),
        BillingCadence.Yearly => from.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(cadence))
    };

    /// <summary>How long after the expected date a charge may arrive before it counts as missed.</summary>
    public static int GraceDays(BillingCadence cadence) => cadence == BillingCadence.Weekly ? 2 : 5;
}
