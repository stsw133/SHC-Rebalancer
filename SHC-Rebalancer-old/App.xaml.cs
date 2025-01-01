global using StswExpress;
using System.IO;
using System.Windows;

namespace SHC_Rebalancer_old;
public partial class App : StswApp
{
    public readonly static string ConfigsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "rebalance");
    public static string GetConfigPath(StrongholdType type, string configName) => $"{ConfigsPath}\\{type}\\{configName}.json";

    /// OnStartup
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        foreach (var name in Enum.GetNames(typeof(StrongholdType)))
            Directory.CreateDirectory($"{ConfigsPath}\\{name.ToLower()}");
    }

    /// OnExit
    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        Settings.Default.Save();
    }
}
