namespace Ledgerline.Core.Domain;

public class Merchant
{
    public int Id { get; set; }

    /// <summary>Normalised description used to match transactions to this merchant.</summary>
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
