# Security

Ledgerline is a local app. It has no accounts, no login and no server component, and it makes no outbound network requests.

## What the app does to protect your data

- It listens on `localhost` only, and rejects requests that arrive with any other host name (this blocks DNS rebinding from a web page).
- Your statements and results live in one SQLite file, `ledgerline.db`, next to the app. Nothing is uploaded.
- Responses carry `X-Content-Type-Options`, `X-Frame-Options` and `Referrer-Policy` headers.
- Uploads are limited to 5 MB and parsed as CSV only. Values from a file are shown through Blazor, which encodes them, and stored through EF Core, which parameterises every query.
- SQL statements and values are not written to the log.

## What it does not do

- It does not encrypt `ledgerline.db`. Anyone who can read your files can read your data. Use full-disk encryption if that matters.
- It has no login. Do not expose it beyond your own computer, for example by binding it to `0.0.0.0` or forwarding the port.

## Reporting a problem

Please use the repository's private vulnerability reporting (Security tab, then "Report a vulnerability") rather than a public issue. Do not attach real bank statements to any report.
