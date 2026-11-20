using Ledgerline.Core.Importing;

namespace Ledgerline.Tests;

public class MerchantKeyTests
{
    [Theory]
    [InlineData("EXAMPLE STREAMING 8841", "EXAMPLE STREAMING")]
    [InlineData("example streaming 1207", "EXAMPLE STREAMING")]
    [InlineData("SAMPLE GYM MONTHLY", "SAMPLE GYM MONTHLY")]
    [InlineData("  DEMO   CLOUD STORAGE ", "DEMO CLOUD STORAGE")]
    public void From_RemovesReferenceNumbersAndCase(string description, string expected)
    {
        Assert.Equal(expected, MerchantKey.From(description));
    }

    [Fact]
    public void From_FallsBackToTheDescriptionWhenEveryWordHasDigits()
    {
        Assert.Equal("7ELEVEN 123", MerchantKey.From("7eleven 123"));
    }
}
