using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class GoodsModel : StswObservableObject
{
    [JsonIgnore]
    public SkirmishMode Key { get; set; }

    [JsonIgnore]
    public string Name => Key.ToString();

    public GoldModel? Gold { get; set; }

    public Dictionary<Resource, int>? Resources { get; set; }

    public class GoldModel
    {
        public int[]? Human { get; set; }
        public int[]? AI { get; set; }
    }
}
