using Ledgerline.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

/// <summary>Remembers preferences between visits. It opens its own context so it is safe to call from the layout.</summary>
public class AppSettingsService(IDbContextFactory<LedgerlineDbContext> contextFactory)
{
    private const string ThemeKey = "theme";
    private const string Dark = "dark";
    private const string Light = "light";

    public async Task<bool> GetDarkModeAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var setting = await context.Settings.AsNoTracking().SingleOrDefaultAsync(s => s.Key == ThemeKey);
        return setting?.Value == Dark;
    }

    public async Task SetDarkModeAsync(bool dark)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var setting = await context.Settings.SingleOrDefaultAsync(s => s.Key == ThemeKey);

        if (setting is null)
        {
            context.Settings.Add(new AppSetting { Key = ThemeKey, Value = dark ? Dark : Light });
        }
        else
        {
            setting.Value = dark ? Dark : Light;
        }

        await context.SaveChangesAsync();
    }
}
