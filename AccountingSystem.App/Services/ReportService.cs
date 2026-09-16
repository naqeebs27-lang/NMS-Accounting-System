using AccountingSystem.App.Models;

namespace AccountingSystem.App.Services;

/// <summary>
/// Builds the derived reports (Ledger, Trial Balance, Income Statement, Balance Sheet)
/// straight from Accounts + JournalEntries — the same way the original workbook's
/// pivot tables rolled the General Ledger sheet up into each report.
///
/// All reports accept an optional date range so the user can reproduce the
/// "From / To" filters the Excel dashboard used.
/// </summary>
public class ReportService
{
    private readonly AccountService _accountService = new();
    private readonly JournalService _journalService = new();

    private IEnumerable<JournalEntry> GetFilteredEntries(DateTime? from, DateTime? to)
    {
        var entries = _journalService.GetAll().AsEnumerable();
        if (from.HasValue) entries = entries.Where(e => e.EntryDate.Date >= from.Value.Date);
        if (to.HasValue) entries = entries.Where(e => e.EntryDate.Date <= to.Value.Date);
        return entries;
    }

    public List<LedgerRow> GetLedger(DateTime? from = null, DateTime? to = null)
    {
        return GetFilteredEntries(from, to)
            .GroupBy(e => new { e.AccountId, e.AccountName })
            .Select(g => new LedgerRow
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.AccountName,
                SumDebit = g.Sum(x => x.Debit),
                SumCredit = g.Sum(x => x.Credit)
            })
            .OrderBy(r => r.AccountName)
            .ToList();
    }

    public List<TrialBalanceRow> GetTrialBalance(DateTime? from = null, DateTime? to = null)
    {
        return GetFilteredEntries(from, to)
            .GroupBy(e => new { e.AccountId, e.AccountName, e.Category, e.Type })
            .Select(g => new TrialBalanceRow
            {
                AccountId = g.Key.AccountId,
                Type = g.Key.Type,
                AccountName = g.Key.AccountName,
                Category = g.Key.Category,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .OrderBy(r => r.AccountId)
            .ToList();
    }

    /// <summary>Expense/Income accounts only (Chart of Accounts Type == "Expense").</summary>
    public List<IncomeStatementRow> GetIncomeStatement(DateTime? from = null, DateTime? to = null)
    {
        return GetFilteredEntries(from, to)
            .Where(e => string.Equals(e.Type, "Expense", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(e.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .GroupBy(e => e.AccountName)
            .Select(g => new IncomeStatementRow
            {
                AccountName = g.Key,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .OrderBy(r => r.AccountName)
            .ToList();
    }

    /// <summary>Assets/Liability/Owner Equity accounts, grouped by Category.</summary>
    public List<BalanceSheetRow> GetBalanceSheet(DateTime? from = null, DateTime? to = null)
    {
        return GetFilteredEntries(from, to)
            .Where(e => string.Equals(e.Type, "Assets", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(e.Type, "Liability", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(e.Type, "Owner Equity", StringComparison.OrdinalIgnoreCase))
            .GroupBy(e => new { e.AccountId, e.AccountName, e.Category })
            .Select(g => new BalanceSheetRow
            {
                Category = g.Key.Category,
                AccountId = g.Key.AccountId,
                AccountName = g.Key.AccountName,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .OrderBy(r => r.Category).ThenBy(r => r.AccountId)
            .ToList();
    }
}
