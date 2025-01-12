using System.Collections.ObjectModel;
using System.Reflection;

namespace SHC_Rebalancer;
public class FinderContext : StswObservableObject
{
    public StswAsyncCommand<GameVersion> FindCommand { get; }

    public FinderContext()
    {
        FindCommand = new(Find);
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
