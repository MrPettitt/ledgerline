namespace Ledgerline.Core.Domain;

public class Subscription
{
    public int Id { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Suspected;
    public BillingCadence Cadence { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly FirstChargeDate { get; set; }
    public DateOnly LastChargeDate { get; set; }
    public DateOnly NextExpectedDate { get; set; }

    public int MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public List<SubscriptionEvent> Events { get; set; } = [];
}
