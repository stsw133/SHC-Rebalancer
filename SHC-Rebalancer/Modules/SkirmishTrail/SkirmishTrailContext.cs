using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class SkirmishTrailContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<SkirmishTrailConfigModel> Configs_skirmishtrail => new(Storage.Configs.ContainsKey("skirmishtrail") == true ? Storage.Configs["skirmishtrail"].Cast<SkirmishTrailConfigModel>() : []);

    /// SelectedConfig
    public SkirmishTrailConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private SkirmishTrailConfigModel? _selectedConfig;

    /// SelectedPair
    public KeyValuePair<int, SkirmishTrailModel>? SelectedPair
    {
        get => _selectedPair;
        set => SetProperty(ref _selectedPair, value);
    }
    private KeyValuePair<int, SkirmishTrailModel>? _selectedPair;
}
