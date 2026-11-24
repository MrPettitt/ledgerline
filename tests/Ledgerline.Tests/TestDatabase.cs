using Ledgerline.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

/// <summary>A migrated SQLite file in the temp folder that is removed when the test finishes.</summary>
public sealed class TestDatabase : IDisposable, IDbContextFactory<LedgerlineDbContext>
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");
    private readonly DbContextOptions<LedgerlineDbContext> _options;

    public TestDatabase()
    {
        _options = new DbContextOptionsBuilder<LedgerlineDbContext>().UseSqlite($"Data Source={_path}").Options;
        using var context = CreateDbContext();
        context.Database.Migrate();
    }

    public LedgerlineDbContext CreateDbContext() => new(_options);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }
}
