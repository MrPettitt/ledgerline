namespace Ledgerline.Core.Alerts;

public record AlertList(IReadOnlyList<Alert> Active, IReadOnlyList<Alert> Dismissed);
