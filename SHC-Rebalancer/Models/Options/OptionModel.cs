﻿namespace SHC_Rebalancer;
public class OptionModel
{
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string? Group { get; set; }
    public string? Type { get; set; }
    public IEnumerable<OptionValueModel> Modifications { get; set; } = [];

    public class OptionValueModel
    {
        public GameVersion Version { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? EndAddress { get; set; }
        public int Size { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public bool IsNewValueDynamic { get; set; }
        public int? Addend { get; set; }
        public double? Multiplier { get; set; }
    }
}
