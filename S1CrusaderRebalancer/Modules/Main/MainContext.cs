using System.IO;

namespace S1CrusaderRebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand InstallCommand { get; }
    public StswAsyncCommand<StrongholdType> EditConfigCommand { get; }

    public MainContext()
    {
        InstallCommand = new(Install);
        EditConfigCommand = new(EditConfig);

        var configs = new Dictionary<StrongholdType, IEnumerable<string>>();
        foreach (StrongholdType value in Enum.GetValues(typeof(StrongholdType)))
            configs.Add(value, Directory.GetFiles($"{App.ConfigsPath}\\{value.ToString().ToLower()}", "*.json").Select(x => Path.GetFileNameWithoutExtension(x)));
        Configs = configs;
    }

    /// Install
    public async Task Install()
    {
        try
        {
            Rebalancer.Rebalance(Settings.Default.StrongholdPath, $"{App.ConfigsPath}\\{StrongholdType.Stronghold}\\{Settings.Default.StrongholdConfig}.json");
            Rebalancer.Rebalance(Settings.Default.CrusaderPath, $"{App.ConfigsPath}\\{StrongholdType.Crusader}\\{Settings.Default.CrusaderConfig}.json");
            Rebalancer.Rebalance(Settings.Default.ExtremePath, $"{App.ConfigsPath}\\{StrongholdType.Extreme}\\{Settings.Default.ExtremeConfig}.json");
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// EditConfig
    private async Task EditConfig(StrongholdType type)
    {
        try
        {
            await Task.Run(() =>
            {
                var configPath = type switch
                {
                    StrongholdType.Stronghold => $"{App.ConfigsPath}\\{type}\\{Settings.Default.StrongholdConfig}.json",
                    StrongholdType.Crusader => $"{App.ConfigsPath}\\{type}\\{Settings.Default.CrusaderConfig}.json",
                    StrongholdType.Extreme => $"{App.ConfigsPath}\\{type}\\{Settings.Default.ExtremeConfig}.json",
                    _ => throw new NotImplementedException()
                };
                if (File.Exists(configPath))
                    StswFn.OpenFile(configPath);
            });
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// Configs
    public Dictionary<StrongholdType, IEnumerable<string>> Configs
    {
        get => _configs;
        set => SetProperty(ref _configs, value);
    }
    private Dictionary<StrongholdType, IEnumerable<string>> _configs = [];
}
