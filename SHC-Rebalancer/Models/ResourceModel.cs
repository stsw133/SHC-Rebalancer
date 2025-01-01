namespace SHC_Rebalancer;
public class ResourceModel : StswObservableObject
{
    public Resource Key { get; set; }
    public string Name => Key.ToString();

    public int? Buy
    {
        get => _buy;
        set => SetProperty(ref _buy, value, nameof(SellRatio));
    }
    public int? _buy;

    public int? Sell
    {
        get => _sell;
        set => SetProperty(ref _sell, value, nameof(SellRatio));
    }
    public int? _sell;

    public decimal? SellRatio => 1.0m * Sell / Buy;
}
