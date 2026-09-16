namespace AccountingSystem.App.Models;

/// <summary>
/// One account's rolled-up activity, used by the Ledger view.
/// </summary>
public class LedgerRow
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal SumDebit { get; set; }
    public decimal SumCredit { get; set; }
    public decimal Balance => SumDebit - SumCredit;
}

/// <summary>
/// One account's totals, used by the Trial Balance view.
/// </summary>
public class TrialBalanceRow
{
    public string AccountId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
}

/// <summary>
/// One expense/income account, used by the Income Statement view.
/// </summary>
public class IncomeStatementRow
{
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
}

/// <summary>
/// One asset/liability/equity account, used by the Balance Sheet view.
/// </summary>
public class BalanceSheetRow
{
    public string Category { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
}
