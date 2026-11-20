namespace Ledgerline.Data;

public record ImportResult(int RowsRead, int Added, int Duplicates);
