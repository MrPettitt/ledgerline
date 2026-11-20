using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;

namespace Ledgerline.Tests;

public class BankProfileValidatorTests
{
    private static BankProfile ValidProfile() => new()
    {
        Name = "Example Bank",
        DateColumn = "Date",
        DescriptionColumn = "Details",
        AmountColumn = "Amount",
        DateFormat = "dd/MM/yyyy"
    };

    private static IReadOnlyList<string> Validate(BankProfile profile, params string[] otherNames) =>
        new BankProfileValidator().Validate(profile, otherNames);

    [Fact]
    public void Validate_AcceptsAWellFormedProfile()
    {
        Assert.Empty(Validate(ValidProfile()));
    }

    [Fact]
    public void Validate_RejectsABlankName()
    {
        var profile = ValidProfile();
        profile.Name = "   ";

        Assert.Single(Validate(profile));
    }

    [Fact]
    public void Validate_RejectsANameAnotherProfileAlreadyUsesIgnoringCase()
    {
        var error = Assert.Single(Validate(ValidProfile(), "EXAMPLE BANK"));

        Assert.Contains("already exists", error);
    }

    [Fact]
    public void Validate_RejectsABlankColumn()
    {
        var profile = ValidProfile();
        profile.AmountColumn = "";

        Assert.Contains("Amount column", Assert.Single(Validate(profile)));
    }

    [Fact]
    public void Validate_RejectsTheSameColumnUsedTwice()
    {
        var profile = ValidProfile();
        profile.DescriptionColumn = "date";

        Assert.Contains("different column", Assert.Single(Validate(profile)));
    }

    [Theory]
    [InlineData("yyyy-MM-dd", "2026-11-25")]
    [InlineData("dd/MM/yyyy", "25/11/2026")]
    [InlineData("MM/dd/yyyy", "11/25/2026")]
    [InlineData("yyyyMMdd", "20261125")]
    public void ExampleDate_FormatsTheSampleDate(string format, string expected)
    {
        Assert.Equal(expected, BankProfileValidator.ExampleDate(format));
    }

    [Theory]
    [InlineData("")]
    [InlineData("yyyy-MM")]
    [InlineData("hello")]
    [InlineData("dd")]
    public void ExampleDate_RejectsFormatsThatCannotReadADateBack(string format)
    {
        Assert.Null(BankProfileValidator.ExampleDate(format));
    }
}
