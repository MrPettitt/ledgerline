using Ledgerline.Core.Domain;
using Ledgerline.Core.Importing;
using Microsoft.EntityFrameworkCore;

namespace Ledgerline.Data;

public class BankProfileService(LedgerlineDbContext context, BankProfileValidator validator)
{
    public async Task<List<BankProfile>> ListAsync() =>
        await context.BankProfiles.AsNoTracking().OrderBy(p => p.Name).ToListAsync();

    /// <summary>Adds the profile when its Id is 0, otherwise updates it. Returns the problems found, empty on success.</summary>
    public async Task<IReadOnlyList<string>> SaveAsync(BankProfile profile)
    {
        var otherNames = await context.BankProfiles
            .Where(p => p.Id != profile.Id)
            .Select(p => p.Name)
            .ToListAsync();

        var errors = validator.Validate(profile, otherNames);
        if (errors.Count > 0)
        {
            return errors;
        }

        var target = profile.Id == 0
            ? context.BankProfiles.Add(new BankProfile()).Entity
            : await context.BankProfiles.SingleAsync(p => p.Id == profile.Id);

        target.Name = profile.Name.Trim();
        target.DateColumn = profile.DateColumn.Trim();
        target.DescriptionColumn = profile.DescriptionColumn.Trim();
        target.AmountColumn = profile.AmountColumn.Trim();
        target.DateFormat = profile.DateFormat.Trim();
        target.DebitsAreNegative = profile.DebitsAreNegative;

        await context.SaveChangesAsync();
        return [];
    }

    /// <summary>Returns false, and deletes nothing, when imported statements still use the profile.</summary>
    public async Task<bool> DeleteAsync(int profileId)
    {
        if (await context.StatementImports.AnyAsync(i => i.BankProfileId == profileId))
        {
            return false;
        }

        var profile = await context.BankProfiles.SingleAsync(p => p.Id == profileId);
        context.BankProfiles.Remove(profile);
        await context.SaveChangesAsync();
        return true;
    }
}
