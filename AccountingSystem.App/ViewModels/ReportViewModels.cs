using System.Collections.ObjectModel;
using AccountingSystem.App.Models;
using AccountingSystem.App.Services;

namespace AccountingSystem.App.ViewModels;

/// <summary>Base class for the four report tabs: shared From/To filter + Refresh command.</summary>
public abstract class ReportViewModelBase : ViewModelBase
{
    protected readonly ReportService ReportService = new();

    private DateTime? _fromDate;
    public DateTime? FromDate { get => _fromDate; set => SetField(ref _fromDate, value); }

    private DateTime? _toDate;
    public DateTime? ToDate { get => _toDate; set => SetField(ref _toDate, value); }

    public RelayCommand RefreshCommand { get; }

    protected ReportViewModelBase()
    {
        RefreshCommand = new RelayCommand(Refresh);
    }

    public abstract void Refresh();
}

public class LedgerViewModel : ReportViewModelBase
{
    public ObservableCollection<LedgerRow> Rows { get; } = new();

    public LedgerViewModel() => Refresh();

    public override void Refresh()
    {
        Rows.Clear();
        foreach (var row in ReportService.GetLedger(FromDate, ToDate))
        {
            Rows.Add(row);
        }
    }
}

public class TrialBalanceViewModel : ReportViewModelBase
{
    public ObservableCollection<TrialBalanceRow> Rows { get; } = new();

    private decimal _totalDebit;
    public decimal TotalDebit { get => _totalDebit; set => SetField(ref _totalDebit, value); }

    private decimal _totalCredit;
    public decimal TotalCredit { get => _totalCredit; set => SetField(ref _totalCredit, value); }

    public TrialBalanceViewModel() => Refresh();

    public override void Refresh()
    {
        Rows.Clear();
        var rows = ReportService.GetTrialBalance(FromDate, ToDate);
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
        TotalDebit = rows.Sum(r => r.Debit);
        TotalCredit = rows.Sum(r => r.Credit);
    }
}

public class IncomeStatementViewModel : ReportViewModelBase
{
    public ObservableCollection<IncomeStatementRow> Rows { get; } = new();

    private decimal _totalExpense;
    public decimal TotalExpense { get => _totalExpense; set => SetField(ref _totalExpense, value); }

    public IncomeStatementViewModel() => Refresh();

    public override void Refresh()
    {
        Rows.Clear();
        var rows = ReportService.GetIncomeStatement(FromDate, ToDate);
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
        TotalExpense = rows.Sum(r => r.Balance);
    }
}

public class BalanceSheetViewModel : ReportViewModelBase
{
    public ObservableCollection<BalanceSheetRow> Rows { get; } = new();

    private decimal _totalBalance;
    public decimal TotalBalance { get => _totalBalance; set => SetField(ref _totalBalance, value); }

    public BalanceSheetViewModel() => Refresh();

    public override void Refresh()
    {
        Rows.Clear();
        var rows = ReportService.GetBalanceSheet(FromDate, ToDate);
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
        TotalBalance = rows.Sum(r => r.Balance);
    }
}
