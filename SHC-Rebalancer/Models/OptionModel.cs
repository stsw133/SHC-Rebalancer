namespace SHC_Rebalancer;
public class OptionModel
{
    public string? Description { get; set; }
    public IEnumerable<OptionValueModel> Modifications { get; set; } = [];

    public class OptionValueModel
    {
        public GameVersion Version { get; set; }
        public string? Address { get; set; }
        public string? EndAddress { get; set; }
        public int Size { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public bool IsNewValueDynamic { get; set; }
        public int? AddToValue { get; set; }
        public int? MultiplyValueBy { get; set; }
    }
}
