namespace SHC_Rebalancer;
public partial class FinderDataModel : StswObservableObject
{
    [StswObservableProperty] string _address = string.Empty;
    [StswObservableProperty] object? _value;
    [StswObservableProperty] bool _isInConfigFile;
    [StswObservableProperty] string? _description;
}
