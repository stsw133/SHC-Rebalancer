namespace SHC_Rebalancer;
public class SkirmishMissionModel
{
    public string? MapNameAddress { get; set; }

    public string? MapName { get; set; }

    public int? Difficulty { get; set; }

    public SkirmishType? Type { get; set; }

    public int[]? AIs { get; set; }

    public int[]? Locations { get; set; }

    public int[]? Teams { get; set; }

    public int[]? AIVs { get; set; }
}
