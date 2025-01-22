using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace SHC_Rebalancer;
public class FinderContext : StswObservableObject
{
    public StswAsyncCommand FindCommand { get; }
    public StswCommand<DataGridCellEditEndingEventArgs> AddressValueChangedCommand { get; }

    public FinderContext()
    {
        FindCommand = new(Find);
        AddressValueChangedCommand = new(AddressValueChanged);
    }



    /// Find
    public async Task Find()
    {
        try
        {
            if (FinderFilterType.HasValue)
            {
                Finder.Find(FinderResults, FinderFilterType.Value, FinderFilterSize, FinderFilterAddress, FinderFilterSkips, FinderFilterValues);
                _finderResultsType = FinderFilterType.Value;
                _finderResultsSize = FinderFilterSize;
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// AddressValueChanged
    public void AddressValueChanged(DataGridCellEditEndingEventArgs? e)
    {
        try
        {
            if (e == null)
                return;

            if (e.Row.Item is FinderDataModel model && e.EditingElement is StswDecimalBox stsw)
            {
                var filePath = _finderResultsType == GameVersion.Extreme ? Settings.Default.ExtremePath : Settings.Default.CrusaderPath;

                try
                {
                    using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                    fs.Seek(Convert.ToInt32(model.Address, 16), SeekOrigin.Begin);

                    if (_finderResultsSize == 4)
                        fs.Write(BitConverter.GetBytes(Convert.ToInt32(stsw.Value)), 0, 4);
                    else if (_finderResultsSize == 1)
                        fs.WriteByte(Convert.ToByte(stsw.Value));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wystąpił błąd: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
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

    private GameVersion _finderResultsType;
    private int _finderResultsSize;

    /// IsEditModeEnabled
    public bool IsEditModeEnabled
    {
        get => _isEditModeEnabled;
        set => SetProperty(ref _isEditModeEnabled, value);
    }
    private bool _isEditModeEnabled;
}
