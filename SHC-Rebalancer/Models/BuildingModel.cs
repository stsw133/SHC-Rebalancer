namespace SHC_Rebalancer;
public class BuildingModel
{
    public Building Key { get; set; }
    public string Name => Key.ToString();

    public int? Health { get; set; }
    public byte? Housing { get; set; }
    public int[]? Cost { get; set; }
}
