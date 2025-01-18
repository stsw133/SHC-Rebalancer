using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

public class AicConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Dictionary<AI, AicModel> Values { get; set; } = [];
    public ObservableCollection<AicModel> View => new(Values.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}

public class BuildingsConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Dictionary<Building, BuildingModel> Values { get; set; } = [];
    public ObservableCollection<BuildingModel> View => new(Values.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}

public class OthersConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public ObservableCollection<OtherValueModel> Values { get; set; } = [];
}

public class ResourcesConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Dictionary<Resource, ResourceModel> Values { get; set; } = [];
    public ObservableCollection<ResourceModel> View => new(Values.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}

public class SkirmishTrailConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Dictionary<int, SkirmishTrailModel> Values { get; set; } = [];
    public ObservableCollection<SkirmishTrailModel> View => new(Values.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}

public class UnitsConfigModel : IConfigModel
{
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Dictionary<Unit, UnitModel> Values { get; set; } = [];
    public ObservableCollection<UnitModel> View => new(Values.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}
