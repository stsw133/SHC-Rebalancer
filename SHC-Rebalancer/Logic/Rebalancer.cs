using System.IO;
using System.Text.Json;

namespace SHC_Rebalancer;
internal class Rebalancer
{
    /// Rebalance
    internal static void Rebalance(string exePath, string configPath)
    {
        if (!File.Exists(exePath) || !File.Exists(configPath))
            return;

        var json = File.ReadAllText(configPath);
        var items = JsonSerializer.Deserialize<RebalanceHeaderModel>(json, new JsonSerializerOptions() { AllowTrailingCommas = true })!;

        using var fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite);
        using var reader = new BinaryReader(fs);
        using var writer = new BinaryWriter(fs);

        foreach (var category in new[] { items.Buildings, items.Resources, items.Units, items.Other })
            ProcessItems(fs, reader, writer, category);
    }

    /// ProcessItems
    private static void ProcessItems(FileStream fs, BinaryReader reader, BinaryWriter writer, IEnumerable<RebalanceItemModel> items)
    {
        foreach (var item in items)
        {
            var address = Convert.ToInt32(item.Address, 16);
            fs.Seek(address, SeekOrigin.Begin);

            if (item.Value is not JsonElement jsonValue)
                continue;

            if (jsonValue.ValueKind == JsonValueKind.Array)
            {
                var newValue = jsonValue.EnumerateArray().Select(x => (byte)x.GetInt32()).ToArray();
                var oldValue = reader.ReadBytes(newValue.Length);
                UpdateIfDifferent(writer, address, newValue, oldValue, item.Description);
            }
            else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.GetInt32() is int intValue)
            {
                if (item.Size == 1)
                {
                    var newValue = Convert.ToByte(intValue);
                    var oldValue = reader.ReadByte();
                    UpdateIfDifferent(writer, address, newValue, oldValue, item.Description);
                }
                if (item.Size == 4)
                {
                    var newValue = Convert.ToInt32(intValue);
                    var oldValue = reader.ReadInt32();
                    UpdateIfDifferent(writer, address, newValue, oldValue, item.Description);
                }
            }
        }
    }
    
    /// UpdateIfDifferent
    private static void UpdateIfDifferent(BinaryWriter writer, int address, byte[] newValue, byte[] oldValue, string? description)
    {
        if (!newValue.SequenceEqual(oldValue))
        {
            Console.WriteLine($"Address: {address:X}, old bytes: {BitConverter.ToString(oldValue)}, new bytes: {BitConverter.ToString(newValue)}, description: {description}");
            writer.Seek(address, SeekOrigin.Begin);
            writer.Write(newValue);
        }
    }
    
    /// UpdateIfDifferent
    private static void UpdateIfDifferent(BinaryWriter writer, int address, byte newValue, byte oldValue, string? description)
    {
        if (oldValue != newValue)
        {
            Console.WriteLine($"Address: {address:X}, old value: {oldValue}, new value: {newValue}, description: {description}");
            writer.Seek(address, SeekOrigin.Begin);
            writer.Write(newValue);
        }
    }

    /// UpdateIfDifferent
    private static void UpdateIfDifferent(BinaryWriter writer, int address, int newValue, int oldValue, string? description)
    {
        if (oldValue != newValue)
        {
            Console.WriteLine($"Address: {address:X}, old value: {oldValue}, new value: {newValue}, description: {description}");
            writer.Seek(address, SeekOrigin.Begin);
            writer.Write(newValue);
        }
    }
}
