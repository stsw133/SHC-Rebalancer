using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswCommand<object> ReloadRebalancesCommand { get; }
    public StswAsyncCommand<string> EditRebalanceCommand { get; }
    public StswAsyncCommand<string> RemoveRebalanceCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }

    public MainContext()
    {
        InstallCommand = new(Install, () => InstallState != StswProgressState.Running);
        UninstallCommand = new(Uninstall, () => InstallState != StswProgressState.Running);

        ReloadRebalancesCommand = new(ReloadRebalances);
        EditRebalanceCommand = new(EditRebalance);
        RemoveRebalanceCommand = new(RemoveRebalance);

        Task.Run(() => ReloadRebalancesCommand.Execute());
    }



    /// ReloadRebalances
    private void ReloadRebalances(object? parameter)
    {
        try
        {
            var currentRebalanceName = Settings.Default.RebalanceName;

            Storage.LoadBaseAddresses();
            Storage.LoadRebalances();

            if (parameter is ComboBox comboBox)
                comboBox.ItemsSource = Storage.Rebalances;

            if (Storage.Rebalances.TryGetValue(currentRebalanceName, out var value))
                SelectedRebalance = value;
            else if (Storage.Rebalances.Count > 0)
                SelectedRebalance = Storage.Rebalances.First().Value;
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// EditRebalance
    private async Task EditRebalance(string? parameter)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return;

            var filePath = Path.Combine(Storage.PathRebalances, parameter + ".json");
            if (!File.Exists(filePath))
            {
                var filePathVanilla = Path.Combine(Storage.PathRebalances, "vanilla.json");
                if (File.Exists(filePathVanilla))
                    File.Copy(filePathVanilla, filePath);
                else
                    File.Create(filePath);
            }
            StswFn.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// RemoveRebalance
    private async Task RemoveRebalance(string? parameter)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return;

            var filePath = Path.Combine(Storage.PathRebalances, parameter + ".json");
            if (File.Exists(filePath) && await StswMessageDialog.Show($"Are you sure you want to remove '{parameter}' rebalance config?", "Confirmation", null, StswDialogButtons.YesNo, StswDialogImage.Question) == true)
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// Install
    public async Task Install(CancellationToken token)
    {
        try
        {
            InstallState = StswProgressState.Running;

            Backup.Make(Settings.Default.CrusaderPath);
            Rebalancer.Rebalance(GameVersion.Crusader, Settings.Default.CrusaderPath, SelectedRebalance);

            Backup.Make(Settings.Default.ExtremePath);
            Rebalancer.Rebalance(GameVersion.Extreme, Settings.Default.ExtremePath, SelectedRebalance);

            InstallState = StswProgressState.Finished;
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
            InstallState = StswProgressState.Running;

            Backup.Restore(Settings.Default.CrusaderPath);
            Backup.Restore(Settings.Default.ExtremePath);

            InstallState = StswProgressState.Finished;
        }
        catch (Exception ex)
        {
            InstallState = StswProgressState.Error;
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

    /// SelectedRebalance
    public RebalanceModel? SelectedRebalance
    {
        get => _selectedRebalance;
        set => SetProperty(ref _selectedRebalance, value);
    }
    private RebalanceModel? _selectedRebalance;
}
