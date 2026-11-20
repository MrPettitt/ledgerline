using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Importing;

public class StatementParser
{
    public IReadOnlyList<ParsedTransaction> Parse(TextReader input, BankProfile profile)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant() };
        using var csv = new CsvReader(input, configuration);

        if (!csv.Read() || !csv.ReadHeader())
        {
            throw new StatementFormatException("The file is empty.");
        }

        RequireColumns(csv, profile);

        var debits = new List<ParsedTransaction>();
        while (csv.Read())
        {
            var row = csv.Parser.Row;
            var date = ParseDate(csv.GetField(profile.DateColumn), profile.DateFormat, row);
            var amount = ParseAmount(csv.GetField(profile.AmountColumn), row);
            var description = csv.GetField(profile.DescriptionColumn)?.Trim() ?? string.Empty;

            var debit = profile.DebitsAreNegative ? -amount : amount;
            if (debit > 0)
            {
                debits.Add(new ParsedTransaction(date, description, debit));
            }
        }

        return debits;
    }

    private static void RequireColumns(CsvReader csv, BankProfile profile)
    {
        var header = csv.HeaderRecord ?? [];
        foreach (var column in new[] { profile.DateColumn, profile.DescriptionColumn, profile.AmountColumn })
        {
            if (!header.Any(h => string.Equals(h.Trim(), column.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new StatementFormatException($"The file has no '{column}' column. Check the bank profile '{profile.Name}'.");
            }
        }
    }

    private static DateOnly ParseDate(string? value, string format, int row)
    {
        if (DateOnly.TryParseExact(value?.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new StatementFormatException($"Row {row}: '{value}' is not a date in the format {format}.");
    }

    private static decimal ParseAmount(string? value, int row)
    {
        if (decimal.TryParse(value?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }

        throw new StatementFormatException($"Row {row}: '{value}' is not an amount.");
    }
}
