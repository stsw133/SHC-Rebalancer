namespace SHC_Rebalancer;

/// BuildingsConfigModel
public class BuildingsConfigModel : ConfigModel
{
    public Dictionary<Building, BuildingModel> Buildings { get; set; } = [];
}
