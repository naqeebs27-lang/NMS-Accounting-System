using System.Collections.ObjectModel;
using System.Windows;
using AccountingSystem.App.Models;
using AccountingSystem.App.Services;

namespace AccountingSystem.App.ViewModels;

public class ChartOfAccountsViewModel : ViewModelBase
{
    private readonly AccountService _accountService = new();

    public ObservableCollection<Account> Accounts { get; } = new();

    public static readonly string[] TypeOptions =
        { "Assets", "Liability", "Owner Equity", "Expense", "Income" };

    private Account? _selectedAccount;
    public Account? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (SetField(ref _selectedAccount, value))
            {
                LoadSelectedIntoEditor();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }
    }

    // Editor fields (used for both Add and Update)
    private string _accountId = string.Empty;
    public string EditAccountId { get => _accountId; set => SetField(ref _accountId, value); }

    private string _name = string.Empty;
    public string EditName { get => _name; set => SetField(ref _name, value); }

    private string _category = string.Empty;
    public string EditCategory { get => _category; set => SetField(ref _category, value); }

    private string _type = "Expense";
    public string EditType { get => _type; set => SetField(ref _type, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ClearCommand { get; }

    public ChartOfAccountsViewModel()
    {
        AddCommand = new RelayCommand(AddAccount);
        UpdateCommand = new RelayCommand(UpdateAccount, () => SelectedAccount is not null);
        DeleteCommand = new RelayCommand(DeleteAccount, () => SelectedAccount is not null);
        ClearCommand = new RelayCommand(ClearEditor);

        Reload();
    }

    public void Reload()
    {
        Accounts.Clear();
        foreach (var account in _accountService.GetAll())
        {
            Accounts.Add(account);
        }
    }

    private void LoadSelectedIntoEditor()
    {
        if (SelectedAccount is null)
        {
            ClearEditor();
            return;
        }

        EditAccountId = SelectedAccount.AccountId;
        EditName = SelectedAccount.Name;
        EditCategory = SelectedAccount.Category;
        EditType = SelectedAccount.Type;
    }

    private void ClearEditor()
    {
        SelectedAccount = null;
        EditAccountId = string.Empty;
        EditName = string.Empty;
        EditCategory = string.Empty;
        EditType = "Expense";
    }

    private bool ValidateEditor()
    {
        if (string.IsNullOrWhiteSpace(EditAccountId) || string.IsNullOrWhiteSpace(EditName)
            || string.IsNullOrWhiteSpace(EditCategory) || string.IsNullOrWhiteSpace(EditType))
        {
            MessageBox.Show("Account ID, Name, Category and Type are all required.",
                "Missing information", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void AddAccount()
    {
        if (!ValidateEditor()) return;

        _accountService.Add(new Account
        {
            AccountId = EditAccountId.Trim(),
            Name = EditName.Trim(),
            Category = EditCategory.Trim(),
            Type = EditType.Trim()
        });
        Reload();
        ClearEditor();
    }

    private void UpdateAccount()
    {
        if (SelectedAccount is null || !ValidateEditor()) return;

        _accountService.Update(new Account
        {
            Id = SelectedAccount.Id,
            AccountId = EditAccountId.Trim(),
            Name = EditName.Trim(),
            Category = EditCategory.Trim(),
            Type = EditType.Trim()
        });
        Reload();
        ClearEditor();
    }

    private void DeleteAccount()
    {
        if (SelectedAccount is null) return;

        if (_accountService.IsInUse(SelectedAccount.AccountId))
        {
            MessageBox.Show(
                $"'{SelectedAccount.Name}' has journal entries posted against it and cannot be deleted.",
                "Account in use", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Delete account '{SelectedAccount.Name}'?", "Confirm delete",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        _accountService.Delete(SelectedAccount.Id);
        Reload();
        ClearEditor();
    }
}
