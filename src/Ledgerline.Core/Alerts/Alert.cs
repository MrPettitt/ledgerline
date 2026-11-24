namespace Ledgerline.Core.Alerts;

/// <param name="Date">The renewal date, or the date the new price was charged.</param>
/// <param name="PreviousAmount">Set for price increases only.</param>
public record Alert(AlertKind Kind, int SubscriptionId, string MerchantName, DateOnly Date, decimal Amount, decimal? PreviousAmount)
{
    public AlertKey Key => new(Kind, SubscriptionId, Date);
}
