namespace SHC_Rebalancer;

/// TroopsConfigModel
public class TroopsConfigModel : ConfigModel
{
    public Dictionary<string, Dictionary<SkirmishMode, StswObservableDictionary<Troop, int?>>> Troops { get; set; } = [];
}
