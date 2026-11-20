using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ledgerline.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLedgerlineData(this IServiceCollection services, string connectionString)
    {
        return services.AddDbContext<LedgerlineDbContext>(options => options.UseSqlite(connectionString));
    }
}
