using System.Reflection;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand AcceptTermsCommand { get; }
    public StswCommand ExitAppCommand { get; }
    public StswAsyncCommand GamePathChangedCommand { get; }

    public StswAsyncCommand ReloadAllCommand { get; }
    public StswAsyncCommand SaveAllCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }

    public StswAsyncCommand ShowUcpExplanationCommand { get; }
    public StswCommand UncheckUCPCommand => new(() => { SettingsService.Instance.Settings.IncludeOptions = false; SettingsService.Instance.Settings.SelectedConfigs["aic"] = null; });

    public MainContext()
    {
        AcceptTermsCommand = new(AcceptTerms, () => SettingsService.Instance.Settings.TermsAccepted);
        ExitAppCommand = new(App.Current.Shutdown);
        GamePathChangedCommand = new(GamePathChanged);

        ReloadAllCommand = new(ReloadAll);
        SaveAllCommand = new(SaveAll);
        InstallCommand = new(Install, InstallCondition);
        UninstallCommand = new(Uninstall, UninstallCondition);

        ShowUcpExplanationCommand = new(ShowUcpExplanation);

        StswMessanger.Instance.Register<ProgressUpdateMessage>(msg => InstallValue += msg.Increment);
        StswMessanger.Instance.Register<ProgressTextMessage>(msg => InstallText = msg.Text);
        GamePathChangedCommand.Execute();
    }



    /// AcceptTerms
    public async Task AcceptTerms()
    {
        try
        {
            StswContentDialog.Close("TermsDialog");
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// GamePathChanged
    public async Task GamePathChanged()
    {
        try
        {
            if (!string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath))
            {
                if (BackupService.Exists(StorageService.ExePath[GameVersion.Crusader], out var _)
                 && BackupService.Exists(StorageService.ExePath[GameVersion.Extreme], out var _))
                {
                    InstallState = StswProgressState.Finished;
                    InstallText = "Installed...";
                    IsInstalled = true;
                }
                else
                {
                    InstallState = StswProgressState.Ready;
                    InstallText = "Ready to install...";
                    IsInstalled = false;
                }
            }
            else
            {
                InstallState = StswProgressState.Ready;
                InstallText = "Select game path...";
                IsInstalled = false;
            }
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
            StorageService.BaseAddresses = StorageService.LoadBaseAddresses();
            ReloadConfigs();

            InstallState = StswProgressState.Finished;
            InstallValue = 100;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    private void ReloadConfigs()
    {
        foreach (var config in StorageService.Configs)
        {
            var type = config.Key.ToLower();
            SettingsService.Instance.Settings.SelectedConfigs.TryGetValue(type, out var selectedRebalance);

            var newConfigs = StorageService.LoadConfigs(type)[type].Cast<object>().ToList();
            StorageService.Configs[type].Clear();
            foreach (var item in newConfigs)
                StorageService.Configs[type].Add(item);

            if (StorageService.Configs[type].Any(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance))
                SettingsService.Instance.Settings.SelectedConfigs[type] = selectedRebalance;
            else if (StorageService.Configs[type].Count > 0)
                SettingsService.Instance.Settings.SelectedConfigs[type] = StorageService.Configs[type]
                    .First()
                    .GetPropertyValue(nameof(ConfigModel.Name))!
                    .ToString()!;
        }
        OnPropertyChanged(nameof(SelectedConfigs));
    }

    /// SaveAll
    public async Task SaveAll()
    {
        try
        {
            InstallValue = 0;
            InstallState = StswProgressState.Running;

            await Task.Delay(1000);
            StorageService.SaveConfigs();
            SettingsService.Instance.SaveSettings();

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
            InstallText = "Installation starting...";
            InstallState = StswProgressState.Running;

            StorageService.SaveConfigs();
            SettingsService.Instance.SaveSettings();

            InstallValueMax = RebalancerService.CountTotalOperations() + 100;

            //Backup.Restore();

            await Task.Run(async () =>
            {
                InstallText = "Making backup...";
                BackupService.Make();
                await RebalancerService.Rebalance(token);
            }, token);

            InstallState = StswProgressState.Finished;
            InstallText = "Installation completed!";
            InstallValue = InstallValueMax;
            IsInstalled = true;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            InstallText = $"Installation error: {ex.Message}";
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    public bool InstallCondition() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath);

    /// Uninstall
    public async Task Uninstall()
    {
        try
        {
            InstallValue = 0;
            InstallValueMax = 100;
            InstallText = "Uninstallation starting...";
            InstallState = StswProgressState.Running;

            await Task.Delay(500);

            InstallText = "Restoring backup...";
            BackupService.Restore();

            InstallValue += 50;
            await Task.Delay(500);

            InstallState = StswProgressState.Finished;
            InstallText = "Uninstallation completed!";
            InstallValue = 100;
            IsInstalled = false;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
            InstallText = $"Uninstallation error: {ex.Message}";
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    public bool UninstallCondition() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath) && IsInstalled;

    /// ShowUcpExplanation
    public async Task ShowUcpExplanation()
    {
        try
        {
            await StswContentDialog.Show(new UcpExplanationView(), "InfoDialog");
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

    /// InstallText
    public string? InstallText
    {
        get => _installText;
        set => SetProperty(ref _installText, value);
    }
    private string? _installText;
    
    /// InstallValue
    public int InstallValue
    {
        get => _installValue;
        set => SetProperty(ref _installValue, value);
    }
    private int _installValue;

    /// InstallValueMax
    public int InstallValueMax
    {
        get => _installValueMax;
        set => SetProperty(ref _installValueMax, value);
    }
    private int _installValueMax;

    /// IsInstalled
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
    private bool _isInstalled;

    /// IsTermsDialogOpen
    public bool IsTermsDialogOpen
    {
        get => _isTermsDialogOpen;
        set => SetProperty(ref _isTermsDialogOpen, value);
    }
    private bool _isTermsDialogOpen = !SettingsService.Instance.Settings.TermsAccepted;

    /// SelectedConfigs
    public StswObservableDictionary<string, ConfigModel?> SelectedConfigs
    {
        get => _selectedConfigs;
        set => SetProperty(ref _selectedConfigs, value);
    }
    private StswObservableDictionary<string, ConfigModel?> _selectedConfigs = [];
}
