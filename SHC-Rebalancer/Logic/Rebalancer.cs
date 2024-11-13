using System.IO;
using System.Text.Json;

namespace SHC_Rebalancer;
internal class Rebalancer
{
    private static FileStream? _fs;
    private static BinaryReader? _reader;
    private static BinaryWriter? _writer;

    /// Rebalance
    internal static void Rebalance(string exePath, string configPath)
    {
        if (!File.Exists(exePath) || !File.Exists(configPath))
            return;

        var json = File.ReadAllText(configPath);
        var configData = JsonSerializer.Deserialize<ConfigDataModel>(json, new JsonSerializerOptions { AllowTrailingCommas = true })!;

        using (_fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite))
        using (_reader = new BinaryReader(_fs))
        using (_writer = new BinaryWriter(_fs))
        {
            var baseAddresses = configData.BaseAddresses.ToDictionary(x => x.Description, x => x);

            ProcessValues(configData.Buildings, baseAddresses);
            ProcessValues(configData.Resources, baseAddresses);
            ProcessValues(configData.Units, baseAddresses);
            ProcessOtherAddress(configData.Other);
        }
    }

    /// ProcessValues
    private static void ProcessValues<T>(Dictionary<string, T> data, Dictionary<string, AddressModel> baseAddresses)
    {
        foreach (var item in data)
        {
            if (item.Value is BuildingDataModel building)
                ProcessBuildingValues(baseAddresses, item.Key, building);
            else if (item.Value is ResourceDataModel resource)
                ProcessResourceValues(baseAddresses, item.Key, resource);
            else if (item.Value is UnitDataModel unit)
                ProcessUnitValues(baseAddresses, item.Key, unit);
        }
    }

    /// ProcessBuildingValues
    private static void ProcessBuildingValues(Dictionary<string, AddressModel> baseAddresses, string key, BuildingDataModel building)
    {
        /// health
        if (building.Health.HasValue && baseAddresses.TryGetValue("Buildings Health", out var baseAddress))
        {
            var address = GetAddress<Building>(baseAddress, key);
            WriteIfDifferent(address, building.Health.Value, baseAddress.Size, $"{key} Health");
        }
        /// cost
        if (building.Cost != null && building.Cost.Length > 0 && baseAddresses.TryGetValue("Buildings Cost", out baseAddress))
        {
            var address = GetAddress<Building>(baseAddress, key, building.Cost.Length);
            WriteIfDifferent(address, building.Cost, building.Cost.Length, $"{key} Cost");
        }
    }

    /// ProcessResourceValues
    private static void ProcessResourceValues(Dictionary<string, AddressModel> baseAddresses, string key, ResourceDataModel resource)
    {
        /// buy
        if (resource.Buy.HasValue && baseAddresses.TryGetValue("Resources Buy", out var baseAddress))
        {
            var address = GetAddress<Resource>(baseAddress, key);
            WriteIfDifferent(address, resource.Buy.Value, baseAddress.Size, $"{key} Buy");
        }
        /// sell
        if (resource.Sell.HasValue && baseAddresses.TryGetValue("Resources Sell", out baseAddress))
        {
            var address = GetAddress<Resource>(baseAddress, key);
            WriteIfDifferent(address, resource.Sell.Value, baseAddress.Size, $"{key} Sell");
        }
    }

    /// ProcessUnitValues
    private static void ProcessUnitValues(Dictionary<string, AddressModel> baseAddresses, string key, UnitDataModel unit)
    {
        /// speed
        if (unit.Speed.HasValue && baseAddresses.TryGetValue("Units Speed", out var baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, key);
            WriteIfDifferent(address, unit.Speed.Value, baseAddress.Size, $"{key} Speed");
        }
        /// health
        if (unit.Health.HasValue && baseAddresses.TryGetValue("Units Health", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, key);
            WriteIfDifferent(address, unit.Health.Value, baseAddress.Size, $"{key} Health");
        }
        /// damageFromBow
        if (unit.DamageFromBow.HasValue && baseAddresses.TryGetValue("Units DamageFromBow", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, key);
            WriteIfDifferent(address, unit.DamageFromBow.Value, baseAddress.Size, $"{key} DamageFromBow");
        }
        /// damageFromCrossbow
        if (unit.DamageFromCrossbow.HasValue && baseAddresses.TryGetValue("Units DamageFromCrossbow", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, key);
            WriteIfDifferent(address, unit.DamageFromCrossbow.Value, baseAddress.Size, $"{key} DamageFromCrossbow");
        }
        /// damageFromSling
        if (unit.DamageFromSling.HasValue && baseAddresses.TryGetValue("Units DamageFromSling", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, key);
            WriteIfDifferent(address, unit.DamageFromSling.Value, baseAddress.Size, $"{key} DamageFromSling");
        }
        /// meleeDamage
        var opponents = Enum.GetNames(typeof(Unit));

        if (unit.MeleeDamage.HasValue && baseAddresses.TryGetValue("Units MeleeDamage", out baseAddress))
        {
            for (var i = 0; i < opponents.Length; i++)
            {
                if (unit.MeleeDamageVs.ContainsKey(opponents[i]) || opponents[i].StartsWith("Unknown"))
                    continue;

                var address = GetAddress<Unit>(baseAddress, key, opponents.Length) + (i * baseAddress.Size);
                WriteIfDifferent(address, unit.MeleeDamage.Value, baseAddress.Size, $"{key} MeleeDamage vs {(Unit)i}");
            }
        }
        /// meleeDamageVs

        if (baseAddresses.TryGetValue("Units MeleeDamage", out baseAddress))
        {
            foreach (var meleeDamageVs in unit.MeleeDamageVs)
            {
                var i = (int)Enum.Parse(typeof(Unit), meleeDamageVs.Key);

                var address = GetAddress<Unit>(baseAddress, key, opponents.Length) + (i * baseAddress.Size);
                WriteIfDifferent(address, meleeDamageVs.Value, baseAddress.Size, $"{key} MeleeDamage vs {meleeDamageVs.Key}");
            }
        }
    }

    /// ProcessOtherAddress
    private static void ProcessOtherAddress(IEnumerable<AddressModel> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Address))
                continue;

            var address = Convert.ToInt32(item.Address, 16);

            if (item.Value is JsonElement jsonValue)
            {
                if (jsonValue.ValueKind == JsonValueKind.Array)
                {
                    var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                    WriteIfDifferent(address, newValue, item.Size, item.Description);
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    WriteIfDifferent(address, intValue, item.Size, item.Description);
                }
            }
        }
    }

    /// GetAddress
    private static int GetAddress<T>(AddressModel baseAddress, string key, int arraySize = 1) where T : Enum
    {
        try
        {
            return Convert.ToInt32(baseAddress.Address, 16) + ((int)Enum.Parse(typeof(T), key) * baseAddress.Size * arraySize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating address for {key}: {ex.Message}");
            throw;
        }
    }

    /// WriteIfDifferent
    private static void WriteIfDifferent<T>(int address, T newValue, int size, string? description)
    {
        _fs!.Seek(address, SeekOrigin.Begin);

        T oldValue = newValue switch
        {
            byte => (T)(object)_reader!.ReadByte(),
            int => size == 1 ? (T)(object)(int)_reader!.ReadByte() : (T)(object)_reader!.ReadInt32(),
            byte[] byteArray => (T)(object)_reader!.ReadBytes(byteArray.Length),
            int[] intArray => (T)(object)Enumerable.Range(0, intArray.Length).Select(_ => _reader!.ReadInt32()).ToArray(),
            _ => throw new InvalidOperationException("Unsupported type")
        };

        var areEqual = (newValue, oldValue) switch
        {
            (byte[] newArray, byte[] oldArray) => newArray.SequenceEqual(oldArray),
            (int[] newArray, int[] oldArray) => newArray.SequenceEqual(oldArray),
            _ => EqualityComparer<T>.Default.Equals(newValue, oldValue)
        };

        if (!areEqual)
        {
            Console.WriteLine($"Address {address:X}, old value: [{FormatValue(oldValue)}], new value: [{FormatValue(newValue)}], description: {description}");
            _fs!.Seek(address, SeekOrigin.Begin);

            if (newValue is int intValue)
            {
                if (size == 1)
                    _writer!.Write((byte)intValue);
                else if (size == 4)
                    _writer!.Write(intValue);
            }
            else if (newValue is byte byteValue)
            {
                _writer!.Write(byteValue);
            }
            else if (newValue is byte[] byteArray)
            {
                foreach (var value in byteArray)
                    _writer!.Write(value);
            }
            else if (newValue is int[] intArray)
            {
                foreach (var value in intArray)
                    _writer!.Write(value);
            }
            else
            {
                throw new InvalidOperationException("Unsupported type for writing.");
            }
        }
    }

    /// FormatValue
    private static string FormatValue<T>(T value) => value switch
    {
        byte[] byteArray => string.Join(", ", byteArray),
        int[] intArray => string.Join(", ", intArray),
        _ => value?.ToString() ?? string.Empty
    };
}
