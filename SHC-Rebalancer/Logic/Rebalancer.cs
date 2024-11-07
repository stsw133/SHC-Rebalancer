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

        foreach (var item in items.Buildings)
            ProcessItem(fs, reader, writer, item);

        foreach (var item in items.Resources)
            ProcessItem(fs, reader, writer, item);

        foreach (var item in items.Units)
            ProcessItem(fs, reader, writer, item);

        foreach (var item in items.Other)
            ProcessItem(fs, reader, writer, item);
    }

    /// ProcessItem
    private static void ProcessItem(FileStream fs, BinaryReader reader, BinaryWriter writer, RebalanceItemModel item)
    {
        if (item.Size == 4)
        {
            var newValue = Convert.ToInt32(item.Value);
            fs.Seek(Convert.ToInt64(item.Address, 16), SeekOrigin.Begin);

            var oldValue = reader.ReadInt32();
            if (oldValue != newValue)
            {
                Console.WriteLine($"Address: {item.Address}, old value: {oldValue}, new value: {newValue}, description: {item.Description}");

                fs.Seek(Convert.ToInt64(item.Address, 16), SeekOrigin.Begin);
                writer.Write(newValue);
            }
        }
    }

    /// IntToByteArray
    private static byte[] IntToByteArray(int value, int size)
    {
        var bytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        if (size < bytes.Length)
        {
            var truncated = new byte[size];
            Array.Copy(bytes, bytes.Length - size, truncated, 0, size);
            return truncated;
        }
        else if (size > bytes.Length)
        {
            byte[] padded = new byte[size];
            Array.Copy(bytes, 0, padded, size - bytes.Length, bytes.Length);
            return padded;
        }

        return bytes;
    }
}
