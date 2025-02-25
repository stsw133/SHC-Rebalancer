namespace SHC_Rebalancer;

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}
