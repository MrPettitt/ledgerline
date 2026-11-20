using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ledgerline.Data;

/// <summary>Lets the EF tools create the context without starting the web app.</summary>
public class LedgerlineDbContextFactory : IDesignTimeDbContextFactory<LedgerlineDbContext>
{
    public LedgerlineDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LedgerlineDbContext>()
            .UseSqlite("Data Source=ledgerline.db")
            .Options;

        return new LedgerlineDbContext(options);
    }
}
