using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
internal static class Rebalancer
{
    private static FileStream? _fs;
    private static BinaryReader? _reader;
    private static BinaryWriter? _writer;

    /// Rebalance
    internal static void Rebalance()
    {
        foreach (var exePath in Storage.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            using (_fs = new FileStream(exePath.Value, FileMode.Open, FileAccess.ReadWrite))
            using (_reader = new BinaryReader(_fs))
            using (_writer = new BinaryWriter(_fs))
            {
                if (Settings.Default.IncludeUcp)
                    ProcessUcp(exePath.Key);

                if (Storage.Configs["aic"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_aic) is AicConfigModel aicConfig)
                    ProcessAicConfig(exePath.Key, aicConfig);

                if (Storage.Configs["buildings"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_buildings) is BuildingsConfigModel buildingsConfig)
                    ProcessBuildingsConfig(exePath.Key, buildingsConfig);
                
                if (Storage.Configs["goods"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_goods) is GoodsConfigModel goodsConfig)
                    ProcessGoodsConfig(exePath.Key, goodsConfig);

                if (Storage.Configs["resources"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_resources) is ResourcesConfigModel resourcesConfig)
                    ProcessResourcesConfig(exePath.Key, resourcesConfig);

                if (Storage.Configs["skirmishtrail"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_skirmishtrail) is SkirmishTrailConfigModel skirmishtrailModel)
                    ProcessSkirmishTrailConfig(exePath.Key, skirmishtrailModel);

                if (Storage.Configs["troops"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_troops) is TroopsConfigModel troopsConfig)
                    ProcessTroopsConfig(exePath.Key, troopsConfig);

                if (Storage.Configs["units"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_units) is UnitsConfigModel unitsConfig)
                    ProcessUnitsConfig(exePath.Key, unitsConfig);

                if (Storage.Configs["customs"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_customs) is CustomsConfigModel customsConfig)
                    ProcessCustomsConfig(exePath.Key, customsConfig);
            }
        }

        if (Storage.Configs["aiv"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == Settings.Default.ConfigName_aiv) is AivConfigModel aivConfig)
            ProcessAivConfig(aivConfig);
    }

    /// ProcessUcp
    private static void ProcessUcp(GameVersion gameVersion)
    {
        var upcStarterFilePath = Path.Combine(Storage.UcpPath, $"{gameVersion}.txt");
        if (File.Exists(upcStarterFilePath))
        {
            var fileContent = File.ReadLines(upcStarterFilePath);
            var lastAddress = Convert.ToInt32(fileContent.Last().Split(',')[0], 16);
            _fs?.SetLength(lastAddress + 1);

            foreach (var line in fileContent)
            {
                var model = line.Split(',');
                WriteIfDifferent(Convert.ToInt32(model[0], 16), Convert.ToByte(model[1]), 1, "ucp compatible config");
            }
        }
        else throw new FileNotFoundException("UCP config file not found!");
    }

    /// ProcessAicConfig
    private static void ProcessAicConfig(GameVersion gameVersion, AicConfigModel config)
    {
        if (Storage.BaseAddresses[gameVersion].TryGetValue("AIC", out var baseAddress))
        {
            var properties = typeof(AicModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null)
                .OrderBy(p => p.MetadataToken)
                .ToList();

            foreach (var model in config.Values.Where(x => x.Key.Between(AI.Rat, AI.Abbot)))
            {
                var address = Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key - 1) * 1697);
                for (var i = 0; i < properties.Count; i++)
                {
                    var propertyValue = properties[i].GetValue(model.Value);
                    if (propertyValue == null) continue;

                    WriteIfDifferent(address, propertyValue, baseAddress.Size, $"{model.Key} {properties[i].Name}");
                    address += 10;
                }
            }
        }
    }

    /// ProcessAivConfig
    private static void ProcessAivConfig(AivConfigModel config)
    {
        var sourceDirectory = Path.Combine(Storage.ConfigsPath, "aiv", config.Name);
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"AIV directory '{config.Name}' not found.");

        Directory.CreateDirectory(Storage.AivPath);
        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            var fileName = Path.GetFileName(file);
            var destinationPath = Path.Combine(Storage.AivPath, fileName);

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    /// ProcessBuildingsConfig
    private static void ProcessBuildingsConfig(GameVersion gameVersion, BuildingsConfigModel config)
    {
        /// health
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Health", out var baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Health.HasValue))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// housing
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Housing", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Housing.HasValue))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Housing, baseAddress.Size, $"{model.Key} Housing");

        /// cost
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Buildings Cost", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Cost?.Length > 0))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString(), model.Value.Cost!.Length), model.Value.Cost, model.Value.Cost.Length, $"{model.Key} Cost");
    }
    
    /// ProcessGoodsConfig
    private static void ProcessGoodsConfig(GameVersion gameVersion, GoodsConfigModel config)
    {
        /// resources
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Goods Resources", out var baseAddress))
        {
            foreach (var mode in config.Values.Where(x => x.Value.Resources != null))
            {
                var modeAddress = (((int)mode.Key) - 1) * (Enum.GetValues<Resource>().Length - 1) * 4;
                foreach (var model in mode.Value.Resources!)
                    WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()) + modeAddress - 4, model.Value, baseAddress.Size, $"Goods {mode.Key} {model.Key}");
            }
        }

        /// gold
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Goods Gold", out baseAddress))
        {
            foreach (var mode in config.Values.Where(x => x.Value.Gold != null))
            {
                var modeAddress = Convert.ToInt32(baseAddress.Address, 16) + (((int)mode.Key) - 1) * 40;

                if (mode.Value.Gold?.Human != null)
                    for (var i = 0; i < 5; i++)
                        WriteIfDifferent(modeAddress + i * 8, mode.Value.Gold.Human[i], baseAddress.Size, $"Goods {mode.Key} Gold Human {i + 1}");

                if (mode.Value.Gold?.AI != null)
                    for (var i = 0; i < 5; i++)
                        WriteIfDifferent(modeAddress + 4 + i * 8, mode.Value.Gold.AI[i], baseAddress.Size, $"Goods {mode.Key} Gold AI {i + 1}");
            }
        }
    }
    
    /// ProcessResourcesConfig
    private static void ProcessResourcesConfig(GameVersion gameVersion, ResourcesConfigModel config)
    {
        /// buy
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Resources Buy", out var baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Buy.HasValue))
                WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Buy, baseAddress.Size, $"{model.Key} Buy");

        /// sell
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Resources Sell", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Sell.HasValue))
                WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Sell, baseAddress.Size, $"{model.Key} Sell");
    }

    /// ProcessSkirmishTrailConfig
    private static void ProcessSkirmishTrailConfig(GameVersion gameVersion, SkirmishTrailConfigModel config)
    {
        if (Storage.BaseAddresses[gameVersion].TryGetValue("SkirmishTrail Missions", out var baseAddress))
            foreach (var model in config.Values.Where(x => x.Key.Between(1, 80)))
            {
                var address = Convert.ToInt32(baseAddress.Address, 16) + ((model.Key - 1) * 144);

                if (!string.IsNullOrWhiteSpace(model.Value.MapNameAddress) && !string.IsNullOrWhiteSpace(model.Value.MapName))
                {
                    WriteIfDifferent(address + 0, Convert.ToInt32(model.Value.MapNameAddress, 16) + 0x400000, baseAddress.Size, $"Mission {model.Key}, MapNameOffset");
                    WriteIfDifferent(Convert.ToInt32(model.Value.MapNameAddress, 16), ConvertStringToBytesWithAutoPadding(model.Value.MapName, 4), 4, $"Mission {model.Key}, MapName");
                }
                WriteIfDifferent(address + 4, (int?)model.Value.Difficulty, baseAddress.Size, $"Mission {model.Key}, Difficulty");
                WriteIfDifferent(address + 8, (int?)model.Value.Type, baseAddress.Size, $"Mission {model.Key}, Type");
                WriteIfDifferent(address + 12, model.Value.NumberOfPlayers, baseAddress.Size, $"Mission {model.Key}, NumberOfPlayers");
                WriteIfDifferent(address + 16, model.Value.AIs?.Select(x => (int)x)?.Concat(Enumerable.Repeat(0, 8 - model.Value.AIs.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, AIs");
                WriteIfDifferent(address + 48, model.Value.Locations?.Concat(Enumerable.Repeat(0, 8 - model.Value.Locations.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, Locations");
                WriteIfDifferent(address + 80, model.Value.Teams?.Select(x => (int)x)?.Concat(Enumerable.Repeat(0, 8 - model.Value.Teams.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, Teams");
                WriteIfDifferent(address + 112, model.Value.AIVs?.Concat(Enumerable.Repeat(0, 8 - model.Value.AIVs.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, AIVs");
            }
    }

    /// ProcessTroopsConfig
    private static void ProcessTroopsConfig(GameVersion gameVersion, TroopsConfigModel config)
    {
        /// lord type
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Troops LordType", out var baseAddress))
        {
            foreach (var model in config.Values)
                if (model.Value.Lord?.Type != null)
                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + ((int)model.Key) - 1, (byte)model.Value.Lord.Type, baseAddress.Size, $"Troops {model.Key} LordType");
        }

        /// lord strength
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Troops LordStrength", out baseAddress))
        {
            foreach (var model in config.Values)
                if (model.Value.Lord?.Strength != null)
                {
                    var realStrength = Convert.ToInt32(model.Value.Lord.Strength * 100);
                    if (realStrength > 100)
                        realStrength += realStrength - 100;
                    var dots = realStrength > 100 ? Math.Min((realStrength - 100) / 20, 5) : realStrength < 100 ? Math.Min(15 - realStrength / 10, 10) : 0;

                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8, dots, baseAddress.Size, $"Troops {model.Key} LordStrength Dots");
                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8 + 4, realStrength, baseAddress.Size, $"Troops {model.Key} LordStrength Value");
                }
        }

        /// troops
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Troops", out baseAddress))
        {
            foreach (var model in config.Values)
                if (model.Value.Troops != null)
                    foreach (var mode in model.Value.Troops)
                        foreach (var unit in Enum.GetValues<Troop>())
                        {
                            var addressOffset = (((int)model.Key) - 1) * Enum.GetValues<SkirmishMode>().Length * Enum.GetValues<Troop>().Length * 4 + (((int)mode.Key) - 1) * Enum.GetValues<Troop>().Length * 4 + ((int)unit) * 4;
                            WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + addressOffset, mode.Value.TryGetValue(unit, out var result) ? result : 0, baseAddress.Size, $"Troops {model.Key} {mode.Key} {unit}");
                        }
        }
    }

    /// ProcessUnitsConfig
    private static void ProcessUnitsConfig(GameVersion gameVersion, UnitsConfigModel config)
    {
        /// speed
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units Speed", out var baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Speed.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Speed, baseAddress.Size, $"{model.Key} Speed");

        /// canGoOnWall
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units CanGoOnWall", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.CanGoOnWall.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanGoOnWall, baseAddress.Size, $"{model.Key} CanGoOnWall");

        /// canBeMoved
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units CanBeMoved", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.CanBeMoved.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanBeMoved, baseAddress.Size, $"{model.Key} CanBeMoved");

        /// health
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units Health", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.Health.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// damageFromBow
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromBow", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.DamageFromBow.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromBow, baseAddress.Size, $"{model.Key} DamageFromBow");

        /// damageFromSling
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromSling", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.DamageFromSling.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromSling, baseAddress.Size, $"{model.Key} DamageFromSling");

        /// damageFromCrossbow
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units DamageFromCrossbow", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.DamageFromCrossbow.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromCrossbow, baseAddress.Size, $"{model.Key} DamageFromCrossbow");

        /// canMeleeDamage
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units CanMeleeDamage", out baseAddress))
            foreach (var model in config.Values.Where(x => x.Value.CanMeleeDamage.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanMeleeDamage, baseAddress.Size, $"{model.Key} CanMeleeDamage");

        /// meleeDamage
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units MeleeDamage", out baseAddress))
        {
            var opponents = Enum.GetValues<Unit>();
            foreach (var model in config.Values.Where(x => x.Value.MeleeDamage.HasValue))
            {
                for (var i = 0; i < opponents.Length; i++)
                {
                    if (model.Value.MeleeDamageVs.ContainsKey(opponents[i]))
                        continue;

                    var address = GetAddressByEnum<Unit>(baseAddress, model.Key.ToString(), opponents.Length) + (i * baseAddress.Size);
                    WriteIfDifferent(address, model.Value.MeleeDamage, baseAddress.Size, $"{model.Key} MeleeDamage vs {(Unit)i}");
                }

                /// meleeDamageVs
                foreach (var meleeDamageVs in model.Value.MeleeDamageVs)
                {
                    var address = GetAddressByEnum<Unit>(baseAddress, model.Key.ToString(), Enum.GetValues<Unit>().Length) + ((int)meleeDamageVs.Key * baseAddress.Size);
                    WriteIfDifferent(address, meleeDamageVs.Value, baseAddress.Size, $"{model.Key} MeleeDamage vs {meleeDamageVs.Key}");
                }
            }
        }

        /// canClimbLadder
        if (Storage.BaseAddresses[gameVersion].TryGetValue("Units CanClimbLadder", out baseAddress)
         && Storage.BaseAddresses[gameVersion].TryGetValue("Units FocusClimbLadder", out var baseAddress2))
            foreach (var model in config.Values.Where(x => x.Value.CanClimbLadder.HasValue))
            {
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress.Size, $"{model.Key} CanClimbLadder");
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress2, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress2.Size, $"{model.Key} FocusClimbLadder");
            }
    }

    /// ProcessCustomsConfig
    private static void ProcessCustomsConfig(GameVersion gameVersion, CustomsConfigModel config)
    {
        foreach (var item in config.Values.Where(x => x.Version.In(null, gameVersion)))
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
                        WriteIfDifferent(address, newValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                    else if (item.Size == 4)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    if (item.Size == 1)
                        WriteIfDifferent(address, (byte)intValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    else if (item.Size == 4)
                        WriteIfDifferent(address, intValue, item.Size ?? Storage.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                }
            }
        }
    }

    /// GetAddress
    private static int GetAddressByEnum<T>(BaseAddressModel baseAddress, string key, int skipBy = 1) where T : Enum
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

        if (address + size > _fs!.Length)
        {
            Console.WriteLine($"Address {address:X} is out of bounds (File Length: {_fs.Length}). Extending file...");
            _fs.SetLength(address + size);
        }
        _fs!.Seek(address, SeekOrigin.Begin);

        T oldValue = newValue switch
        {
            bool => (T)(object)_reader!.ReadBoolean(),
            byte => (T)(object)_reader!.ReadByte(),
            int => size == 1 ? (T)(object)(int)_reader!.ReadByte() : (T)(object)_reader!.ReadInt32(),
            byte[] byteArray => (T)(object)_reader!.ReadBytes(byteArray.Length),
            int[] intArray => (T)(object)Enumerable.Range(0, intArray.Length).Select(_ => _reader!.ReadInt32()).ToArray(),
            string str => (T)(object)new string(_reader!.ReadChars(str.Length)),
            _ when typeof(T).IsEnum || (typeof(T) == typeof(object) && newValue?.GetType().IsEnum == true) => (T)(object)_reader!.ReadInt32(),
            _ when Nullable.GetUnderlyingType(typeof(T))?.IsEnum == true => (T)(object)_reader!.ReadInt32(),
            _ => throw new InvalidOperationException($"Unsupported type {typeof(T).Name}")
        };

        var areEqual = (newValue, oldValue) switch
        {
            (byte[] newArray, byte[] oldArray) => newArray.SequenceEqual(oldArray),
            (int[] newArray, int[] oldArray) => newArray.SequenceEqual(oldArray),
            (string newStr, string oldStr) => newStr == oldStr,
            _ when typeof(T).IsEnum || (typeof(T) == typeof(object) && newValue?.GetType().IsEnum == true) => Convert.ToInt32(newValue) == Convert.ToInt32(oldValue),
            _ => EqualityComparer<T>.Default.Equals(newValue, oldValue)
        };

        if (!areEqual)
        {
            Console.WriteLine($"Address {address:X}, old value: [{FormatValue(oldValue)}], new value: [{FormatValue(newValue)}], description: {description}");
            _fs!.Seek(address, SeekOrigin.Begin);

            if (newValue is bool boolValue)
                _writer!.Write(Convert.ToInt32(boolValue));
            else if (newValue is byte byteValue)
                _writer!.Write(byteValue);
            else if (newValue is int intValue)
                _writer!.Write(intValue);
            else if (newValue is byte[] byteArray)
                byteArray.ToList().ForEach(x => _writer!.Write(x));
            else if (newValue is int[] intArray)
                intArray.ToList().ForEach(x => _writer!.Write(x));
            else if (newValue is string strValue)
                strValue.ToList().ForEach(c => _writer!.Write((byte)c));
            else if (newValue?.GetType().IsEnum == true)
                _writer!.Write(Convert.ToInt32(newValue));
            else
                throw new InvalidOperationException("Unsupported type for writing.");
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
