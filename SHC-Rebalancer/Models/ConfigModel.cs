using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

/// ConfigModel
public abstract class ConfigModel : StswObservableObject
{
    [JsonIgnore]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name = string.Empty;

    public string? Description { get; set; }
}
