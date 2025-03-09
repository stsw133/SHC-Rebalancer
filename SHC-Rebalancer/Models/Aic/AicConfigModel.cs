namespace SHC_Rebalancer;

/// AicConfigModel
public class AicConfigModel : ConfigModel
{
    public Dictionary<string, AicModel> AIs { get; set; } = [];
}
