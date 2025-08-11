using System.Reflection;
using System.Windows;

namespace SHC_Rebalancer;
public partial class MainContext : StswObservableObject
{
    public MainContext()
    {
        StswMessanger.Instance.Register<ProgressUpdateMessage>(msg => InstallValue += msg.Increment);
        StswMessanger.Instance.Register<ProgressTextMessage>(msg => InstallText = msg.Text);
        GamePathChangedCommand.Execute();
    }

    [StswCommand]
    async Task AcceptTerms(bool parameter)
    {
        try
        {
            if (parameter)
            {
                SettingsService.Instance.Settings.TermsAccepted = true;
                StswContentDialog.Close("TermsDialog");
            }
            else Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    [StswCommand]
    async Task GamePathChanged()
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

    [StswCommand]
    async Task ShowUcpExplanation()
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



    [StswCommand]
    async Task ReloadAll()
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

    [StswCommand]
    async Task SaveAll()
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

    [StswCommand(ConditionMethodName = nameof(InstallCondition))]
    async Task Install(CancellationToken token)
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
    bool InstallCondition() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath);

    [StswCommand(ConditionMethodName = nameof(UninstallCondition))]
    async Task Uninstall()
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
    bool UninstallCondition() => InstallState != StswProgressState.Running && !string.IsNullOrEmpty(SettingsService.Instance.Settings.GamePath) && IsInstalled;
    
    [StswCommand]
    void UncheckUCP()
    {
        SettingsService.Instance.Settings.IncludeOptions = false;
        SettingsService.Instance.Settings.SelectedConfigs["aic"] = null;
    }

    [StswObservableProperty] StswProgressState _installState;
    [StswObservableProperty] string? _installText;
    [StswObservableProperty] int _installValue;
    [StswObservableProperty] int _installValueMax;
    [StswObservableProperty] bool _isInstalled;
    [StswObservableProperty] bool _termsAccepted = SettingsService.Instance.Settings.TermsAccepted;
    [StswObservableProperty] StswObservableDictionary<string, ConfigModel?> _selectedConfigs = [];
}
