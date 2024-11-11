using System.IO;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand ConfigFolderOpenCommand { get; }
    public StswAsyncCommand<StrongholdType> EditConfigCommand { get; }

    public MainContext()
    {
        InstallCommand = new(Install);
        ConfigFolderOpenCommand = new(ConfigFolderOpen);
        EditConfigCommand = new(EditConfig);

        Configs = LoadConfigs();
    }

    /// Install
    public async Task Install(CancellationToken token)
    {
        try
        {
            InstallState = StswProgressState.Running;

            Rebalancer.Rebalance(Settings.Default.StrongholdPath, $"{App.ConfigsPath}\\{StrongholdType.Stronghold}\\{Settings.Default.StrongholdConfig}.json");
            Rebalancer.Rebalance(Settings.Default.CrusaderPath, $"{App.ConfigsPath}\\{StrongholdType.Crusader}\\{Settings.Default.CrusaderConfig}.json");
            Rebalancer.Rebalance(Settings.Default.ExtremePath, $"{App.ConfigsPath}\\{StrongholdType.Extreme}\\{Settings.Default.ExtremeConfig}.json");

            InstallState = StswProgressState.Finished;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// ConfigFolderOpen
    public async Task ConfigFolderOpen()
    {
        try
        {
            StswFn.OpenFile(App.ConfigsPath);
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
            var configPath = GetConfigPath(type, type switch
            {
                StrongholdType.Stronghold => Settings.Default.StrongholdConfig,
                StrongholdType.Crusader => Settings.Default.CrusaderConfig,
                StrongholdType.Extreme => Settings.Default.ExtremeConfig,
                _ => throw new NotImplementedException()
            });

            if (File.Exists(configPath))
                StswFn.OpenFile(configPath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// GetConfigPath
    private string GetConfigPath(StrongholdType type, string configName) => $"{App.ConfigsPath}\\{type}\\{configName}.json";

    /// LoadConfigs
    private Dictionary<StrongholdType, IEnumerable<string>> LoadConfigs()
    {
        var configs = new Dictionary<StrongholdType, IEnumerable<string>>();
        foreach (StrongholdType value in Enum.GetValues(typeof(StrongholdType)))
        {
            var path = $"{App.ConfigsPath}\\{value.ToString().ToLower()}";
            configs[value] = Directory.GetFiles(path, "*.json").Select(Path.GetFileNameWithoutExtension)!;
        }
        return configs;
    }

    /// Configs
    public Dictionary<StrongholdType, IEnumerable<string>> Configs
    {
        get => _configs;
        set => SetProperty(ref _configs, value);
    }
    private Dictionary<StrongholdType, IEnumerable<string>> _configs = [];

    /// InstallState
    public StswProgressState InstallState
    {
        get => _installState;
        set => SetProperty(ref _installState, value);
    }
    private StswProgressState _installState;
}
