using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

/// ConfigModel
public abstract class ConfigModel : StswObservableObject, IConfigModel
{
    [JsonIgnore]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name = string.Empty;

    public string? Description { get; set; }
}

/// AicConfigModel
public class AicConfigModel : ConfigModel
{
    public Dictionary<AI, AicModel> AIs { get; set; } = [];
}

/// AivConfigModel
public class AivConfigModel : ConfigModel
{

}

/// BuildingsConfigModel
public class BuildingsConfigModel : ConfigModel
{
    public Dictionary<Building, BuildingModel> Buildings { get; set; } = [];
    public Dictionary<int, OutpostModel> Outposts { get; set; } = [];
}

/// GoodsConfigModel
public class GoodsConfigModel : ConfigModel
{
    public Dictionary<SkirmishMode, GoodsModel> Goods { get; set; } = [];
}

/// ResourcesConfigModel
public class ResourcesConfigModel : ConfigModel
{
    public Dictionary<Resource, ResourceModel> Prices { get; set; } = [];
}

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}

/// TroopsConfigModel
public class TroopsConfigModel : ConfigModel
{
    public Dictionary<AIForTroops, TroopsModel> Troops { get; set; } = [];
}

/// UnitsConfigModel
public class UnitsConfigModel : ConfigModel
{
    public Dictionary<Unit, UnitModel> Units { get; set; } = [];
}

/// CustomsConfigModel
public class CustomsConfigModel : ConfigModel
{
    public ObservableCollection<OtherValueModel> Values { get; set; } = [];
}
