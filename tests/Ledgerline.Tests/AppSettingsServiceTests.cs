using Ledgerline.Data;

namespace Ledgerline.Tests;

public class AppSettingsServiceTests
{
    [Fact]
    public async Task GetDarkMode_IsLightUntilTheUserChoosesDark()
    {
        using var database = new TestDatabase();

        Assert.False(await new AppSettingsService(database).GetDarkModeAsync());
    }

    [Fact]
    public async Task SetDarkMode_IsRememberedByAnotherServiceInstance()
    {
        using var database = new TestDatabase();
        await new AppSettingsService(database).SetDarkModeAsync(true);

        Assert.True(await new AppSettingsService(database).GetDarkModeAsync());
    }

    [Fact]
    public async Task SetDarkMode_CanSwitchBackToLight()
    {
        using var database = new TestDatabase();
        var service = new AppSettingsService(database);
        await service.SetDarkModeAsync(true);

        await service.SetDarkModeAsync(false);

        Assert.False(await service.GetDarkModeAsync());
    }
}
