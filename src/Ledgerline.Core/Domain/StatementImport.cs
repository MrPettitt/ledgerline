namespace Ledgerline.Core.Domain;

public class StatementImport
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset ImportedAt { get; set; }
    public int RowCount { get; set; }

    public int BankProfileId { get; set; }
    public BankProfile? BankProfile { get; set; }

    public List<Transaction> Transactions { get; set; } = [];
}
