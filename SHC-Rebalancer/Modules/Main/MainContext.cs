using System.Collections.ObjectModel;
using System.IO;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }
    public StswAsyncCommand ConfigFolderOpenCommand { get; }
    public StswAsyncCommand<StrongholdType> EditConfigCommand { get; }
    public StswAsyncCommand<StrongholdType> FindCommand { get; }

    public MainContext()
    {
        InstallCommand = new(Install, () => InstallState != StswProgressState.Running);
        UninstallCommand = new(Uninstall, () => InstallState != StswProgressState.Running);
        ConfigFolderOpenCommand = new(ConfigFolderOpen);
        EditConfigCommand = new(EditConfig);
        FindCommand = new(Find);

        Configs = LoadConfigs();
    }
    


    /// Install
    public async Task Install(CancellationToken token)
    {
        try
        {
            InstallState = StswProgressState.Running;

            Backup.Make(Settings.Default.StrongholdPath);
            Rebalancer.Rebalance(Settings.Default.StrongholdPath, $"{App.ConfigsPath}\\{StrongholdType.Stronghold}\\{Settings.Default.StrongholdConfig}.json");

            Backup.Make(Settings.Default.CrusaderPath);
            Rebalancer.Rebalance(Settings.Default.CrusaderPath, $"{App.ConfigsPath}\\{StrongholdType.Crusader}\\{Settings.Default.CrusaderConfig}.json");

            Backup.Make(Settings.Default.ExtremePath);
            Rebalancer.Rebalance(Settings.Default.ExtremePath, $"{App.ConfigsPath}\\{StrongholdType.Extreme}\\{Settings.Default.ExtremeConfig}.json");

            InstallState = StswProgressState.Finished;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// Uninstall
    public async Task Uninstall()
    {
        try
        {
            InstallState = StswProgressState.Running;

            Backup.Restore(Settings.Default.StrongholdPath);
            Backup.Restore(Settings.Default.CrusaderPath);
            Backup.Restore(Settings.Default.ExtremePath);

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
            var configPath = App.GetConfigPath(type, type switch
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



    /// Find
    public async Task Find(StrongholdType type)
    {
        try
        {
            Finder.Find(FinderResults, type, FinderFilterSize, FinderFilterAddress, FinderFilterSkips ?? 0, FinderFilterValues);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, "Error", true);
        }
    }

    /// FinderFilterType
    public StrongholdType? FinderFilterType
    {
        get => _finderFilterType;
        set => SetProperty(ref _finderFilterType, value);
    }
    private StrongholdType? _finderFilterType;
    
    /// FinderFilterSize
    public int FinderFilterSize
    {
        get => _finderFilterSize;
        set => SetProperty(ref _finderFilterSize, value);
    }
    private int _finderFilterSize = 1;

    /// FinderFilterAddress
    public string? FinderFilterAddress
    {
        get => _finderFilterAddress;
        set
        {
            SetProperty(ref _finderFilterAddress, value);
            FindCommand.Execute(FinderFilterType);
        }
    }
    private string? _finderFilterAddress;
    
    /// FinderFilterSkips
    public int? FinderFilterSkips
    {
        get => _finderFilterSkips;
        set
        {
            SetProperty(ref _finderFilterSkips, value);
            FindCommand.Execute(FinderFilterType);
        }
    }
    private int? _finderFilterSkips;

    /// FinderFilterValues
    public string? FinderFilterValues
    {
        get => _finderFilterValues;
        set
        {
            SetProperty(ref _finderFilterValues, value);
            FindCommand.Execute(FinderFilterType);
        }
    }
    private string? _finderFilterValues;

    /// FinderResults
    public ObservableCollection<FinderDataModel> FinderResults
    {
        get => _finderResults;
        set => SetProperty(ref _finderResults, value);
    }
    private ObservableCollection<FinderDataModel> _finderResults = [];
}
