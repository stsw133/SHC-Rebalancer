namespace SHC_Rebalancer;
public class RebalanceHeaderModel
{
    public IEnumerable<RebalanceItemModel> Buildings { get; set; } = [];
    public IEnumerable<RebalanceItemModel> Resources { get; set; } = [];
    public IEnumerable<RebalanceItemModel> Units { get; set; } = [];
    public IEnumerable<RebalanceItemModel> Other { get; set; } = [];
}
