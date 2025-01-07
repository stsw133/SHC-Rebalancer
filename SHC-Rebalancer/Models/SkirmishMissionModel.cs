using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class SkirmishMissionModel
{
    [JsonIgnore]
    public int Key { get; set; }

    public string? MapNameAddress { get; set; }

    public string? MapName { get; set; }

    public int? Difficulty { get; set; }

    public SkirmishType? Type { get; set; }

    public AI[]? AIs { get; set; } = [];

    public int[]? Locations { get; set; } = [];

    public int[]? Teams { get; set; } = [];

    public int[]? AIVs { get; set; } = [];
}
