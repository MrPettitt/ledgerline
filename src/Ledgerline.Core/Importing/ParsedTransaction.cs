namespace Ledgerline.Core.Importing;

/// <summary>A debit read from a statement. Amount is the positive sum that left the account.</summary>
public record ParsedTransaction(DateOnly Date, string Description, decimal Amount);
