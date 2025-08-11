using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace SHC_Rebalancer;
public partial class FinderContext : StswObservableObject
{
    [StswCommand]
    async Task Find()
    {
        try
        {
            if (FinderFilterVersion.HasValue)
            {
                FinderService.Find(FinderResults, FinderFilterVersion.Value, FinderFilterSize, FinderDisplayAsChar, FinderFilterAddress, FinderFilterSkips ?? 0, FinderFilterValues, FinderFilterLimit);
                _finderResultsType = FinderFilterVersion.Value;
                _finderResultsSize = FinderFilterSize;
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    [StswCommand]
    void AddressValueChanged(DataGridCellEditEndingEventArgs? e)
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
    
    [StswObservableProperty] GameVersion? _finderFilterVersion;
    partial void OnFinderFilterVersionChanged(GameVersion? oldValue, GameVersion? newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] int _finderFilterSize = 1;
    partial void OnFinderFilterSizeChanged(int oldValue, int newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] bool _finderDisplayAsChar;
    partial void OnFinderDisplayAsCharChanged(bool oldValue, bool newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] string? _finderFilterAddress;
    partial void OnFinderFilterAddressChanged(string? oldValue, string? newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] int? _finderFilterSkips;
    partial void OnFinderFilterSkipsChanged(int? oldValue, int? newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] string? _finderFilterValues;
    partial void OnFinderFilterValuesChanged(string? oldValue, string? newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] int _finderFilterLimit = 50;
    partial void OnFinderFilterLimitChanged(int oldValue, int newValue) => FindCommand.Execute(FinderFilterVersion);

    [StswObservableProperty] ObservableCollection<FinderDataModel> _finderResults = [];
    private GameVersion _finderResultsType;
    private int _finderResultsSize;
    [StswObservableProperty] bool _isEditModeEnabled;
}
