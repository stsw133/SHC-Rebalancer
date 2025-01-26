using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class TroopsModel
{
    [JsonIgnore]
    public AIForTroops Key { get; set; }

    [JsonIgnore]
    public string Name => Key.ToString();

    public LordModel? Lord { get; set; }
    public Dictionary<SkirmishMode, Dictionary<Troop, int>>? Troops { get; set; }

    public class LordModel
    {
        public double? Strength { get; set; }
        public LordType? Type { get; set; }
    }
}
