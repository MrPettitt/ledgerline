# Ledgerline

A local subscription tracker. It imports bank statement CSVs, finds recurring debits, flags price increases and warns before renewals.

Everything runs on your machine. Data lives in a SQLite file that is never committed.

## Run

```
dotnet run --project src/Ledgerline.Web
```

## Test

```
dotnet test
```

## Layout

- `Ledgerline.Core`: domain entities and logic, no UI or database references
- `Ledgerline.Data`: EF Core context and migrations (SQLite)
- `Ledgerline.Web`: Blazor Web App (interactive server) with MudBlazor
- `Ledgerline.Tests`: xUnit tests

Sample data in `samples` must use invented merchants only.
