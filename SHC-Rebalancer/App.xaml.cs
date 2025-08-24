global using StswExpress;
global using StswExpress.Commons;
using System.Windows;
using System.Windows.Threading;

namespace SHC_Rebalancer;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : StswApp
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Resources["Settings"] = SettingsService.Instance.Settings;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        SettingsService.Instance.SaveSettings();
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (StswFn.IsUiThreadAvailable())
            StswMessageDialog.Show(e.Exception, "Error");
        else
            StswLog.WriteException(e.Exception);
    }
}
