namespace SHC_Rebalancer;
public class BaseValueModel
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
    public GameVersion? Version { get; set; }
}
