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

## Releases

Each release is a self-contained Windows build that needs no .NET install. Download the zip from the Releases page, unzip it and run `Ledgerline.Web.exe`, then open http://localhost:5000. The database file `ledgerline.db` is created next to the exe and stays on your machine.

To publish a release, push a version tag:

```
git tag v0.2.0
git push origin v0.2.0
```

The release workflow runs the tests, builds the zip and creates the release with generated notes.
