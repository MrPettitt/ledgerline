using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;

namespace Ledgerline.Tests;

public class StatementParserTests
{
    private static readonly BankProfile Profile = new()
    {
        Name = "Test",
        DateColumn = "Date",
        DescriptionColumn = "Description",
        AmountColumn = "Amount",
        DateFormat = "yyyy-MM-dd",
        DebitsAreNegative = true
    };

    private static IReadOnlyList<ParsedTransaction> Parse(string csv, BankProfile? profile = null) =>
        new StatementParser().Parse(new StringReader(csv), profile ?? Profile);

    [Fact]
    public void Parse_KeepsDebitsAsPositiveAmountsAndSkipsCredits()
    {
        var rows = Parse("""
            Date,Description,Amount,Balance
            2026-08-03,EXAMPLE STREAMING 8841,-149.99,100.00
            2026-08-28,SALARY ACME LTD,28500.00,28600.00
            """);

        var debit = Assert.Single(rows);
        Assert.Equal(new DateOnly(2026, 8, 3), debit.Date);
        Assert.Equal("EXAMPLE STREAMING 8841", debit.Description);
        Assert.Equal(149.99m, debit.Amount);
    }

    [Fact]
    public void Parse_TreatsPositiveAmountsAsDebitsWhenProfileSaysSo()
    {
        var profile = new BankProfile
        {
            Name = "Positive",
            DateColumn = "Date",
            DescriptionColumn = "Description",
            AmountColumn = "Amount",
            DateFormat = "yyyy-MM-dd",
            DebitsAreNegative = false
        };

        var rows = Parse("Date,Description,Amount\n2026-08-03,SAMPLE GYM,399.00\n2026-08-04,REFUND,-10.00", profile);

        Assert.Equal(399.00m, Assert.Single(rows).Amount);
    }

    [Fact]
    public void Parse_MatchesColumnNamesIgnoringCaseAndSpaces()
    {
        var rows = Parse("  DATE ,description,AMOUNT\n2026-08-03,SAMPLE GYM,-399.00");

        Assert.Equal(399.00m, Assert.Single(rows).Amount);
    }

    [Fact]
    public void Parse_ThrowsWhenAColumnIsMissing()
    {
        var error = Assert.Throws<StatementFormatException>(() => Parse("Date,Details,Amount\n2026-08-03,X,-1.00"));

        Assert.Contains("Description", error.Message);
    }

    [Fact]
    public void Parse_ThrowsWithRowNumberWhenDateIsInvalid()
    {
        var error = Assert.Throws<StatementFormatException>(() => Parse("Date,Description,Amount\n03/08/2026,X,-1.00"));

        Assert.Contains("Row 2", error.Message);
    }

    [Fact]
    public void Parse_ThrowsWhenFileIsEmpty()
    {
        Assert.Throws<StatementFormatException>(() => Parse(string.Empty));
    }
}
