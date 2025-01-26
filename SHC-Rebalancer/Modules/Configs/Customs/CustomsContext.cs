using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class CustomsContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<CustomsConfigModel> Configs_customs => new(Storage.Configs.ContainsKey("customs") == true ? Storage.Configs["customs"].Cast<CustomsConfigModel>() : []);

    /// SelectedConfig
    public CustomsConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private CustomsConfigModel? _selectedConfig;
}
