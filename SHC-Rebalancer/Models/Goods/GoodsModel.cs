namespace SHC_Rebalancer;
public class GoodsModel : StswObservableObject
{
    public GoldModel? Gold { get; set; }

    public Dictionary<Resource, int>? Resources { get; set; }

    public class GoldModel
    {
        public int[]? Human { get; set; }
        public int[]? AI { get; set; }
    }
}
