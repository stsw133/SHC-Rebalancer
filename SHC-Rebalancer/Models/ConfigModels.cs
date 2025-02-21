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

/// OptionsConfigModel
public class OptionsConfigModel : ConfigModel
{
    public Dictionary<string, OptionModel> Options { get; set; } = [];
}

/// AicConfigModel
public class AicConfigModel : ConfigModel
{
    public Dictionary<AI, AicModel> AIs { get; set; } = [];
}

/// AivConfigModel
public class AivConfigModel : ConfigModel
{
    public Dictionary<AI, ObservableCollection<AivModel>> AIs { get; set; } = [];
    public Dictionary<string, string> Images { get; set; } = [];
}

/// GoodsConfigModel
public class GoodsConfigModel : ConfigModel
{
    public Dictionary<SkirmishMode, GoodsModel> Goods { get; set; } = [];
}

/// TroopsConfigModel
public class TroopsConfigModel : ConfigModel
{
    public Dictionary<AIForTroops, Dictionary<SkirmishMode, StswDictionary<Troop, int?>>> Troops { get; set; } = [];
}

/// BuildingsConfigModel
public class BuildingsConfigModel : ConfigModel
{
    public Dictionary<Building, BuildingModel> Buildings { get; set; } = [];
    public Dictionary<int, OutpostModel> Outposts { get; set; } = [];
}

/// PopularityConfigModel
public class PopularityConfigModel : ConfigModel
{
    public PopularityModel Popularity { get; set; } = new();
}

/// ResourcesConfigModel
public class ResourcesConfigModel : ConfigModel
{
    public Dictionary<Resource, ResourceModel> Prices { get; set; } = [];
}

/// UnitsConfigModel
public class UnitsConfigModel : ConfigModel
{
    public Dictionary<Unit, UnitModel> Units { get; set; } = [];
}

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}

/// CustomsConfigModel
public class CustomsConfigModel : ConfigModel
{
    public ObservableCollection<OtherValueModel> Values { get; set; } = [];
}
