using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class BuildingModel
{
    [JsonIgnore]
    public Building Key { get; set; }

    [JsonIgnore]
    public string Name => Key.ToString();

    public int? Health { get; set; }

    public byte? Housing { get; set; }

    public int[]? Cost { get; set; }
}
