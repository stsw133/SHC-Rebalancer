using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class OthersContext : StswObservableObject
{
    /// Configs
    public ObservableCollection<OthersConfigModel> Configs_others => new(Storage.Configs.ContainsKey("others") == true ? Storage.Configs["others"].Cast<OthersConfigModel>() : []);

    /// SelectedConfig
    public OthersConfigModel? SelectedConfig
    {
        get => _selectedConfig;
        set => SetProperty(ref _selectedConfig, value);
    }
    private OthersConfigModel? _selectedConfig;
}
