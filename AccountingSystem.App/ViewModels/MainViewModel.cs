namespace AccountingSystem.App.ViewModels;

/// <summary>
/// Root view model. Holds one view model per tab and refreshes the report
/// tabs whenever the Chart of Accounts or Journal Entries tab is left,
/// so the Ledger/Trial Balance/Income Statement/Balance Sheet always
/// reflect the latest postings — the same way the Excel pivot tables
/// needed a "Refresh" click after new rows were added.
/// </summary>
public class MainViewModel : ViewModelBase
{
    public ChartOfAccountsViewModel ChartOfAccounts { get; } = new();
    public JournalEntryViewModel JournalEntries { get; } = new();
    public LedgerViewModel Ledger { get; } = new();
    public TrialBalanceViewModel TrialBalance { get; } = new();
    public IncomeStatementViewModel IncomeStatement { get; } = new();
    public BalanceSheetViewModel BalanceSheet { get; } = new();
    public PettyCashViewModel PettyCash { get; } = new();

    // Navigation commands used by the dashboard view
    public RelayCommand NavigateToChartCommand { get; }
    public RelayCommand NavigateToJournalCommand { get; }
    public RelayCommand NavigateToLedgerCommand { get; }
    public RelayCommand NavigateToTrialBalanceCommand { get; }
    public RelayCommand NavigateToIncomeStatementCommand { get; }
    public RelayCommand NavigateToBalanceSheetCommand { get; }
    public RelayCommand NavigateToPettyCashCommand { get; }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetField(ref _selectedTabIndex, value))
            {
                RefreshReportsIfNeeded();
            }
        }
    }

    private void RefreshReportsIfNeeded()
    {
        // Tabs: 0 = Dashboard, 1 = Chart of Accounts, 2 = Journal, 3 = Ledger,
        //       4 = Trial Balance, 5 = Income Statement, 6 = Balance Sheet, 7 = Petty Cash
        switch (SelectedTabIndex)
        {
            case 4: Ledger.Refresh(); break;
            case 5: TrialBalance.Refresh(); break;
            case 6: IncomeStatement.Refresh(); break;
            case 7: BalanceSheet.Refresh(); break;
        }
    }

    public MainViewModel()
    {
        PettyCash.SetJournalEntriesViewModel(JournalEntries);
        NavigateToChartCommand = new RelayCommand(() => SelectedTabIndex = 1);
        NavigateToJournalCommand = new RelayCommand(() => SelectedTabIndex = 2);
        NavigateToPettyCashCommand = new RelayCommand(() => SelectedTabIndex = 7);
        NavigateToLedgerCommand = new RelayCommand(() => SelectedTabIndex = 4);
        NavigateToTrialBalanceCommand = new RelayCommand(() => SelectedTabIndex = 5);
        NavigateToIncomeStatementCommand = new RelayCommand(() => SelectedTabIndex = 6);
        NavigateToBalanceSheetCommand = new RelayCommand(() => SelectedTabIndex = 7);
    }
}
