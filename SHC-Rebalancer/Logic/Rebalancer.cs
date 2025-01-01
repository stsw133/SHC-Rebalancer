using System.IO;
using System.Text;

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
            //ProcessValues(gameVersion, rebalance.Units);
            //ProcessOtherAddress(gameVersion, rebalance.Other);
        }
    }

    /// ProcessBuildingValues
    private static void ProcessBuildingValues(GameVersion gameVersion, BuildingModel model)
    {
        /// health
        if (model.Health.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Health", out var aHealth))
        {
            var address = GetAddress<Building>(aHealth, model.Key.ToString());
            WriteIfDifferent(address, model.Health, aHealth.Size, $"{model.Key} Health");
        }
        /// housing
        if (model.Housing.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Housing", out var aHousing))
        {
            var address = GetAddress<Building>(aHousing, model.Key.ToString(), 4);
            WriteIfDifferent(address, model.Housing, aHousing.Size, $"{model.Key} Housing");
        }
        /// cost
        if (model.Cost != null && model.Cost.Length > 0 && Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Cost", out var aCost))
        {
            var address = GetAddress<Building>(aCost, model.Key.ToString(), model.Cost.Length);
            WriteIfDifferent(address, model.Cost, model.Cost.Length, $"{model.Key} Cost");
        }
    }

    /// ProcessResourceValues
    private static void ProcessResourceValues(GameVersion gameVersion, ResourceModel model)
    {
        /// buy
        if (model.Buy.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Resources Buy", out var aBuy))
        {
            var address = GetAddress<Resource>(aBuy, model.Key.ToString());
            WriteIfDifferent(address, model.Buy, aBuy.Size, $"{model.Key} Buy");
        }
        /// sell
        if (model.Sell.HasValue && Storage.BaseAddresses[gameVersion].TryGetValue("Resources Sell", out var aSell))
        {
            var address = GetAddress<Resource>(aSell, model.Key.ToString());
            WriteIfDifferent(address, model.Sell, aSell.Size, $"{model.Key} Sell");
        }
    }
    /*
    /// ProcessSkirmishMissionValues
    private static void ProcessSkirmishMissionValues(GameVersion gameVersion, string key, SkirmishMissionModel mission)
    {
        if (baseAddresses.TryGetValue("SkirmishTrail Mission", out var aMission))
        {
            var i = Convert.ToInt32(key) - 1;
            var address = Convert.ToInt32(aMission.Address, 16) + (i * 144);

            if (!string.IsNullOrWhiteSpace(mission.MapNameAddress) && !string.IsNullOrWhiteSpace(mission.MapName))
            {
                if (baseAddresses.TryGetValue("SkirmishTrail MapNameOffset", out var mapNameOffset))
                {
                    WriteIfDifferent(address + 0, Convert.ToInt32(mission.MapNameAddress, 16) + Convert.ToInt32(mapNameOffset.Address, 16), aMission.Size, $"Mission {i + 1}, MapNameOffset");
                    WriteIfDifferent(Convert.ToInt32(mission.MapNameAddress, 16), ConvertStringToBytesWithAutoPadding(mission.MapName, 4), mapNameOffset.Size, $"Mission {i + 1}, MapName");
                }
            }
            WriteIfDifferent(address + 4, mission.Difficulty, aMission.Size, $"Mission {i + 1}, Difficulty");
            WriteIfDifferent(address + 8, (int?)mission.Type, aMission.Size, $"Mission {i + 1}, Type");
            WriteIfDifferent(address + 12, mission.AIs?.Length, aMission.Size, $"Mission {i + 1}, NumberOfPlayers");
            WriteIfDifferent(address + 16, mission.AIs?.Concat(Enumerable.Repeat(0, 8 - mission.AIs.Length))?.ToArray(), aMission.Size, $"Mission {i + 1}, AIs");
            WriteIfDifferent(address + 48, mission.Locations?.Concat(Enumerable.Repeat(0, 8 - mission.Locations.Length))?.ToArray(), aMission.Size, $"Mission {i + 1}, Locations");
            WriteIfDifferent(address + 80, mission.Teams?.Concat(Enumerable.Repeat(0, 8 - mission.Teams.Length))?.ToArray(), aMission.Size, $"Mission {i + 1}, Teams");
            WriteIfDifferent(address + 112, mission.AIVs?.Concat(Enumerable.Repeat(0, 8 - mission.AIVs.Length))?.ToArray(), aMission.Size, $"Mission {i + 1}, AIVs");
        }
    }
    
    /// ProcessUnitValues
    private static void ProcessUnitValues(GameVersion gameVersion, string key, UnitModel unit)
    {
        /// speed
        if (unit.Speed.HasValue && Storage.BaseAddresses.TryGetValue("Units Speed", out var aSpeed))
        {
            var address = GetAddress<Unit>(aSpeed, key);
            WriteIfDifferent(address, unit.Speed, aSpeed.Size, $"{key} Speed");
        }
        /// canGoOnWall
        if (unit.CanGoOnWall.HasValue && baseAddresses.TryGetValue("Units CanGoOnWall", out var aCanGoOnWall))
        {
            var address = GetAddress<Unit>(aCanGoOnWall, key);
            WriteIfDifferent(address, unit.CanGoOnWall.Value ? 1 : 0, aCanGoOnWall.Size, $"{key} CanGoOnWall");
        }
        /// canBeMoved
        if (unit.CanBeMoved.HasValue && baseAddresses.TryGetValue("Units CanBeMoved", out var aCanBeMoved))
        {
            var address = GetAddress<Unit>(aCanBeMoved, key);
            WriteIfDifferent(address, unit.CanBeMoved.Value ? 1 : 0, aCanBeMoved.Size, $"{key} CanBeMoved");
        }
        /// health
        if (unit.Health.HasValue && baseAddresses.TryGetValue("Units Health", out var aHealth))
        {
            var address = GetAddress<Unit>(aHealth, key);
            WriteIfDifferent(address, unit.Health, aHealth.Size, $"{key} Health");
        }
        /// damageFromBow
        if (unit.DamageFromBow.HasValue && baseAddresses.TryGetValue("Units DamageFromBow", out var aDamageFromBow))
        {
            var address = GetAddress<Unit>(aDamageFromBow, key);
            WriteIfDifferent(address, unit.DamageFromBow, aDamageFromBow.Size, $"{key} DamageFromBow");
        }
        /// damageFromSling
        if (unit.DamageFromSling.HasValue && baseAddresses.TryGetValue("Units DamageFromSling", out var aDamageFromSling))
        {
            var address = GetAddress<Unit>(aDamageFromSling, key);
            WriteIfDifferent(address, unit.DamageFromSling, aDamageFromSling.Size, $"{key} DamageFromSling");
        }
        /// damageFromCrossbow
        if (unit.DamageFromCrossbow.HasValue && baseAddresses.TryGetValue("Units DamageFromCrossbow", out var aDamageFromCrossbow))
        {
            var address = GetAddress<Unit>(aDamageFromCrossbow, key);
            WriteIfDifferent(address, unit.DamageFromCrossbow, aDamageFromCrossbow.Size, $"{key} DamageFromCrossbow");
        }
        /// canMeleeDamage
        if (unit.CanMeleeDamage.HasValue && baseAddresses.TryGetValue("Units CanMeleeDamage", out var aCanMeleeDamage))
        {
            var address = GetAddress<Unit>(aCanMeleeDamage, key);
            WriteIfDifferent(address, unit.CanMeleeDamage.Value ? 1 : 0, aCanMeleeDamage.Size, $"{key} CanMeleeDamage");
        }
        /// meleeDamage
        var opponents = Enum.GetNames(typeof(Unit));

        if (unit.MeleeDamage.HasValue && baseAddresses.TryGetValue("Units MeleeDamage", out var aMeleeDamage))
        {
            for (var i = 0; i < opponents.Length; i++)
            {
                if (unit.MeleeDamageVs.ContainsKey(opponents[i]) || opponents[i].StartsWith("Unknown"))
                    continue;

                var address = GetAddress<Unit>(aMeleeDamage, key, opponents.Length) + (i * aMeleeDamage.Size);
                WriteIfDifferent(address, unit.MeleeDamage, aMeleeDamage.Size, $"{key} MeleeDamage vs {(Unit)i}");
            }
        }
        /// meleeDamageVs

        if (baseAddresses.TryGetValue("Units MeleeDamage", out aMeleeDamage))
        {
            foreach (var meleeDamageVs in unit.MeleeDamageVs)
            {
                var i = (int)Enum.Parse(typeof(Unit), meleeDamageVs.Key);

                var address = GetAddress<Unit>(aMeleeDamage, key, opponents.Length) + (i * aMeleeDamage.Size);
                WriteIfDifferent(address, meleeDamageVs.Value, aMeleeDamage.Size, $"{key} MeleeDamage vs {meleeDamageVs.Key}");
            }
        }
        /// canClimbLadder
        if (unit.CanClimbLadder.HasValue
         && baseAddresses.TryGetValue("Units CanClimbLadder", out var aCanClimbLadder)
         && baseAddresses.TryGetValue("Units FocusClimbLadder", out var aFocusClimbLadder))
        {
            var address = GetAddress<Unit>(aCanClimbLadder, key);
            WriteIfDifferent(address, unit.CanClimbLadder.Value ? 1 : 0, aCanClimbLadder.Size, $"{key} CanClimbLadder");
            address = GetAddress<Unit>(aFocusClimbLadder, key);
            WriteIfDifferent(address, unit.CanClimbLadder.Value ? 1 : 0, aFocusClimbLadder.Size, $"{key} FocusClimbLadder");
        }
    }
    */
    /// ProcessOtherAddress
    /*
    private static void ProcessOtherAddress(IEnumerable<BaseAddressModel> items)
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
                    if (item.Size == 1)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetByte()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size, item.Description);
                    }
                    else if (item.Size == 4)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size, item.Description);
                    }
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    if (item.Size == 1)
                        WriteIfDifferent(address, (byte)intValue, item.Size, item.Description);
                    else if (item.Size == 4)
                        WriteIfDifferent(address, intValue, item.Size, item.Description);
                }
            }
        }
    }
    */

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
