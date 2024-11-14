namespace SHC_Rebalancer;
public class UnitDataModel
{
    public int? Speed { get; set; }
    public int? Health { get; set; }
    public int? DamageFromBow { get; set; }
    public int? DamageFromCrossbow { get; set; }
    public int? DamageFromSling { get; set; }
    public bool? CanMeleeDamage { get; set; }
    public int? MeleeDamage { get; set; }
    public Dictionary<string, int> MeleeDamageVs { get; set; } = [];
}
