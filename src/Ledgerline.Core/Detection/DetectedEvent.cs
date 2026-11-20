using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Detection;

public record DetectedEvent(SubscriptionEventType Type, DateOnly OccurredOn, decimal? PreviousAmount, decimal Amount);
