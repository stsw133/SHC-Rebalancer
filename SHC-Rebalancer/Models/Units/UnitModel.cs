namespace SHC_Rebalancer;
public class UnitModel
{
    public int? Cost { get; set; }
    public int? Speed { get; set; }
    public bool? AlwaysRun { get; set; }
    public int? Health { get; set; }
    public int? DamageFromBow { get; set; }
    public int? DamageFromSling { get; set; }
    public int? DamageFromCrossbow { get; set; }
    public bool? CanMeleeDamage { get; set; }
    public int? MeleeDamage { get; set; }
    public Dictionary<Unit, int> MeleeDamageVs { get; set; } = [];
    public int? MeleeDamageToBuildings { get; set; }
    public int? MeleeDamageToTowers { get; set; }
    public int? MeleeDamageToWalls { get; set; }
    public bool? CanDigMoat { get; set; }
    public bool? CanClimbLadder { get; set; }
    public bool? CanGoOnWall { get; set; }
    public bool? CanBeMoved { get; set; }
}
