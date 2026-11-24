using Ledgerline.Core.Alerts;

namespace Ledgerline.Core.Domain;

/// <summary>
/// An alert the user has dealt with. It is identified by subscription, kind and date, so the same
/// kind of alert comes back for a new price change or a later missed renewal.
/// </summary>
public class DismissedAlert
{
    public int Id { get; set; }
    public AlertKind Kind { get; set; }
    public DateOnly AlertDate { get; set; }
    public DateTimeOffset DismissedAt { get; set; }

    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
}
