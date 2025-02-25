namespace SHC_Rebalancer;

/// OutpostsConfigModel
public class OutpostsConfigModel : ConfigModel
{
    public Dictionary<int, OutpostModel> Outposts { get; set; } = [];
}
