namespace Ledgerline.Core.Domain;

/// <summary>Describes how to read the CSV layout of one bank.</summary>
public class BankProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DateColumn { get; set; } = string.Empty;
    public string DescriptionColumn { get; set; } = string.Empty;
    public string AmountColumn { get; set; } = string.Empty;
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>True when the bank exports money leaving the account as negative amounts.</summary>
    public bool DebitsAreNegative { get; set; } = true;
}
