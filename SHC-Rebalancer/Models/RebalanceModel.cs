using System.Collections.ObjectModel;

namespace SHC_Rebalancer;
public class RebalanceModel
{
    public Dictionary<Building, BuildingModel> Buildings { get; set; } = [];
    public Dictionary<Resource, ResourceModel> Resources { get; set; } = [];
    public Dictionary<int, SkirmishMissionModel> SkirmishTrail { get; set; } = [];
    public Dictionary<Unit, UnitModel> Units { get; set; } = [];
    public IEnumerable<BaseValueModel> Other { get; set; } = [];

    /// Views
    public ObservableCollection<BuildingModel> BuildingsView => new(Buildings.Select(x => { x.Value.Key = x.Key; return x.Value; }));
    public ObservableCollection<ResourceModel> ResourcesView => new(Resources.Select(x => { x.Value.Key = x.Key; return x.Value; }));
}
