using Ledgerline.Core.Importing;
using Ledgerline.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

public sealed class StatementImporterTests : IDisposable
{
    private const string Csv = """
        Date,Description,Amount,Balance
        2026-08-03,EXAMPLE STREAMING 8841,-149.99,1.00
        2026-09-03,EXAMPLE STREAMING 9120,-149.99,1.00
        2026-09-04,IMAGINARY CAFE,-48.50,1.00
        2026-09-04,IMAGINARY CAFE,-48.50,1.00
        2026-09-28,SALARY ACME LTD,28500.00,1.00
        """;

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");
    private readonly LedgerlineDbContext _context;

    public StatementImporterTests()
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        _context = new LedgerlineDbContext(options);
        _context.Database.Migrate();
    }

    private static MemoryStream Stream(string csv) => new(System.Text.Encoding.UTF8.GetBytes(csv));

    private Task<ImportResult> Import(string csv) =>
        new StatementImporter(_context, new StatementParser()).ImportAsync("test.csv", Stream(csv), bankProfileId: 1);

    [Fact]
    public async Task Import_StoresDebitsAndGroupsThemByMerchant()
    {
        var result = await Import(Csv);

        Assert.Equal(new ImportResult(RowsRead: 4, Added: 4, Duplicates: 0), result);
        Assert.Equal(2, await _context.Merchants.CountAsync());
        Assert.Equal(2, await _context.Transactions.CountAsync(t => t.Merchant!.Key == "EXAMPLE STREAMING"));
    }

    [Fact]
    public async Task Import_KeepsIdenticalRowsOnTheSameDay()
    {
        await Import(Csv);

        Assert.Equal(2, await _context.Transactions.CountAsync(t => t.Description == "IMAGINARY CAFE"));
    }

    [Fact]
    public async Task Import_SkipsRowsAlreadyImported()
    {
        await Import(Csv);

        var second = await Import(Csv);

        Assert.Equal(new ImportResult(RowsRead: 4, Added: 0, Duplicates: 4), second);
        Assert.Equal(4, await _context.Transactions.CountAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
