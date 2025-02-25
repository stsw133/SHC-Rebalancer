namespace SHC_Rebalancer;
public class OtherValueModel
{
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public int? Size { get; set; }
    public object? Value { get; set; }
    public GameVersion? Version { get; set; }
    public bool IsEnabled { get; set; } = true;
}
