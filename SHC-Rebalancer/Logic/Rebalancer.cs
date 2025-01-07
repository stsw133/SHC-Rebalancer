using System.IO;
using System.Text;
using System.Text.Json;

namespace SHC_Rebalancer;
internal class Rebalancer
{
    private static FileStream? _fs;
    private static BinaryReader? _reader;
    private static BinaryWriter? _writer;

    /// Rebalance
    internal static void Rebalance(GameVersion gameVersion, string exePath, RebalanceModel rebalance)
    {
        using (_fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite))
        using (_reader = new BinaryReader(_fs))
        using (_writer = new BinaryWriter(_fs))
        {
            foreach (var item in rebalance.BuildingsView)
                ProcessBuildingValues(gameVersion, item);
            foreach (var item in rebalance.ResourcesView)
                ProcessResourceValues(gameVersion, item);
            //ProcessValues(gameVersion, rebalance.SkirmishTrail);
            foreach (var item in rebalance.UnitsView)
                ProcessUnitValues(gameVersion, item);
            
            ProcessOtherAddress(gameVersion, rebalance.Other.Where(x => x.Version.In(null, gameVersion)));
        }
    }

    /// ProcessBuildingValues
    private static void ProcessBuildingValues(GameVersion gameVersion, BuildingModel model)
    {
        /// health
        if (model.Health.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Health", out var baseAddress))
        {
            var address = GetAddress<Building>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.Health, baseAddress.Size, $"{model.Key} Health");
        }
        /// housing
        if (model.Housing.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Housing", out baseAddress))
        {
            var address = GetAddress<Building>(baseAddress, model.Key.ToString(), 4);
            WriteIfDifferent(address, model.Housing, baseAddress.Size, $"{model.Key} Housing");
        }
        /// cost
        if (model.Cost?.Length > 0 && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Cost", out baseAddress))
        {
            var address = GetAddress<Building>(baseAddress, model.Key.ToString(), model.Cost.Length);
            WriteIfDifferent(address, model.Cost, model.Cost.Length, $"{model.Key} Cost");
        }
    }

    /// ProcessResourceValues
    private static void ProcessResourceValues(GameVersion gameVersion, ResourceModel model)
    {
        /// buy
        if (model.Buy.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Resources Buy", out var baseAddress))
        {
            var address = GetAddress<Resource>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.Buy, baseAddress.Size, $"{model.Key} Buy");
        }
        /// sell
        if (model.Sell.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Resources Sell", out baseAddress))
        {
            var address = GetAddress<Resource>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.Sell, baseAddress.Size, $"{model.Key} Sell");
        }
    }
    
    /// ProcessSkirmishMissionValues
    private static void ProcessSkirmishMissionValues(GameVersion gameVersion, string key, SkirmishMissionModel mission)
    {
        if (Storage.BaseAddresses[gameVersion].TryGetValue("SkirmishTrail Mission", out var baseAddress))
        {
            var i = Convert.ToInt32(key) - 1;
            var address = Convert.ToInt32(baseAddress.Address, 16) + (i * 144);

            if (!string.IsNullOrWhiteSpace(mission.MapNameAddress) && !string.IsNullOrWhiteSpace(mission.MapName))
            {
                if (Storage.BaseAddresses[gameVersion].TryGetValue("SkirmishTrail MapNameOffset", out var mapNameOffset))
                {
                    WriteIfDifferent(address + 0, Convert.ToInt32(mission.MapNameAddress, 16) + Convert.ToInt32(mapNameOffset.Address, 16), baseAddress.Size, $"Mission {i + 1}, MapNameOffset");
                    WriteIfDifferent(Convert.ToInt32(mission.MapNameAddress, 16), ConvertStringToBytesWithAutoPadding(mission.MapName, 4), mapNameOffset.Size, $"Mission {i + 1}, MapName");
                }
            }
            WriteIfDifferent(address + 4, mission.Difficulty, baseAddress.Size, $"Mission {i + 1}, Difficulty");
            WriteIfDifferent(address + 8, (int?)mission.Type, baseAddress.Size, $"Mission {i + 1}, Type");
            WriteIfDifferent(address + 12, mission.AIs?.Length, baseAddress.Size, $"Mission {i + 1}, NumberOfPlayers");
            WriteIfDifferent(address + 16, mission.AIs?.Concat(Enumerable.Repeat(0, 8 - mission.AIs.Length))?.ToArray(), baseAddress.Size, $"Mission {i + 1}, AIs");
            WriteIfDifferent(address + 48, mission.Locations?.Concat(Enumerable.Repeat(0, 8 - mission.Locations.Length))?.ToArray(), baseAddress.Size, $"Mission {i + 1}, Locations");
            WriteIfDifferent(address + 80, mission.Teams?.Concat(Enumerable.Repeat(0, 8 - mission.Teams.Length))?.ToArray(), baseAddress.Size, $"Mission {i + 1}, Teams");
            WriteIfDifferent(address + 112, mission.AIVs?.Concat(Enumerable.Repeat(0, 8 - mission.AIVs.Length))?.ToArray(), baseAddress.Size, $"Mission {i + 1}, AIVs");
        }
    }
    
    /// ProcessUnitValues
    private static void ProcessUnitValues(GameVersion gameVersion, UnitModel model)
    {
        /// speed
        if (model.Speed.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units Speed", out var baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.Speed, baseAddress.Size, $"{model.Key} Speed");
        }
        /// canGoOnWall
        if (model.CanGoOnWall.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units CanGoOnWall", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.CanGoOnWall.Value ? 1 : 0, baseAddress.Size, $"{model.Key} CanGoOnWall");
        }
        /// canBeMoved
        if (model.CanBeMoved.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units CanBeMoved", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.CanBeMoved.Value ? 1 : 0, baseAddress.Size, $"{model.Key} CanBeMoved");
        }
        /// health
        if (model.Health.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units Health", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.Health, baseAddress.Size, $"{model.Key} Health");
        }
        /// damageFromBow
        if (model.DamageFromBow.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromBow", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.DamageFromBow, baseAddress.Size, $"{model.Key} DamageFromBow");
        }
        /// damageFromSling
        if (model.DamageFromSling.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromSling", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.DamageFromSling, baseAddress.Size, $"{model.Key} DamageFromSling");
        }
        /// damageFromCrossbow
        if (model.DamageFromCrossbow.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromCrossbow", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.DamageFromCrossbow, baseAddress.Size, $"{model.Key} DamageFromCrossbow");
        }
        /// canMeleeDamage
        if (model.CanMeleeDamage.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units CanMeleeDamage", out baseAddress))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.CanMeleeDamage.Value ? 1 : 0, baseAddress.Size, $"{model.Key} CanMeleeDamage");
        }
        /// meleeDamage
        if (model.MeleeDamage.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Units MeleeDamage", out baseAddress))
        {
            var opponents = Enum.GetValues<Unit>();
            for (var i = 0; i < opponents.Length; i++)
            {
                if (model.MeleeDamageVs.ContainsKey(opponents[i]) || opponents[i].ToString().StartsWith("Unknown"))
                    continue;

                var address = GetAddress<Unit>(baseAddress, model.Key.ToString(), opponents.Length) + (i * baseAddress.Size);
                WriteIfDifferent(address, model.MeleeDamage, baseAddress.Size, $"{model.Key} MeleeDamage vs {(Unit)i}");
            }
        }
        /// meleeDamageVs
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units MeleeDamage", out baseAddress))
        {
            foreach (var meleeDamageVs in model.MeleeDamageVs)
            {
                var address = GetAddress<Unit>(baseAddress, model.Key.ToString(), Enum.GetValues<Unit>().Length) + ((int)meleeDamageVs.Key * baseAddress.Size);
                WriteIfDifferent(address, meleeDamageVs.Value, baseAddress.Size, $"{model.Key} MeleeDamage vs {meleeDamageVs.Key}");
            }
        }
        /// canClimbLadder
        if (model.CanClimbLadder.HasValue
         && Storage.BaseAddresses[gameVersion].TryGetValue("Units CanClimbLadder", out baseAddress)
         && Storage.BaseAddresses[gameVersion].TryGetValue("Units FocusClimbLadder", out var baseAddress2))
        {
            var address = GetAddress<Unit>(baseAddress, model.Key.ToString());
            WriteIfDifferent(address, model.CanClimbLadder.Value ? 1 : 0, baseAddress.Size, $"{model.Key} CanClimbLadder");

            address = GetAddress<Unit>(baseAddress2, model.Key.ToString());
            WriteIfDifferent(address, model.CanClimbLadder.Value ? 1 : 0, baseAddress2.Size, $"{model.Key} FocusClimbLadder");
        }
    }
    
    /// ProcessOtherAddress
    private static void ProcessOtherAddress(GameVersion gameVersion, IEnumerable<BaseValueModel> items)
    {
        foreach (var item in items)
        {
            Storage.BaseAddresses[gameVersion].TryGetValue(item.Key, out var baseAddress);

            var address = Convert.ToInt32(item.Address ?? baseAddress?.Address, 16);
            if (address == default)
                continue;

            if (item.Value is JsonElement jsonValue)
            {
                if (jsonValue.ValueKind == JsonValueKind.Array)
                {
                    if (item.Size == 1)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetByte()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description);
                    }
                    else if (item.Size == 4)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description);
                    }
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    if (item.Size == 1)
                        WriteIfDifferent(address, (byte)intValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description);
                    else if (item.Size == 4)
                        WriteIfDifferent(address, intValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description);
                }
            }
        }
    }

    /// GetAddress
    private static int GetAddress<T>(BaseAddressModel baseAddress, string key, int skipBy = 1) where T : Enum
    {
        try
        {
            return Convert.ToInt32(baseAddress.Address, 16) + ((int)Enum.Parse(typeof(T), key) * baseAddress.Size * skipBy);
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
        if (address == default || newValue == null)
            return;

        _fs!.Seek(address, SeekOrigin.Begin);

        T oldValue = newValue switch
        {
            byte => (T)(object)_reader!.ReadByte(),
            int => size == 1 ? (T)(object)(int)_reader!.ReadByte() : (T)(object)_reader!.ReadInt32(),
            byte[] byteArray => (T)(object)_reader!.ReadBytes(byteArray.Length),
            int[] intArray => (T)(object)Enumerable.Range(0, intArray.Length).Select(_ => _reader!.ReadInt32()).ToArray(),
            _ => throw new InvalidOperationException($"Unsupported type {typeof(T).Name}")
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

            if (newValue is byte byteValue)
            {
                _writer!.Write(byteValue);
            }
            else if (newValue is int intValue)
            {
                _writer!.Write(intValue);
            }
            else if (newValue is byte[] byteArray)
            {
                foreach (var x in byteArray)
                    _writer!.Write(x);
            }
            else if (newValue is int[] intArray)
            {
                foreach (var x in intArray)
                    _writer!.Write(x);
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

    /// ConvertStringToBytesWithAutoPadding
    private static byte[] ConvertStringToBytesWithAutoPadding(string input, int alignment)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (alignment <= 0)
            throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be greater than 0.");

        var stringBytes = Encoding.ASCII.GetBytes(input);

        var requiredLength = stringBytes.Length + 1;
        var totalLength = ((requiredLength + alignment - 1) / alignment) * alignment;

        var result = new byte[totalLength];
        Array.Copy(stringBytes, result, stringBytes.Length);

        return result;
    }
}
