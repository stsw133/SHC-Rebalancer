namespace SHC_Rebalancer;

/// AirConfigModel
public class AirConfigModel : ConfigModel
{
    public StswObservableDictionary<AI, string> AIs { get; set; } = [];
}
