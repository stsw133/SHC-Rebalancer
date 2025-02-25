namespace SHC_Rebalancer;

/// UnitsConfigModel
public class UnitsConfigModel : ConfigModel
{
    public Dictionary<Unit, UnitModel> Units { get; set; } = [];
}
