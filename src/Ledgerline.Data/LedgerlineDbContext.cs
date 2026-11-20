using Ledgerline.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class LedgerlineDbContext(DbContextOptions<LedgerlineDbContext> options) : DbContext(options)
{
    public DbSet<BankProfile> BankProfiles => Set<BankProfile>();
    public DbSet<StatementImport> StatementImports => Set<StatementImport>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionEvent> SubscriptionEvents => Set<SubscriptionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankProfile>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<BankProfile>().HasData(new BankProfile
        {
            Id = 1,
            Name = "Generic CSV",
            DateColumn = "Date",
            DescriptionColumn = "Description",
            AmountColumn = "Amount",
            DateFormat = "yyyy-MM-dd",
            DebitsAreNegative = true
        });

        modelBuilder.Entity<Merchant>().HasIndex(m => m.Key).IsUnique();

        modelBuilder.Entity<Transaction>().HasIndex(t => t.Fingerprint).IsUnique();

        modelBuilder.Entity<Subscription>()
            .HasIndex(s => s.MerchantId)
            .IsUnique();

        modelBuilder.Entity<Subscription>().Property(s => s.Status).HasConversion<string>();
        modelBuilder.Entity<Subscription>().Property(s => s.Cadence).HasConversion<string>();
        modelBuilder.Entity<SubscriptionEvent>().Property(e => e.Type).HasConversion<string>();
    }
}
