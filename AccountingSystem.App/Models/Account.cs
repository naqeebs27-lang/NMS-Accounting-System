namespace AccountingSystem.App.Models;

/// <summary>
/// Represents a single row in the Chart of Accounts.
/// </summary>
public class Account
{
    public int Id { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Assets, Liability, Owner Equity, Expense

    public override string ToString() => $"{AccountId} - {Name}";
}
