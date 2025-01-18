using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class ResourcesContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<ResourcesConfigModel> Configs_resources => new(Storage.Configs.ContainsKey("resources") == true ? Storage.Configs["resources"].Cast<ResourcesConfigModel>() : []);

    /// SelectedConfig
    public ResourcesConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private ResourcesConfigModel? _selectedConfig;
}
