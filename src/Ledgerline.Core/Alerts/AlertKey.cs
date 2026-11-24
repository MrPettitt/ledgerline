namespace Ledgerline.Core.Alerts;

public record AlertKey(AlertKind Kind, int SubscriptionId, DateOnly Date);
