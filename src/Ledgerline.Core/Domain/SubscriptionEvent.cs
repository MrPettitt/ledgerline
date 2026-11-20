namespace Ledgerline.Core.Domain;

public class SubscriptionEvent
{
    public int Id { get; set; }
    public SubscriptionEventType Type { get; set; }
    public DateOnly OccurredOn { get; set; }

    /// <summary>Set for price changes only.</summary>
    public decimal? PreviousAmount { get; set; }

    public decimal? Amount { get; set; }

    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
}
