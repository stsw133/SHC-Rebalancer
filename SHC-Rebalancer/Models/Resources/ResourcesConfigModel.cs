namespace SHC_Rebalancer;

/// ResourcesConfigModel
public class ResourcesConfigModel : ConfigModel
{
    public Dictionary<Resource, ResourceModel> Resources { get; set; } = [];
}
