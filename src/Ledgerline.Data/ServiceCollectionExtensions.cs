using Ledgerline.Core.Alerts;
using Ledgerline.Core.Details;
using Ledgerline.Core.Detection;
using Ledgerline.Core.Importing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ledgerline.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLedgerlineData(this IServiceCollection services, string connectionString)
    {
        // The factory lets the layout save a preference without sharing a context with the page being shown.
        services.AddDbContextFactory<LedgerlineDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped(provider => provider.GetRequiredService<IDbContextFactory<LedgerlineDbContext>>().CreateDbContext());

        return services
            .AddSingleton<StatementParser>()
            .AddSingleton<BankProfileValidator>()
            .AddSingleton<SubscriptionDetector>()
            .AddSingleton<AlertBuilder>()
            .AddSingleton<SubscriptionDetailBuilder>()
            .AddSingleton<AppSettingsService>()
            .AddScoped<BankProfileService>()
            .AddScoped<SubscriptionDetectionService>()
            .AddScoped<SubscriptionStatusService>()
            .AddScoped<SubscriptionDetailService>()
            .AddScoped<AlertService>()
            .AddScoped<StatementImporter>();
    }
}
