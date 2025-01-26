using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

/// ConfigModel
public class ConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

/// AicConfigModel
public class AicConfigModel : ConfigModel
{
    public Dictionary<AI, AicModel> Values { get; set; } = [];
}

/// AivConfigModel
public class AivConfigModel : ConfigModel
{

}

/// BuildingsConfigModel
public class BuildingsConfigModel : ConfigModel
{
    public Dictionary<Building, BuildingModel> Values { get; set; } = [];
}

/// ResourcesConfigModel
public class ResourcesConfigModel : ConfigModel
{
    public Dictionary<Resource, ResourceModel> Values { get; set; } = [];
}

/// ConfigModel
public class GoodsConfigModel : ConfigModel
{
    public Dictionary<SkirmishMode, GoodsModel> Values { get; set; } = [];
}

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public Dictionary<int, SkirmishTrailModel> Values { get; set; } = [];
}

/// TroopsConfigModel
public class TroopsConfigModel : ConfigModel
{
    public Dictionary<AIForTroops, TroopsModel> Values { get; set; } = [];
}

/// UnitsConfigModel
public class UnitsConfigModel : ConfigModel
{
    public Dictionary<Unit, UnitModel> Values { get; set; } = [];
}

/// CustomsConfigModel
public class CustomsConfigModel : ConfigModel
{
    public ObservableCollection<OtherValueModel> Values { get; set; } = [];
}
