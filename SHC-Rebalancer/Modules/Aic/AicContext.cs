using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class AicContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<AicConfigModel> Configs_aic => new(Storage.Configs.ContainsKey("aic") == true ? Storage.Configs["aic"].Cast<AicConfigModel>() : []);

    /// SelectedConfig
    public AicConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private AicConfigModel? _selectedConfig;
}
