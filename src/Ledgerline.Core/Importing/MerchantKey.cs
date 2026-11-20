namespace Ledgerline.Core.Importing;

public static class MerchantKey
{
    /// <summary>
    /// Reduces a bank description to the part that identifies the merchant. Tokens containing digits
    /// are dropped because banks append reference and terminal numbers that change on every charge.
    /// </summary>
    public static string From(string description)
    {
        var words = description
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => !word.Any(char.IsDigit));

        var key = string.Join(' ', words).ToUpperInvariant();
        return key.Length > 0 ? key : description.Trim().ToUpperInvariant();
    }
}
