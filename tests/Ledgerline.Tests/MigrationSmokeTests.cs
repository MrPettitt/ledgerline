using Ledgerline.Core.Domain;
using Ledgerline.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

public sealed class MigrationSmokeTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");

    [Fact]
    public void Migrate_CreatesUsableSchemaInTemporaryDatabase()
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        using var context = new LedgerlineDbContext(options);
        context.Database.Migrate();

        context.Merchants.Add(new Merchant { Key = "example streaming", DisplayName = "Example Streaming" });
        context.SaveChanges();

        Assert.Empty(context.Database.GetPendingMigrations());
        Assert.Equal(1, context.Merchants.Count());
    }

    public void Dispose()
    {
        // Pooled connections keep the file locked on Windows until cleared.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
