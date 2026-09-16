# Accounting System (WPF + SQLite)

A C# WPF desktop rewrite of the `Dashboard_Accounting_System` Excel workbook.
It covers the **core accounting workflow**: Chart of Accounts, Journal Entries
(General Ledger), and the three reports that were driven by pivot tables in
the spreadsheet — Ledger, Trial Balance, Income Statement, and Balance Sheet.

## What's included

| Excel sheet | WPF tab | Notes |
|---|---|---|
| Chart of Accounts | **Chart of Accounts** | Add / edit / delete accounts. An account can't be deleted once it has journal entries posted against it. |
| General Ledger | **Journal Entries** | Enter dated, double-entry lines (Debit/Credit) against any account. Shows a running "out of balance" total so you can see immediately if debits ≠ credits. |
| Ledger (pivot) | **Ledger** | Per-account Sum of Debit / Sum of Credit / Balance, recomputed live from the journal. |
| Trial Balance (pivot) | **Trial Balance** | All accounts with Debit/Credit/Balance totals and a Debit/Credit total row. |
| Income Statement (pivot) | **Income Statement** | Accounts whose Chart-of-Accounts *Type* is `Expense` or `Income`. |
| Balance Sheet (pivot) | **Balance Sheet** | Accounts whose Type is `Assets`, `Liability`, or `Owner Equity`, grouped by Category. |

**Not included in this version** (per your choice of "core only"): Petty Cash
ledger, Current Payroll, and Voucher sheets. The architecture (Model /
Data / Services / ViewModels / Views, one SQLite table per entity) is set up
so any of those can be added later as one more table + service + tab.

## Data import

Your workbook's **Chart of Accounts** (42 accounts) and **General Ledger**
(782 posted journal lines, from 1 Aug 2025 to 14 Sep 2026) were extracted and
converted into SQL seed scripts, embedded as resources in the app
(`Resources/ChartOfAccounts.seed.sql`, `Resources/JournalEntries.seed.sql`).
The first time the app runs, it creates a SQLite database at
`%LocalAppData%\AccountingSystem\accounting.db` and loads that seed data in.
After that first run, all your changes live in that file — the seed scripts
are only used to populate an empty database.

## Design decisions worth knowing about

- **Report numbers vs. the original spreadsheet:** Excel's pivot tables each
  had their own date-range filter (visible in "From / To" cells on each
  sheet), and those filters weren't all set to the same range — that's why,
  e.g., the Trial Balance, Income Statement, and Balance Sheet in your
  original file show slightly different totals for the same account (like
  "Cash"). This app instead computes every report **live from the same
  underlying journal**, with its own From/To filter on each report tab. With
  no filter applied it reports the full journal — treat that as the
  source of truth rather than the old pivot caches.
- **Balance formula:** every report uses `Balance = Debit − Credit`, exactly
  as your workbook's Trial Balance / Income Statement / Balance Sheet columns
  did.
- **Income Statement vs. Balance Sheet routing:** an account lands on the
  Income Statement if its Chart-of-Accounts *Type* is `Expense`/`Income`, and
  on the Balance Sheet if its Type is `Assets`/`Liability`/`Owner Equity`.
  Two accounts in your original Chart of Accounts have a Type that doesn't
  match their Category (`Opening Balance` is Type=Expense despite being
  Category=Current Assets; `Bike` is Type=Assets despite being Category=Other
  Expense) — they're carried over as-is from your data, so you may want to
  fix their Type on the **Chart of Accounts** tab if that was a typo.

## How to open and run it (Visual Studio Community)

1. Install **Visual Studio Community 2022** (free) with the **".NET desktop
   development"** workload checked (this gives you WPF + the .NET 8 SDK).
2. Double-click `AccountingSystem.sln` to open the solution.
3. Visual Studio will restore the one NuGet package (`Microsoft.Data.Sqlite`)
   automatically on first build.
4. Press **F5** (or the green ▶ "Start" button) to build and run.
5. On first launch the app creates and seeds the database automatically —
   no manual setup needed.

### Command line alternative

```bash
cd AccountingSystem
dotnet run --project AccountingSystem.App
```

CI: Build installer and code signing
-----------------------------------

This repository includes a GitHub Actions workflow that publishes the app and builds a Windows installer using Inno Setup. The workflow will optionally code-sign the published binaries and the installer when repository secrets are provided.

To enable signing in CI, add these repository secrets (Settings → Secrets and variables → Actions → New repository secret):

- SIGN_PFX — Base64-encoded contents of your .pfx signing certificate file.
  - Create the base64 string locally (PowerShell):
	$b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\path\to\cert.pfx'))
	Write-Host $b64
  - Copy the printed string and paste into the SIGN_PFX secret value.

- SIGN_PASSWORD — The password for the PFX file.

The workflow file is .github/workflows/ci-build-installer.yml and the local helper script is build-installer.ps1. The CI will only attempt to sign if SIGN_PFX is present; otherwise it will still publish and produce the installer unsigned.

Notes:
- The workflow runs on the Windows runner and installs Inno Setup via Chocolatey to build the installer.
- If you use an EV certificate backed by an HSM/token you must use a hosted signing service or sign artifacts in a secure environment and upload signed binaries to your release pipeline.
- Keep signing keys and passwords secret and rotate keys when needed.

(Requires the .NET 8 SDK and, since this is a WPF app, must be run on
Windows.)

## Project layout

```
AccountingSystem.sln
AccountingSystem.App/
  AccountingSystem.App.csproj
  App.xaml / App.xaml.cs            entry point, creates the DB on startup
  Models/                           Account, JournalEntry, report row DTOs
  Data/                             SQLite connection factory + schema/seed
  Services/                         AccountService, JournalService, ReportService
  ViewModels/                       one view model per tab (MVVM, no framework dependency)
  Views/                            MainWindow + one UserControl per tab
  Resources/                        embedded seed SQL generated from your workbook
```

## Extending it

- **Add Petty Cash / Payroll / Vouchers:** add a table in
  `DatabaseInitializer`, a model in `Models/`, a service in `Services/`, a
  view model in `ViewModels/`, and a `UserControl` + `TabItem` in
  `Views/MainWindow.xaml` — the same pattern used for Journal Entries.
- **Multi-user / networked use:** swap `Microsoft.Data.Sqlite` for SQL
  Server (LocalDB or a real server) by changing `DbConnectionFactory` and the
  ADO.NET provider; the `Services` layer's SQL is plain ANSI SQL and needs
  only minor changes.
- **Printing/export:** the report grids are plain `DataGrid`s — exporting to
  PDF/Excel/CSV can hang off the `RefreshCommand`'s `Rows` collection in each
  report view model.
