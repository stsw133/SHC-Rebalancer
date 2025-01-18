using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class BuildingModel(Building key)
{
    [JsonIgnore]
    public Building Key { get; set; } = key;

    [JsonIgnore]
    public string Name => Key.ToString();

    public int? Health { get; set; }
    public int? Housing { get; set; }
    public int[]? Cost { get; set; }
}
