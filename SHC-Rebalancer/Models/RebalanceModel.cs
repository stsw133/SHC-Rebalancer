using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class RebalanceModel
{
    [JsonIgnore]
    public string Key { get; set; } = string.Empty;

    public Dictionary<Building, BuildingModel> Buildings { get; set; } = [];
    public Dictionary<Resource, ResourceModel> Resources { get; set; } = [];
    public Dictionary<int, SkirmishMissionModel> SkirmishTrail { get; set; } = [];
    public Dictionary<Unit, UnitModel> Units { get; set; } = [];
    public IEnumerable<BaseValueModel> Other { get; set; } = [];

    /// Views
    [JsonIgnore]
    public ObservableCollection<BuildingModel> BuildingsView => new(Buildings.Select(x => { x.Value.Key = x.Key; return x.Value; }));

    [JsonIgnore]
    public ObservableCollection<ResourceModel> ResourcesView => new(Resources.Select(x => { x.Value.Key = x.Key; return x.Value; }));

    [JsonIgnore]
    public ObservableCollection<SkirmishMissionModel> SkirmishTrailView => new(SkirmishTrail.Select(x => { x.Value.Key = x.Key; return x.Value; }));

    [JsonIgnore]
    public ObservableCollection<UnitModel> UnitsView => new(Units.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}
