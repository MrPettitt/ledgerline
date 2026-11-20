using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class StatementImporter(LedgerlineDbContext context, StatementParser parser)
{
    public async Task<ImportResult> ImportAsync(string fileName, Stream content, int bankProfileId)
    {
        var profile = await context.BankProfiles.SingleAsync(p => p.Id == bankProfileId);

        using var reader = new StreamReader(content);
        var parsed = parser.Parse(reader, profile);

        var fingerprinted = Fingerprint(parsed);
        var known = await KnownFingerprintsAsync(fingerprinted.Select(f => f.Fingerprint));
        var fresh = fingerprinted.Where(f => !known.Contains(f.Fingerprint)).ToList();

        var statement = new StatementImport
        {
            FileName = fileName,
            ImportedAt = DateTimeOffset.UtcNow,
            RowCount = parsed.Count,
            BankProfileId = profile.Id
        };
        context.StatementImports.Add(statement);

        var merchants = await MerchantsByKeyAsync(fresh.Select(f => MerchantKey.From(f.Transaction.Description)));
        foreach (var (transaction, fingerprint) in fresh)
        {
            statement.Transactions.Add(new Transaction
            {
                Date = transaction.Date,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Fingerprint = fingerprint,
                Merchant = merchants[MerchantKey.From(transaction.Description)]
            });
        }

        await context.SaveChangesAsync();
        return new ImportResult(parsed.Count, fresh.Count, parsed.Count - fresh.Count);
    }

    private static List<(ParsedTransaction Transaction, string Fingerprint)> Fingerprint(IReadOnlyList<ParsedTransaction> parsed)
    {
        var seen = new Dictionary<ParsedTransaction, int>();
        var result = new List<(ParsedTransaction, string)>();

        foreach (var transaction in parsed)
        {
            seen[transaction] = seen.GetValueOrDefault(transaction) + 1;
            result.Add((transaction, TransactionFingerprint.Create(transaction, seen[transaction])));
        }

        return result;
    }

    private async Task<HashSet<string>> KnownFingerprintsAsync(IEnumerable<string> fingerprints)
    {
        var candidates = fingerprints.ToList();
        var known = await context.Transactions
            .Where(t => candidates.Contains(t.Fingerprint))
            .Select(t => t.Fingerprint)
            .ToListAsync();

        return [.. known];
    }

    private async Task<Dictionary<string, Merchant>> MerchantsByKeyAsync(IEnumerable<string> keys)
    {
        var wanted = keys.Distinct().ToList();
        var merchants = await context.Merchants.Where(m => wanted.Contains(m.Key)).ToDictionaryAsync(m => m.Key);

        foreach (var key in wanted.Where(k => !merchants.ContainsKey(k)))
        {
            merchants[key] = new Merchant { Key = key, DisplayName = ToDisplayName(key) };
        }

        return merchants;
    }

    private static string ToDisplayName(string key) =>
        System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(key.ToLowerInvariant());
}
