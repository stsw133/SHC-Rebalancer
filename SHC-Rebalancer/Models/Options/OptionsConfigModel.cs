namespace SHC_Rebalancer;

/// OptionsConfigModel
public class OptionsConfigModel : ConfigModel
{
    public Dictionary<string, OptionModel> Options { get; set; } = [];
}
