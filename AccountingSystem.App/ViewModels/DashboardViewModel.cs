using System.Collections.ObjectModel;
using AccountingSystem.App.Models;
using AccountingSystem.App.Services;

namespace AccountingSystem.App.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly JournalService _journalService = new();

    private DateTime? _fromDate;
    public DateTime? FromDate { get => _fromDate; set => SetField(ref _fromDate, value); }

    private DateTime? _toDate;
    public DateTime? ToDate { get => _toDate; set => SetField(ref _toDate, value); }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand GeneratePettyCommand { get; }
    public RelayCommand PostPettyCommand { get; }

    private decimal _bankBalance;
    public decimal BankBalance { get => _bankBalance; set => SetField(ref _bankBalance, value); }

    private decimal _cashBalance;
    public decimal CashBalance { get => _cashBalance; set => SetField(ref _cashBalance, value); }

    private decimal _pettyCashBalance;
    public decimal PettyCashBalance { get => _pettyCashBalance; set => SetField(ref _pettyCashBalance, value); }

    public ObservableCollection<LedgerRow> Preview { get; } = new();

    public DashboardViewModel()
    {
        RefreshCommand = new RelayCommand(Refresh);
        GeneratePettyCommand = new RelayCommand(GeneratePettyTotal);
        PostPettyCommand = new RelayCommand(PostPettySummary);
        // default to last 30 days
        ToDate = DateTime.Today;
        FromDate = DateTime.Today.AddDays(-30);
        Refresh();
    }

    private decimal _pettyMonthlyTotal;
    public decimal PettyMonthlyTotal { get => _pettyMonthlyTotal; set => SetField(ref _pettyMonthlyTotal, value); }

    private void GeneratePettyTotal()
    {
        // sum of credits recorded on the petty cash account (A002) within the date range
        var entries = _journalService.GetFilteredEntries(FromDate, ToDate, null);
        PettyMonthlyTotal = entries.Where(e => string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Credit);
    }

    private void PostPettySummary()
    {
        // Petty-cash entries are already posted as balanced vouchers when entered:
        // expense debit and A002 Petty Cash credit. Do not duplicate them here.
        Refresh();
        PettyMonthlyTotal = 0m;
    }

    public void Refresh()
    {
        var entries = _journalService.GetFilteredEntries(FromDate, ToDate, null);

        BankBalance = entries.Where(e => e.AccountName != null && e.AccountName.Contains("Bank", StringComparison.OrdinalIgnoreCase))
                              .Sum(e => e.Debit - e.Credit);

        CashBalance = entries.Where(e => string.Equals(e.AccountName, "Cash", StringComparison.OrdinalIgnoreCase))
                             .Sum(e => e.Debit - e.Credit);

        PettyCashBalance = entries.Where(e => e.AccountName != null && e.AccountName.IndexOf("Petty Cash", StringComparison.OrdinalIgnoreCase) >= 0)
                                 .Sum(e => e.Debit - e.Credit);

        Preview.Clear();
        // provide a small preview list of these accounts
        var previewRows = new[]
        {
            new LedgerRow { AccountId = "BANKS", AccountName = "Banks (summary)", SumDebit = entries.Where(e => e.AccountName != null && e.AccountName.Contains("Bank", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Debit), SumCredit = entries.Where(e => e.AccountName != null && e.AccountName.Contains("Bank", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Credit) },
            new LedgerRow { AccountId = "A001", AccountName = "Cash", SumDebit = entries.Where(e => string.Equals(e.AccountName, "Cash", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Debit), SumCredit = entries.Where(e => string.Equals(e.AccountName, "Cash", StringComparison.OrdinalIgnoreCase)).Sum(e => e.Credit) },
            new LedgerRow { AccountId = "A002", AccountName = "Petty Cash", SumDebit = entries.Where(e => e.AccountName != null && e.AccountName.IndexOf("Petty Cash", StringComparison.OrdinalIgnoreCase) >= 0).Sum(e => e.Debit), SumCredit = entries.Where(e => e.AccountName != null && e.AccountName.IndexOf("Petty Cash", StringComparison.OrdinalIgnoreCase) >= 0).Sum(e => e.Credit) }
        };
        foreach (var r in previewRows) Preview.Add(r);
    }
}
