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
        AcceptTermsCommand = new(AcceptTerms, () => SettingsService.Instance.Settings.TermsAccepted);
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
                var selectedRebalance = SettingsService.Instance.Settings.SelectedConfigs[type];

                var newConfigs = Storage.LoadConfigs(type)[type].Cast<object>().ToList();
                Storage.Configs[type].Clear();
                foreach (var item in newConfigs)
                    Storage.Configs[type].Add(item);

                OnPropertyChanged(nameof(SelectedConfigs));

                if (Storage.Configs[type].Any(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance))
                    SettingsService.Instance.Settings.SelectedConfigs[type] = selectedRebalance;
                else if (Storage.Configs[type].Count > 0)
                    SettingsService.Instance.Settings.SelectedConfigs[type] = Storage.Configs[type].First().GetPropertyValue(nameof(ConfigModel.Name))!.ToString()!;
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
            if (!SettingsService.Instance.Settings.IncludeUCP)
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
    public bool CanEditExe() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath);

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
    private bool _isTermsContentDialogOpen = !SettingsService.Instance.Settings.TermsAccepted;

    /// SelectedConfigs
    public StswDictionary<string, ConfigModel> SelectedConfigs
    {
        get => _selectedConfigs;
        set => SetProperty(ref _selectedConfigs, value);
    }
    private StswDictionary<string, ConfigModel> _selectedConfigs = [];
}
