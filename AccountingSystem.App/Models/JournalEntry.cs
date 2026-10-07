namespace AccountingSystem.App.Models;

/// <summary>
/// Represents a single debit or credit line in the General Ledger / Journal.
/// </summary>
public class JournalEntry
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public int VoucherNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    // Populated by joins for display purposes only (not persisted).
    public string AccountName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class PettyCashSummaryRow
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
