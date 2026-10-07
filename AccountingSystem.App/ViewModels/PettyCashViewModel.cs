using System;
using System.Collections.ObjectModel;
using System.Linq;
using AccountingSystem.App.Models;
using AccountingSystem.App.Services;

namespace AccountingSystem.App.ViewModels;

public class PettyCashViewModel : ViewModelBase
{
    private readonly JournalService _journalService = new();
    private readonly AccountService _accountService = new();
    private JournalEntryViewModel? _journalEntriesViewModel;

    public void SetJournalEntriesViewModel(JournalEntryViewModel journalEntriesViewModel)
    {
        _journalEntriesViewModel = journalEntriesViewModel;
    }

    private DateTime? _fromDate;
    public DateTime? FromDate { get => _fromDate; set => SetField(ref _fromDate, value); }

    private DateTime? _toDate;
    public DateTime? ToDate { get => _toDate; set => SetField(ref _toDate, value); }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand GeneratePettyCommand { get; }
    public RelayCommand PostPettyCommand { get; }
    public RelayCommand AddEntryCommand { get; }
    public RelayCommand UpdateEntryCommand { get; }
    public RelayCommand DeleteEntryCommand { get; }

    public ObservableCollection<JournalEntry> Entries { get; } = new();
    public ObservableCollection<JournalEntry> ExpenseEntries { get; } = new();
    public ObservableCollection<Account> ExpenseAccounts { get; } = new();
    public ObservableCollection<string> EntryTypes { get; } = new() { "Receipt", "Payment" };
    public ObservableCollection<PettyCashSummaryRow> MonthlySummary { get; } = new();

    private string? _selectedExpenseAccountId;
    public string? SelectedExpenseAccountId { get => _selectedExpenseAccountId; set => SetField(ref _selectedExpenseAccountId, value); }

    private decimal _totalDebit;
    public decimal TotalDebit { get => _totalDebit; set => SetField(ref _totalDebit, value); }

    private decimal _totalCredit;
    public decimal TotalCredit { get => _totalCredit; set => SetField(ref _totalCredit, value); }

    private decimal _closingBalance;
    public decimal ClosingBalance { get => _closingBalance; set => SetField(ref _closingBalance, value); }

    // New entry fields
    private DateTime _newEntryDate = DateTime.Today;
    public DateTime NewEntryDate { get => _newEntryDate; set => SetField(ref _newEntryDate, value); }

    private string _newDescription = string.Empty;
    public string NewDescription { get => _newDescription; set => SetField(ref _newDescription, value); }

    private decimal _newDebit;
    public decimal NewDebit { get => _newDebit; set => SetField(ref _newDebit, value); }

    private decimal _newCredit;
    public decimal NewCredit { get => _newCredit; set => SetField(ref _newCredit, value); }

    private string _entryType = "Payment";
    public string EntryType { get => _entryType; set => SetField(ref _entryType, value); }

    private decimal _receiptAmount;
    public decimal ReceiptAmount { get => _receiptAmount; set => SetField(ref _receiptAmount, value); }

    private decimal _paymentAmount;
    public decimal PaymentAmount { get => _paymentAmount; set => SetField(ref _paymentAmount, value); }

    private decimal _pettyMonthlyTotal;
    public decimal PettyMonthlyTotal { get => _pettyMonthlyTotal; set => SetField(ref _pettyMonthlyTotal, value); }

    private JournalEntry? _selectedEntry;
    public JournalEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetField(ref _selectedEntry, value))
            {
                if (_selectedEntry is not null)
                {
                    NewEntryDate = _selectedEntry.EntryDate;
                    NewDescription = _selectedEntry.Description;
                    var lines = _journalService.GetAll()
                        .Where(e => e.VoucherNo == _selectedEntry.VoucherNo)
                        .ToList();
                    var pettyLine = lines.FirstOrDefault(e => string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase));
                    var pair = lines.FirstOrDefault(e => !string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase));
                    var isReceipt = pettyLine?.Debit > 0m;
                    EntryType = isReceipt == true ? "Receipt" : "Payment";
                    ReceiptAmount = isReceipt == true ? pettyLine!.Debit : 0m;
                    PaymentAmount = isReceipt == true ? 0m : pettyLine?.Credit ?? 0m;
                    NewDebit = pair?.Debit ?? 0m;
                    NewCredit = pair?.Credit ?? 0m;
                    // Select the non-petty account used by the paired journal line.
                SelectedExpenseAccountId = pair?.AccountId;
                }
            }
        }
    }

    public PettyCashViewModel()
    {
        RefreshCommand = new RelayCommand(Refresh);
        GeneratePettyCommand = new RelayCommand(GeneratePettyTotal);
        PostPettyCommand = new RelayCommand(PostPettySummary);
        AddEntryCommand = new RelayCommand(AddEntry);
        UpdateEntryCommand = new RelayCommand(UpdateEntry);
        DeleteEntryCommand = new RelayCommand(DeleteEntry);

        ToDate = DateTime.Today;
        FromDate = DateTime.Today.AddDays(-30);

        // load expense accounts for selection
        var accounts = _accountService.GetAll().Where(a => string.Equals(a.Type, "Expense", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var a in accounts) ExpenseAccounts.Add(a);
        if (ExpenseAccounts.Count > 0 && string.IsNullOrEmpty(SelectedExpenseAccountId))
        {
            SelectedExpenseAccountId = ExpenseAccounts[0].AccountId;
        }

        Refresh();
    }

    public void Refresh()
    {
        Entries.Clear();
        ExpenseEntries.Clear();
        var entries = _journalService.GetFilteredEntries(FromDate, ToDate, "A002")
            .Where(e => !string.Equals(e.Description, "To Petty Cash", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var e in entries)
            Entries.Add(e);

        var pettyVouchers = entries.Select(e => e.VoucherNo).Distinct().ToHashSet();
        var expenseEntries = _journalService.GetFilteredEntries(FromDate, ToDate, null)
            .Where(e => pettyVouchers.Contains(e.VoucherNo)
                        && !string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase));
        foreach (var e in expenseEntries)
            ExpenseEntries.Add(e);

        // compute totals and closing balance
        TotalDebit = Entries.Sum(x => x.Debit);
        TotalCredit = Entries.Sum(x => x.Credit);
        ClosingBalance = TotalDebit - TotalCredit;
    }

    private void GeneratePettyTotal()
    {
        BuildMonthlySummary();
    }

    private void BuildMonthlySummary()
    {
        MonthlySummary.Clear();

        var all = _journalService.GetAll();
        var pettyVouchers = all.Where(e => string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase)
                                          && e.Credit > 0m
                                          && !string.Equals(e.Description, "To Petty Cash", StringComparison.OrdinalIgnoreCase)
                                          && (!FromDate.HasValue || e.EntryDate.Date >= FromDate.Value.Date)
                                          && (!ToDate.HasValue || e.EntryDate.Date <= ToDate.Value.Date))
                                .Select(e => e.VoucherNo)
                                .ToHashSet();

        var expenseLines = all.Where(e => pettyVouchers.Contains(e.VoucherNo)
                                          && !string.Equals(e.AccountId, "A002", StringComparison.OrdinalIgnoreCase)
                                          && e.Debit > 0m);

        foreach (var group in expenseLines.GroupBy(e => new { e.AccountId, e.AccountName }))
        {
            MonthlySummary.Add(new PettyCashSummaryRow
            {
                AccountId = group.Key.AccountId,
                AccountName = group.Key.AccountName,
                Amount = group.Sum(e => e.Debit)
            });
        }

        PettyMonthlyTotal = MonthlySummary.Sum(x => x.Amount);
    }

    private void PostPettySummary()
    {
        BuildMonthlySummary();
        if (MonthlySummary.Count == 0) return;

        var entryDate = ToDate ?? DateTime.Today;
        var description = $"Petty Cash cumulative expenses {entryDate:yyyy-MM}";
        var alreadyPosted = _journalService.GetAll().Any(e => e.AccountId == "A002"
                                                             && e.Credit == PettyMonthlyTotal
                                                             && e.EntryDate.Date == entryDate.Date
                                                             && string.Equals(e.Description, "To Petty Cash", StringComparison.OrdinalIgnoreCase));
        if (alreadyPosted) return;

        var voucher = _journalService.GetNextVoucherNo();
        foreach (var summary in MonthlySummary)
        {
            _journalService.Add(new JournalEntry
            {
                EntryDate = entryDate,
                AccountId = summary.AccountId,
                VoucherNo = voucher,
                Description = summary.AccountName,
                Debit = summary.Amount,
                Credit = 0m
            });
        }

        _journalService.Add(new JournalEntry
        {
            EntryDate = entryDate,
            AccountId = "A002",
            VoucherNo = voucher,
            Description = "To Petty Cash",
            Debit = 0m,
            Credit = PettyMonthlyTotal
        });

        _journalEntriesViewModel?.Reload();
        Refresh();
    }

    private void AddEntry()
    {
        // create two journal lines: debit selected expense account, credit petty cash (A002)
        var expenseAccount = SelectedExpenseAccountId ?? "E025"; // fallback
        var voucher = _journalService.GetNextVoucherNo();
        var desc = NewDescription;
        var amount = EntryType == "Receipt" ? ReceiptAmount : PaymentAmount;
        if (amount <= 0m) return;

        var expenseLine = new JournalEntry
        {
            EntryDate = NewEntryDate,
            AccountId = expenseAccount,
            VoucherNo = voucher,
            Description = desc,
            Debit = EntryType == "Payment" ? amount : 0m,
            Credit = EntryType == "Receipt" ? amount : 0m
        };

        var pettyLine = new JournalEntry
        {
            EntryDate = NewEntryDate,
            AccountId = "A002",
            VoucherNo = voucher,
            Description = desc,
            Debit = EntryType == "Receipt" ? amount : 0m,
            Credit = EntryType == "Payment" ? amount : 0m
        };

        _journalService.Add(expenseLine);
        _journalService.Add(pettyLine);

        // clear input
        NewDescription = string.Empty;
        NewDebit = 0m;
        NewCredit = 0m;
        ReceiptAmount = 0m;
        PaymentAmount = 0m;

        Refresh();
    }

    private void UpdateEntry()
    {
        if (SelectedEntry is null) return;

        // update all lines that share the same voucher number
        var all = _journalService.GetAll();
        var lines = all.Where(x => x.VoucherNo == SelectedEntry.VoucherNo).ToList();
        if (!lines.Any()) return;

        var amount = EntryType == "Receipt" ? ReceiptAmount : PaymentAmount;
        if (amount <= 0m) return;

        foreach (var line in lines)
        {
            if (string.Equals(line.AccountId, "A002", StringComparison.OrdinalIgnoreCase))
            {
                line.EntryDate = NewEntryDate;
                line.Description = NewDescription;
                line.Debit = EntryType == "Receipt" ? amount : 0m;
                line.Credit = EntryType == "Payment" ? amount : 0m;
            }
            else
            {
                line.EntryDate = NewEntryDate;
                line.Description = NewDescription;
                line.Debit = EntryType == "Payment" ? amount : 0m;
                line.Credit = EntryType == "Receipt" ? amount : 0m;
                if (!string.IsNullOrEmpty(SelectedExpenseAccountId)) line.AccountId = SelectedExpenseAccountId;
            }
            _journalService.Update(line);
        }

        SelectedEntry = null;
        NewDescription = string.Empty;
        NewDebit = 0m;
        NewCredit = 0m;
        ReceiptAmount = 0m;
        PaymentAmount = 0m;
        Refresh();
    }

    private void DeleteEntry()
    {
        if (SelectedEntry is null) return;
        var all = _journalService.GetAll();
        var lines = all.Where(x => x.VoucherNo == SelectedEntry.VoucherNo).ToList();
        foreach (var l in lines)
            _journalService.Delete(l.Id);

        SelectedEntry = null;
        NewDescription = string.Empty;
        NewDebit = 0m;
        NewCredit = 0m;
        Refresh();
    }
}
