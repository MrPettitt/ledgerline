namespace Ledgerline.Core.Alerts;

public static class AlertFilter
{
    public static AlertList Split(IEnumerable<Alert> alerts, IReadOnlySet<AlertKey> dismissed)
    {
        var all = alerts.ToList();
        return new AlertList(
            all.Where(a => !dismissed.Contains(a.Key)).ToList(),
            all.Where(a => dismissed.Contains(a.Key)).ToList());
    }
}
