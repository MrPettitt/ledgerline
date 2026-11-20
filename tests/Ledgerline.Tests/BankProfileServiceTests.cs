using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;
using Ledgerline.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Tests;

public sealed class BankProfileServiceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ledgerline-{Guid.NewGuid():N}.db");
    private readonly LedgerlineDbContext _context;
    private readonly BankProfileService _service;

    public BankProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        _context = new LedgerlineDbContext(options);
        _context.Database.Migrate();
        _service = new BankProfileService(_context, new BankProfileValidator());
    }

    private static BankProfile NewProfile(string name = "Example Bank") => new()
    {
        Name = name,
        DateColumn = " Date ",
        DescriptionColumn = "Details",
        AmountColumn = "Amount",
        DateFormat = "dd/MM/yyyy",
        DebitsAreNegative = false
    };

    [Fact]
    public async Task Save_AddsANewProfileWithTrimmedColumns()
    {
        var errors = await _service.SaveAsync(NewProfile());

        Assert.Empty(errors);
        var saved = (await _service.ListAsync()).Single(p => p.Name == "Example Bank");
        Assert.Equal("Date", saved.DateColumn);
        Assert.False(saved.DebitsAreNegative);
    }

    [Fact]
    public async Task Save_UpdatesAnExistingProfileInPlace()
    {
        await _service.SaveAsync(NewProfile());
        var existing = (await _service.ListAsync()).Single(p => p.Name == "Example Bank");
        existing.DateFormat = "yyyy/MM/dd";

        var errors = await _service.SaveAsync(existing);

        Assert.Empty(errors);
        Assert.Equal(2, await _context.BankProfiles.CountAsync());
        Assert.Equal("yyyy/MM/dd", (await _service.ListAsync()).Single(p => p.Name == "Example Bank").DateFormat);
    }

    [Fact]
    public async Task Save_RejectsADuplicateNameAndStoresNothing()
    {
        await _service.SaveAsync(NewProfile());

        var errors = await _service.SaveAsync(NewProfile("example bank"));

        Assert.Single(errors);
        Assert.Equal(2, await _context.BankProfiles.CountAsync());
    }

    [Fact]
    public async Task Save_AllowsKeepingTheSameNameWhenEditing()
    {
        var generic = (await _service.ListAsync()).Single();
        generic.DateFormat = "yyyy/MM/dd";

        Assert.Empty(await _service.SaveAsync(generic));
    }

    [Fact]
    public async Task Delete_RemovesAnUnusedProfile()
    {
        await _service.SaveAsync(NewProfile());
        var profile = (await _service.ListAsync()).Single(p => p.Name == "Example Bank");

        Assert.True(await _service.DeleteAsync(profile.Id));
        Assert.DoesNotContain(await _service.ListAsync(), p => p.Name == "Example Bank");
    }

    [Fact]
    public async Task Delete_RefusesWhenStatementsWereImportedWithTheProfile()
    {
        var generic = (await _service.ListAsync()).Single();
        _context.StatementImports.Add(new StatementImport { FileName = "test.csv", BankProfileId = generic.Id });
        await _context.SaveChangesAsync();

        Assert.False(await _service.DeleteAsync(generic.Id));
        Assert.Single(await _service.ListAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
