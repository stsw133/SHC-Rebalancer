using System.Collections.ObjectModel;
using System.Reflection;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand AcceptTermsCommand { get; }
    public StswCommand ExitAppCommand { get; }

    public StswAsyncCommand ReloadAllCommand { get; }
    public StswAsyncCommand SaveAllCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }

    public StswAsyncCommand ShowUcpExplanationCommand { get; }

    public MainContext()
    {
        AcceptTermsCommand = new(AcceptTerms, () => Settings.Default.TermsAccepted);
        ExitAppCommand = new(App.Current.Shutdown);

        ReloadAllCommand = new(ReloadAll);
        SaveAllCommand = new(SaveAll);
        InstallCommand = new(Install, CanEditExe);
        UninstallCommand = new(Uninstall, CanEditExe);

        ShowUcpExplanationCommand = new(ShowUcpExplanation);
    }



    /// AcceptTerms
    public async Task AcceptTerms()
    {
        try
        {
            StswContentDialog.Close("TermsContentDialog");
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// ReloadAll
    public async Task ReloadAll()
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(1000);
            Storage.BaseAddresses = Storage.LoadBaseAddresses();

            foreach (var config in Storage.Configs)
            {
                var type = config.Key.ToLower();
                var selectedRebalance = Settings.Default["ConfigName_" + type].ToString()!;

                Storage.Configs[type] = Storage.LoadConfigs(type)[type].Cast<object>().ToList();
                OnPropertyChanged("Configs_" + type);

                if (Storage.Configs[type].Any(x => x.GetPropertyValue("Name")?.ToString() == selectedRebalance))
                    Settings.Default["ConfigName_" + type] = selectedRebalance;
                else if (Storage.Configs[type].Count > 0)
                    Settings.Default["ConfigName_" + type] = Storage.Configs[type].First().GetPropertyValue("Name");
            }

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// SaveAll
    public async Task SaveAll()
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
            if (!Settings.Default.IncludeUcp)
                await Task.Delay(400);

            //Backup.Restore();
            Backup.Make();
            Rebalancer.Rebalance();

            InstallValue += 40;
            await Task.Delay(400);

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    public bool CanEditExe() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(Settings.Default.GamePath);

    /// Uninstall
    public async Task Uninstall()
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(500);

            Backup.Restore();

            InstallValue += 50;
            await Task.Delay(500);

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// ShowUcpExplanation
    public async Task ShowUcpExplanation()
    {
        try
        {
            await StswContentDialog.Show(new UcpExplanationContext(), "InfoContentDialog");
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// Configs
    public ObservableCollection<AicConfigModel> Configs_aic => new(Storage.Configs.ContainsKey("aic") == true ? Storage.Configs["aic"].Cast<AicConfigModel>() : []);
    public ObservableCollection<AivConfigModel> Configs_aiv => new(Storage.Configs.ContainsKey("aiv") == true ? Storage.Configs["aiv"].Cast<AivConfigModel>() : []);
    public ObservableCollection<BuildingsConfigModel> Configs_buildings => new(Storage.Configs.ContainsKey("buildings") == true ? Storage.Configs["buildings"].Cast<BuildingsConfigModel>() : []);
    public ObservableCollection<GoodsConfigModel> Configs_goods => new(Storage.Configs.ContainsKey("goods") == true ? Storage.Configs["goods"].Cast<GoodsConfigModel>() : []);
    public ObservableCollection<ResourcesConfigModel> Configs_resources => new(Storage.Configs.ContainsKey("resources") == true ? Storage.Configs["resources"].Cast<ResourcesConfigModel>() : []);
    public ObservableCollection<SkirmishTrailConfigModel> Configs_skirmishtrail => new(Storage.Configs.ContainsKey("skirmishtrail") == true ? Storage.Configs["skirmishtrail"].Cast<SkirmishTrailConfigModel>() : []);
    public ObservableCollection<TroopsConfigModel> Configs_troops => new(Storage.Configs.ContainsKey("troops") == true ? Storage.Configs["troops"].Cast<TroopsConfigModel>() : []);
    public ObservableCollection<UnitsConfigModel> Configs_units => new(Storage.Configs.ContainsKey("units") == true ? Storage.Configs["units"].Cast<UnitsConfigModel>() : []);
    public ObservableCollection<CustomsConfigModel> Configs_customs => new(Storage.Configs.ContainsKey("customs") == true ? Storage.Configs["customs"].Cast<CustomsConfigModel>() : []);

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

    /// IsTermsContentDialogOpen
    public bool IsTermsContentDialogOpen
    {
        get => _isTermsContentDialogOpen;
        set => SetProperty(ref _isTermsContentDialogOpen, value);
    }
    private bool _isTermsContentDialogOpen = !Settings.Default.TermsAccepted;
}
