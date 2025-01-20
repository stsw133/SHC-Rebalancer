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

    /// SelectedPair1
    public KeyValuePair<AI, AicModel>? SelectedPair1
    {
        get => _selectedPair1;
        set => SetProperty(ref _selectedPair1, value);
    }
    private KeyValuePair<AI, AicModel>? _selectedPair1;

    /// SelectedPair2
    public KeyValuePair<AI, AicModel>? SelectedPair2
    {
        get => _selectedPair2;
        set => SetProperty(ref _selectedPair2, value);
    }
    private KeyValuePair<AI, AicModel>? _selectedPair2;
}
