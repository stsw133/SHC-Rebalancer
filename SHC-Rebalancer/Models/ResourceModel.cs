using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class ResourceModel : StswObservableObject
{
    [JsonIgnore]
    public Resource Key { get; set; }

    [JsonIgnore]
    public string Name => Key.ToString();

    public int? Buy
    {
        get => _buy;
        set => SetProperty(ref _buy, value);
    }
    public int? _buy;

    public int? Sell
    {
        get => _sell;
        set => SetProperty(ref _sell, value);
    }
    public int? _sell;
}
