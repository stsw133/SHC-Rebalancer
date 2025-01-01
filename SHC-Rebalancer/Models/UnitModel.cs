namespace SHC_Rebalancer;
public class UnitModel
{
    public int? Speed { get; set; }
    public bool? CanGoOnWall { get; set; }
    public bool? CanBeMoved { get; set; }
    public int? Health { get; set; }
    public int? DamageFromBow { get; set; }
    public int? DamageFromSling { get; set; }
    public int? DamageFromCrossbow { get; set; }
    public bool? CanMeleeDamage { get; set; }
    public int? MeleeDamage { get; set; }
    public Dictionary<string, int> MeleeDamageVs { get; set; } = [];
    public bool? CanClimbLadder { get; set; }
}
