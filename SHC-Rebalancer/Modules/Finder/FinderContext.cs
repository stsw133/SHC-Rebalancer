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
            if (FinderFilterVersion.HasValue)
            {
                FinderService.Find(FinderResults, FinderFilterVersion.Value, FinderFilterSize, FinderFilterAddress, FinderFilterSkips ?? 0, FinderFilterValues, FinderFilterLimit);
                _finderResultsType = FinderFilterVersion.Value;
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
                try
                {
                    using FileStream fs = new FileStream(StorageService.ExePath[_finderResultsType], FileMode.Open, FileAccess.ReadWrite);
                    fs.Seek(Convert.ToInt32(model.Address, 16), SeekOrigin.Begin);

                    switch (_finderResultsSize)
                    {
                        case 1:
                            fs.WriteByte(Convert.ToByte(stsw.Value));
                            break;
                        case 2:
                            fs.Write(BitConverter.GetBytes(Convert.ToInt16(stsw.Value)), 0, 2);
                            break;
                        case 4:
                            fs.Write(BitConverter.GetBytes(Convert.ToInt32(stsw.Value)), 0, 4);
                            break;
                    }
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
    public GameVersion? FinderFilterVersion
    {
        get => _finderFilterVersion;
        set => SetProperty(ref _finderFilterVersion, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private GameVersion? _finderFilterVersion;

    /// FinderFilterSize
    public int FinderFilterSize
    {
        get => _finderFilterSize;
        set => SetProperty(ref _finderFilterSize, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private int _finderFilterSize = 1;

    /// FinderFilterAddress
    public string? FinderFilterAddress
    {
        get => _finderFilterAddress;
        set => SetProperty(ref _finderFilterAddress, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private string? _finderFilterAddress;

    /// FinderFilterSkips
    public int? FinderFilterSkips
    {
        get => _finderFilterSkips;
        set => SetProperty(ref _finderFilterSkips, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private int? _finderFilterSkips;

    /// FinderFilterValues
    public string? FinderFilterValues
    {
        get => _finderFilterValues;
        set => SetProperty(ref _finderFilterValues, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private string? _finderFilterValues;

    /// FinderFilterLimit
    public int FinderFilterLimit
    {
        get => _finderFilterLimit;
        set => SetProperty(ref _finderFilterLimit, value, () => FindCommand.Execute(FinderFilterVersion));
    }
    private int _finderFilterLimit = 50;

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
