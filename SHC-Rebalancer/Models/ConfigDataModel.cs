namespace SHC_Rebalancer;
public class ConfigDataModel
{
    public IEnumerable<AddressModel> BaseAddresses { get; set; } = [];
    public Dictionary<string, BuildingDataModel> Buildings { get; set; } = [];
    public Dictionary<string, ResourceDataModel> Resources { get; set; } = [];
    public Dictionary<string, UnitDataModel> Units { get; set; } = [];
    public IEnumerable<AddressModel> Other { get; set; } = [];
}
