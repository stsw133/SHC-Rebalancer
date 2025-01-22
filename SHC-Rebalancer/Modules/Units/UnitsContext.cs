using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class UnitsContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<UnitsConfigModel> Configs_units => new(Storage.Configs.ContainsKey("units") == true ? Storage.Configs["units"].Cast<UnitsConfigModel>() : []);

    /// SelectedConfig
    public UnitsConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private UnitsConfigModel? _selectedConfig;

    /// SelectedPair
    public KeyValuePair<Unit, UnitModel>? SelectedPair
    {
        get => _selectedPair;
        set => SetProperty(ref _selectedPair, value);
    }
    private KeyValuePair<Unit, UnitModel>? _selectedPair;
}
