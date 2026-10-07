using AccountingSystem.App.ViewModels;

namespace AccountingSystem.App.Views;

/// <summary>
/// Simple static locator so XAML in DashboardView can bind directly to the DashboardViewModel.
/// </summary>
public static class DashboardViewLocator
{
    private static readonly DashboardViewModel _vm = new();
    public static DashboardViewModel ViewModel => _vm;
}
