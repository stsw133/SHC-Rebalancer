using System.Collections.ObjectModel;

namespace SHC_Rebalancer;

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public ObservableCollection<string> Maps { get; set; } = [];
    public string MapsSingleView
    {
        get => string.Join(Environment.NewLine, Maps);
        set => Maps = [.. value.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)];
    }
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}
