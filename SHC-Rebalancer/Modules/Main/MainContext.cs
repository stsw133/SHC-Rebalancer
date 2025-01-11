using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace SHC_Rebalancer;
public class MainContext : StswObservableObject
{
    public StswAsyncCommand SaveChangesCommand { get; }
    public StswCancellableAsyncCommand InstallCommand { get; }
    public StswAsyncCommand UninstallCommand { get; }

    public StswCommand<object?> ReloadRebalancesCommand { get; }
    public StswAsyncCommand AddRebalanceCommand { get; }
    public StswAsyncCommand RenameRebalanceCommand { get; }
    public StswAsyncCommand OpenRebalanceCommand { get; }
    public StswAsyncCommand RemoveRebalanceCommand { get; }

    public StswAsyncCommand<GameVersion> FindCommand { get; }

    public MainContext()
    {
        SaveChangesCommand = new(SaveChanges);
        InstallCommand = new(Install, () => InstallState != StswProgressState.Running);
        UninstallCommand = new(Uninstall, () => InstallState != StswProgressState.Running);

        ReloadRebalancesCommand = new(ReloadRebalances);
        AddRebalanceCommand = new(AddRebalance);
        RenameRebalanceCommand = new(RenameRebalance, () => !Settings.Default.RebalanceName.In(null, "vanilla"));
        OpenRebalanceCommand = new(OpenRebalance, () => Settings.Default.RebalanceName != null);
        RemoveRebalanceCommand = new(RemoveRebalance, () => !Settings.Default.RebalanceName.In(null, "vanilla"));

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
            if (Storage.Rebalances[Settings.Default.RebalanceName] == null)
                return;

            InstallState = StswProgressState.Running;

            var filePath = Path.Combine(Storage.PathRebalances, Settings.Default.RebalanceName + ".json");
            Storage.SaveJsonIntoFile(Storage.Rebalances[Settings.Default.RebalanceName], filePath);

            Backup.Make(Settings.Default.CrusaderPath);
            Rebalancer.Rebalance(GameVersion.Crusader, Settings.Default.CrusaderPath, Storage.Rebalances[Settings.Default.RebalanceName]);

            Backup.Make(Settings.Default.ExtremePath);
            Rebalancer.Rebalance(GameVersion.Extreme, Settings.Default.ExtremePath, Storage.Rebalances[Settings.Default.RebalanceName]);

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
            var selectedRebalance = Settings.Default.RebalanceName;

            Storage.LoadBaseAddresses();
            Storage.LoadRebalances();
            OnPropertyChanged(nameof(Rebalances));

            if (Storage.Rebalances.ContainsKey(selectedRebalance))
                Settings.Default.RebalanceName = selectedRebalance;
            else if (Storage.Rebalances.Count > 0)
                Settings.Default.RebalanceName = Storage.Rebalances.First().Key;
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// AddRebalance
    private async Task AddRebalance()
    {
        try
        {
            await StswContentDialog.Show(new NewConfigContext(), "MainContentDialog");

            var selectedRebalance = Settings.Default.RebalanceName;
            OnPropertyChanged(nameof(Rebalances));

            if (Storage.Rebalances.ContainsKey(selectedRebalance))
                Settings.Default.RebalanceName = selectedRebalance;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// RenameRebalance
    private async Task RenameRebalance()
    {
        try
        {
            await StswContentDialog.Show(new NewConfigContext(true), "MainContentDialog");

            var selectedRebalance = Settings.Default.RebalanceName;
            OnPropertyChanged(nameof(Rebalances));

            if (Storage.Rebalances.ContainsKey(selectedRebalance))
                Settings.Default.RebalanceName = selectedRebalance;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// OpenRebalance
    private async Task OpenRebalance()
    {
        try
        {
            var filePath = Path.Combine(Storage.PathRebalances, Settings.Default.RebalanceName + ".json");

            if (!File.Exists(filePath))
                throw new IOException("File for selected rebalance config does not exist!");
            
            StswFn.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// RemoveRebalance
    private async Task RemoveRebalance()
    {
        try
        {
            if (Settings.Default.RebalanceName == "vanilla")
            {
                await StswMessageDialog.Show("`vanilla` rebalance config cannot be removed.", "Information", null, StswDialogButtons.OK, StswDialogImage.Information);
                return;
            }

            var filePath = Path.Combine(Storage.PathRebalances, Settings.Default.RebalanceName + ".json");
            if (await StswMessageDialog.Show($"Are you sure you want to remove '{Settings.Default.RebalanceName}' rebalance config?", "Confirmation", null, StswDialogButtons.YesNo, StswDialogImage.Question) == true)
            {
                File.Delete(filePath);
                Storage.Rebalances.Remove(Settings.Default.RebalanceName);
                OnPropertyChanged(nameof(Rebalances));
            }
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

    /// Rebalances
    public ObservableCollection<RebalanceModel> Rebalances => new(Storage.Rebalances.Select(x => { x.Value.Key = x.Key; return x.Value; }));

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
