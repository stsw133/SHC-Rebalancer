global using StswExpress;
using System.IO;
using System.Windows;

namespace SHC_Rebalancer;
public partial class App : StswApp
{
    public readonly static string ConfigsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "rebalance");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        foreach (var name in Enum.GetNames(typeof(StrongholdType)))
            Directory.CreateDirectory($"{ConfigsPath}\\{name.ToLower()}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        Settings.Default.Save();
    }
}
