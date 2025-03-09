using System.Collections.ObjectModel;

namespace SHC_Rebalancer;

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public ObservableCollection<string> Maps { get; set; } = [];
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}
