using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand SaveChangesCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }

    public MainContext()
    {
        SaveChangesCommand = new(SaveChanges);
        InstallCommand = new(Install, () => InstallState != StswProgressState.Running);
        UninstallCommand = new(Uninstall, () => InstallState != StswProgressState.Running);
    }



    /// SaveChanges
    public async Task SaveChanges()
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(1000);
            Storage.SaveConfigs();

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// Install
    public async Task Install(CancellationToken token)
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(200);
            Storage.SaveConfigs();

            InstallValue += 20;
            await Task.Delay(400);

            Backup.Make(Settings.Default.CrusaderPath);
            Rebalancer.Rebalance(GameVersion.Crusader, Settings.Default.CrusaderPath);

            InstallValue += 40;
            await Task.Delay(400);

            Backup.Make(Settings.Default.ExtremePath);
            Rebalancer.Rebalance(GameVersion.Extreme, Settings.Default.ExtremePath);

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// Uninstall
    public async Task Uninstall()
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(500);

            Backup.Restore(Settings.Default.CrusaderPath);

            InstallValue += 50;
            await Task.Delay(500);

            Backup.Restore(Settings.Default.ExtremePath);

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// Configs
    public ObservableCollection<AicConfigModel> Configs_aic => new(Storage.Configs.ContainsKey("aic") == true ? Storage.Configs["aic"].Cast<AicConfigModel>() : []);
    public ObservableCollection<BuildingsConfigModel> Configs_buildings => new(Storage.Configs.ContainsKey("buildings") == true ? Storage.Configs["buildings"].Cast<BuildingsConfigModel>() : []);
    public ObservableCollection<ResourcesConfigModel> Configs_resources => new(Storage.Configs.ContainsKey("resources") == true ? Storage.Configs["resources"].Cast<ResourcesConfigModel>() : []);
    public ObservableCollection<SkirmishTrailConfigModel> Configs_skirmishtrail => new(Storage.Configs.ContainsKey("skirmishtrail") == true ? Storage.Configs["skirmishtrail"].Cast<SkirmishTrailConfigModel>() : []);
    public ObservableCollection<UnitsConfigModel> Configs_units => new(Storage.Configs.ContainsKey("units") == true ? Storage.Configs["units"].Cast<UnitsConfigModel>() : []);
    public ObservableCollection<OthersConfigModel> Configs_others => new(Storage.Configs.ContainsKey("others") == true ? Storage.Configs["others"].Cast<OthersConfigModel>() : []);

    /// InstallState
    public StswProgressState InstallState
    {
        get => _installState;
        set => SetProperty(ref _installState, value);
    }
    private StswProgressState _installState;

    /// InstallValue
    public int InstallValue
    {
        get => _installValue;
        set => SetProperty(ref _installValue, value);
    }
    private int _installValue;
}
