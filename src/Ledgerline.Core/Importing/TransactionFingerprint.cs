using System.Security.Cryptography;
using System.Text;

namespace Ledgerline.Core.Importing;

public static class TransactionFingerprint
{
    /// <param name="occurrence">
    /// Distinguishes identical rows on the same day, such as two coffees at the same price,
    /// so they are not mistaken for a duplicate import.
    /// </param>
    public static string Create(ParsedTransaction transaction, int occurrence)
    {
        var text = $"{transaction.Date:yyyy-MM-dd}|{transaction.Description}|{transaction.Amount}|{occurrence}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    }
}
