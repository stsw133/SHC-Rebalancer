using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand SaveChangesCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }
    public StswCommand<object> ReloadRebalancesCommand { get; }
    public StswAsyncCommand<string> EditRebalanceCommand { get; }
    public StswAsyncCommand<string> RemoveRebalanceCommand { get; }
    public StswAsyncCommand<GameVersion> FindCommand { get; }

    public MainContext()
    {
        SaveChangesCommand = new(SaveChanges);
        InstallCommand = new(Install, () => InstallState != StswProgressState.Running);
        UninstallCommand = new(Uninstall, () => InstallState != StswProgressState.Running);

        ReloadRebalancesCommand = new(ReloadRebalances);
        EditRebalanceCommand = new(EditRebalance);
        RemoveRebalanceCommand = new(RemoveRebalance, () => Settings.Default.RebalanceName != "vanilla");

        FindCommand = new(Find);

        Task.Run(() => ReloadRebalancesCommand.Execute());
    }



    /// SaveChanges
    public async Task SaveChanges()
    {
        try
        {
            foreach (var rebalance in Storage.Rebalances)
            {
                var filePath = Path.Combine(Storage.PathRebalances, rebalance.Key + ".json");
                Storage.SaveJsonIntoFile(rebalance.Value, filePath);
            }
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
            if (SelectedRebalance == null)
                return;

            InstallState = StswProgressState.Running;

            var filePath = Path.Combine(Storage.PathRebalances, Storage.Rebalances.FirstOrDefault(x => x.Value == SelectedRebalance).Key + ".json");
            Storage.SaveJsonIntoFile(SelectedRebalance, filePath);

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

            if (parameter == "vanilla")
            {
                await StswMessageDialog.Show("`vanilla` rebalance config cannot be removed.", "Information", null, StswDialogButtons.OK, StswDialogImage.Information);
                return;
            }

            var filePath = Path.Combine(Storage.PathRebalances, parameter + ".json");
            if (File.Exists(filePath) && await StswMessageDialog.Show($"Are you sure you want to remove '{parameter}' rebalance config?", "Confirmation", null, StswDialogButtons.YesNo, StswDialogImage.Question) == true)
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// Find
    public async Task Find(GameVersion type)
    {
        try
        {
            Finder.Find(FinderResults, type, FinderFilterSize, FinderFilterAddress, FinderFilterSkips, FinderFilterValues);
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

    /// SelectedRebalance
    public RebalanceModel? SelectedRebalance
    {
        get => _selectedRebalance;
        set => SetProperty(ref _selectedRebalance, value);
    }
    private RebalanceModel? _selectedRebalance;

    /// FinderFilterType
    public GameVersion? FinderFilterType
    {
        get => _finderFilterType;
        set => SetProperty(ref _finderFilterType, value);
    }
    private GameVersion? _finderFilterType;

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
    public int FinderFilterSkips
    {
        get => _finderFilterSkips;
        set
        {
            SetProperty(ref _finderFilterSkips, value);
            FindCommand.Execute(FinderFilterType);
        }
    }
    private int _finderFilterSkips = 0;

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
