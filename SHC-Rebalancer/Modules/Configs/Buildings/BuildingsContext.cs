using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class BuildingsContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<BuildingsConfigModel> Configs_buildings => new(Storage.Configs.ContainsKey("buildings") == true ? Storage.Configs["buildings"].Cast<BuildingsConfigModel>() : []);

    /// SelectedConfig
    public BuildingsConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private BuildingsConfigModel? _selectedConfig;

    /// SelectedPair
    public KeyValuePair<Building, BuildingModel>? SelectedPair
    {
        get => _selectedPair;
        set => SetProperty(ref _selectedPair, value);
    }
    private KeyValuePair<Building, BuildingModel>? _selectedPair;
}
