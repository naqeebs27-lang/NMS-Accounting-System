using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AccountingSystem.App.Models;
using AccountingSystem.App.Services;

namespace AccountingSystem.App.ViewModels;

public class JournalEntryViewModel : ViewModelBase
{
    private readonly JournalService _journalService = new();
    private readonly AccountService _accountService = new();

    public ObservableCollection<JournalEntry> Entries { get; } = new();
    public ObservableCollection<Account> AvailableAccounts { get; } = new();
    public ObservableCollection<Account> FilterAccounts { get; } = new();

    private JournalEntry? _selectedEntry;
    public JournalEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetField(ref _selectedEntry, value))
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
        }
    }

    private DateTime _entryDate = DateTime.Today;
    public DateTime EntryDate { get => _entryDate; set => SetField(ref _entryDate, value); }

    private DateTime? _filterFrom;
    public DateTime? FilterFrom { get => _filterFrom; set => SetField(ref _filterFrom, value); }

    private DateTime? _filterTo;
    public DateTime? FilterTo { get => _filterTo; set => SetField(ref _filterTo, value); }

    private Account? _filterAccount;
    public Account? FilterAccount { get => _filterAccount; set => SetField(ref _filterAccount, value); }

    private int _voucherNo;
    public int VoucherNo { get => _voucherNo; set => SetField(ref _voucherNo, value); }

    private Account? _selectedAccount;
    public Account? SelectedAccount { get => _selectedAccount; set => SetField(ref _selectedAccount, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetField(ref _description, value); }

    private decimal _debit;
    public decimal Debit { get => _debit; set => SetField(ref _debit, value); }

    private decimal _credit;
    public decimal Credit { get => _credit; set => SetField(ref _credit, value); }

    private decimal _outOfBalance;
    public decimal OutOfBalance { get => _outOfBalance; set => SetField(ref _outOfBalance, value); }

    private decimal _totalDebit;
    public decimal TotalDebit { get => _totalDebit; set => SetField(ref _totalDebit, value); }

    private decimal _totalCredit;
    public decimal TotalCredit { get => _totalCredit; set => SetField(ref _totalCredit, value); }

    private decimal _closingBalance;
    public decimal ClosingBalance { get => _closingBalance; set => SetField(ref _closingBalance, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand NewVoucherCommand { get; }
    public RelayCommand ApplyFilterCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public JournalEntryViewModel()
    {
        AddCommand = new RelayCommand(AddEntry);
        DeleteCommand = new RelayCommand(DeleteEntry, () => SelectedEntry is not null);
        NewVoucherCommand = new RelayCommand(StartNewVoucher);
        ApplyFilterCommand = new RelayCommand(ApplyFilter);
        ClearFilterCommand = new RelayCommand(ClearFilter);

        // Recalculate totals whenever the visible entries collection changes
        Entries.CollectionChanged += (_, _) => RecalculateTotals();

        Reload();
        StartNewVoucher();
    }

    private void ApplyFilter()
    {
        Entries.Clear();
        var accountId = string.IsNullOrEmpty(FilterAccount?.AccountId) ? null : FilterAccount?.AccountId;
        foreach (var entry in _journalService.GetFilteredEntries(FilterFrom, FilterTo, accountId))
        {
            Entries.Add(entry);
        }
        RecalculateTotals();
    }

    private void ClearFilter()
    {
        FilterFrom = null;
        FilterTo = null;
        FilterAccount = FilterAccounts.FirstOrDefault();
        Reload();
    }

    public void Reload()
    {
        Entries.Clear();
        foreach (var entry in _journalService.GetAll())
        {
            Entries.Add(entry);
        }

        AvailableAccounts.Clear();
        foreach (var account in _accountService.GetAll())
        {
            AvailableAccounts.Add(account);
        }

        // Build filter accounts list with an "All Accounts" entry first
        FilterAccounts.Clear();
        FilterAccounts.Add(new Account { AccountId = string.Empty, Name = "All Accounts", Category = string.Empty, Type = string.Empty });
        foreach (var account in _accountService.GetAll())
        {
            FilterAccounts.Add(account);
        }
        FilterAccount = FilterAccounts.FirstOrDefault();

        OutOfBalance = _journalService.GetOutOfBalanceAmount();
        RecalculateTotals();
    }

    private void StartNewVoucher()
    {
        VoucherNo = _journalService.GetNextVoucherNo();
        EntryDate = DateTime.Today;
        SelectedAccount = null;
        Description = string.Empty;
        Debit = 0;
        Credit = 0;
    }

    private void AddEntry()
    {
        if (SelectedAccount is null)
        {
            MessageBox.Show("Please choose an account.", "Missing information",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Debit == 0 && Credit == 0)
        {
            MessageBox.Show("Enter either a Debit or a Credit amount (or both, for a compound line).",
                "Missing amount", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            MessageBox.Show("Please enter a description.", "Missing information",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _journalService.Add(new JournalEntry
        {
            EntryDate = EntryDate,
            AccountId = SelectedAccount.AccountId,
            VoucherNo = VoucherNo,
            Description = Description.Trim(),
            Debit = Debit,
            Credit = Credit
        });

        Reload();

        // Keep the same voucher number and date so the user can add the matching line(s).
        SelectedAccount = null;
        Description = string.Empty;
        Debit = 0;
        Credit = 0;
    }

    private void DeleteEntry()
    {
        if (SelectedEntry is null) return;

        var confirm = MessageBox.Show(
            $"Delete this line? ({SelectedEntry.AccountName}, {SelectedEntry.Debit:N2} Dr / {SelectedEntry.Credit:N2} Cr)",
            "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        _journalService.Delete(SelectedEntry.Id);
        Reload();
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalDebit = Entries.Sum(e => e.Debit);
        TotalCredit = Entries.Sum(e => e.Credit);
        ClosingBalance = TotalDebit - TotalCredit;
    }
}
