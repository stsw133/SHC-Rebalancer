namespace SHC_Rebalancer;

/// GoodsConfigModel
public class GoodsConfigModel : ConfigModel
{
    public Dictionary<SkirmishMode, GoodsModel> Goods { get; set; } = [];
}
