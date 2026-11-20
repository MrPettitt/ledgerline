namespace Ledgerline.Core.Detection;

public record MerchantCharges(int MerchantId, IReadOnlyList<Charge> Charges);
