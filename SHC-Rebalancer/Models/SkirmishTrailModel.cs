using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class SkirmishTrailModel
{
    [JsonIgnore]
    public int Key { get; set; }

    public string? MapNameAddress { get; set; }
    public string? MapName { get; set; }
    public SkirmishDifficulty? Difficulty { get; set; }
    public SkirmishMode? Type { get; set; }
    public int? NumberOfPlayers { get; set; }
    public AI[]? AIs { get; set; } = [];
    public int[]? Locations { get; set; } = [];
    public SkirmishTeam[]? Teams { get; set; } = [];
    public int[]? AIVs { get; set; } = [];
}
