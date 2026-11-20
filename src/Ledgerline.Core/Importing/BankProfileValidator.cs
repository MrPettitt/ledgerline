using System.Globalization;
using Ledgerline.Core.Domain;

namespace Ledgerline.Core.Importing;

public class BankProfileValidator
{
    private const int MaxNameLength = 60;
    private const int MaxColumnLength = 100;

    // The day is above 12 so a day-month swap in the format cannot round-trip by accident.
    private static readonly DateOnly SampleDate = new(2026, 11, 25);

    public IReadOnlyList<string> Validate(BankProfile profile, IEnumerable<string> otherProfileNames)
    {
        var errors = new List<string>();

        var name = profile.Name.Trim();
        if (name.Length == 0 || name.Length > MaxNameLength)
        {
            errors.Add($"Give the profile a name of 1 to {MaxNameLength} characters.");
        }
        else if (otherProfileNames.Any(other => string.Equals(other.Trim(), name, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"A profile called '{name}' already exists.");
        }

        var columns = new[]
        {
            ("Date column", profile.DateColumn),
            ("Description column", profile.DescriptionColumn),
            ("Amount column", profile.AmountColumn)
        };

        foreach (var (label, value) in columns)
        {
            if (value.Trim().Length is 0 or > MaxColumnLength)
            {
                errors.Add($"{label} must be 1 to {MaxColumnLength} characters.");
            }
        }

        var names = columns.Select(c => c.Item2.Trim()).Where(n => n.Length > 0).ToList();
        if (names.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            errors.Add("The date, description and amount columns must each be a different column.");
        }

        if (ExampleDate(profile.DateFormat) is null)
        {
            errors.Add("The date format must include a year, month and day, for example yyyy-MM-dd.");
        }

        return errors;
    }

    /// <summary>The sample date written in the given format, or null if the format cannot be used to read dates back.</summary>
    public static string? ExampleDate(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return null;
        }

        try
        {
            var text = SampleDate.ToString(format, CultureInfo.InvariantCulture);
            var roundTrips = DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                             && parsed == SampleDate;
            return roundTrips ? text : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
