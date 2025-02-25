using System.Collections.ObjectModel;

namespace SHC_Rebalancer;

/// CustomsConfigModel
public class CustomsConfigModel : ConfigModel
{
    public ObservableCollection<OtherValueModel> Values { get; set; } = [];
}
