namespace SHC_Rebalancer;

/// AicConfigModel
public class AicConfigModel : ConfigModel
{
    public Dictionary<AI, AicModel> AIs { get; set; } = [];
}
