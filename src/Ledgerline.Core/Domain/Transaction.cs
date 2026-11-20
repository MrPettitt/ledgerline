namespace Ledgerline.Core.Domain;

public class Transaction
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Positive amount of money that left the account.</summary>
    public decimal Amount { get; set; }

    /// <summary>Stable hash of date, description and amount so re-importing a statement adds nothing twice.</summary>
    public string Fingerprint { get; set; } = string.Empty;

    public int StatementImportId { get; set; }
    public StatementImport? StatementImport { get; set; }

    public int? MerchantId { get; set; }
    public Merchant? Merchant { get; set; }
}
