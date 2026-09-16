using System.Windows;
using AccountingSystem.App.Data;

namespace AccountingSystem.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DatabaseInitializer.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not set up the local database.\n\n{ex.Message}",
                "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
