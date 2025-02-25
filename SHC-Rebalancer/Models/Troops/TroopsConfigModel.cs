namespace SHC_Rebalancer;

/// TroopsConfigModel
public class TroopsConfigModel : ConfigModel
{
    public Dictionary<AIForTroops, Dictionary<SkirmishMode, StswDictionary<Troop, int?>>> Troops { get; set; } = [];
}
