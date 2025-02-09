﻿using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

/// RebalancerService
internal static class RebalancerService
{
    private static FileStream? _fs;
    private static BinaryReader? _reader;
    private static BinaryWriter? _writer;

    /// Rebalance
    internal static void Rebalance()
    {
        foreach (var exePath in StorageService.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            using (_fs = new FileStream(exePath.Value, FileMode.Open, FileAccess.ReadWrite))
            using (_reader = new BinaryReader(_fs))
            using (_writer = new BinaryWriter(_fs))
            {
                if (SettingsService.Instance.Settings.IncludeUCP)
                    ProcessUcp(exePath.Key);

                if (StorageService.Configs["options"].Cast<IConfigModel>().FirstOrDefault() is OptionsConfigModel optionsConfig)
                    ProcessOptionsConfig(exePath.Key, optionsConfig);
                
                if (StorageService.Configs["aic"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["aic"]) is AicConfigModel aicConfig)
                    ProcessAicConfig(exePath.Key, aicConfig);

                if (StorageService.Configs["goods"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["goods"]) is GoodsConfigModel goodsConfig)
                    ProcessGoodsConfig(exePath.Key, goodsConfig);

                if (StorageService.Configs["troops"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["troops"]) is TroopsConfigModel troopsConfig)
                    ProcessTroopsConfig(exePath.Key, troopsConfig);

                if (StorageService.Configs["buildings"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["buildings"]) is BuildingsConfigModel buildingsConfig)
                    ProcessBuildingsConfig(exePath.Key, buildingsConfig);
                
                if (StorageService.Configs["resources"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["resources"]) is ResourcesConfigModel resourcesConfig)
                    ProcessResourcesConfig(exePath.Key, resourcesConfig);

                if (StorageService.Configs["units"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["units"]) is UnitsConfigModel unitsConfig)
                    ProcessUnitsConfig(exePath.Key, unitsConfig);

                if (StorageService.Configs["skirmishtrail"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["skirmishtrail"]) is SkirmishTrailConfigModel skirmishtrailModel)
                    ProcessSkirmishTrailConfig(exePath.Key, skirmishtrailModel);

                if (StorageService.Configs["customs"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["customs"]) is CustomsConfigModel customsConfig)
                    ProcessCustomsConfig(exePath.Key, customsConfig);
            }
        }

        if (StorageService.Configs["aiv"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["aiv"]) is AivConfigModel aivConfig)
            ProcessAivConfig(aivConfig);
    }

    /// ProcessUcp
    private static void ProcessUcp(GameVersion gameVersion)
    {
        var upcStarterFilePath = Path.Combine(StorageService.UcpPath, $"{gameVersion}.txt");
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

    /// ProcessOptionsConfig
    private static void ProcessOptionsConfig(GameVersion gameVersion, OptionsConfigModel config)
    {
        var exclusiveGroups = config.Options
            .Where(x => !string.IsNullOrEmpty(x.Value.Group))
            .GroupBy(x => x.Value.Group!)
            .ToDictionary(g => g.Key, g => g.ToDictionary(y => y.Key, y => y.Value));

        var independentOptions = config.Options
            .Where(x => string.IsNullOrEmpty(x.Value.Group))
            .ToList();

        /// options (default)
        foreach (var (key, option) in independentOptions)
        {
            var selectedValue = SettingsService.Instance.Settings.SelectedOptions.ContainsKey(key)
                ? SettingsService.Instance.Settings.SelectedOptions[key]
                : null;

            foreach (var model in option.Modifications.Where(x => x.Version == gameVersion))
            {
                var newValue = !model.IsNewValueDynamic && bool.TryParse(selectedValue?.ToString(), out var boolValue)
                    ? GetNumberOrArray(boolValue ? model.NewValue : model.OldValue, model.Size)
                    : GetNumberOrArray(selectedValue, model.Size);

                if (int.TryParse(newValue?.ToString(), out var intValue))
                {
                    if (model.MultiplyValueBy != null)
                        newValue = (int)(intValue * model.MultiplyValueBy);
                    if (model.AddToValue != null)
                        newValue = (int)(intValue + model.AddToValue);
                }

                WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Option {key}");
            }
        }

        /// options (groups)
        foreach (var (groupName, optionsDict) in exclusiveGroups)
        {
            foreach (var (optionKey, option) in optionsDict)
            {
                foreach (var model in option.Modifications.Where(x => x.Version == gameVersion))
                {
                    var newValue = GetNumberOrArray(model.OldValue, model.Size);
                    WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Default {optionKey}");
                }
            }

            if (SettingsService.Instance.Settings.SelectedOptions.TryGetValue(groupName, out var selectedValue)
             && int.TryParse(selectedValue?.ToString(), out var selectedIndex))
            {
                var selectedOptionKey = $"{groupName}{selectedIndex}";

                if (optionsDict.TryGetValue(selectedOptionKey, out var selectedOption))
                {
                    foreach (var model in selectedOption.Modifications.Where(x => x.Version == gameVersion))
                    {
                        var newValue = !model.IsNewValueDynamic
                            ? GetNumberOrArray(model.NewValue, model.Size)
                            : GetNumberOrArray(SettingsService.Instance.Settings.SelectedOptions[selectedOptionKey], model.Size);

                        if (int.TryParse(newValue?.ToString(), out var intValue))
                        {
                            if (model.MultiplyValueBy != null)
                                newValue = (int)(intValue * model.MultiplyValueBy);
                            if (model.AddToValue != null)
                                newValue = (int)(intValue + model.AddToValue);
                        }

                        WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Selected {selectedOptionKey}");
                    }
                }
            }
        }
    }

    /// ProcessAicConfig
    private static void ProcessAicConfig(GameVersion gameVersion, AicConfigModel config)
    {
        /// lord type
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("AIC LordType", out var baseAddress))
        {
            foreach (var model in config.AIs)
                if (model.Value.LordType != null)
                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + ((int)model.Key) - 1, (byte)model.Value.LordType, baseAddress.Size, $"{model.Key} LordType");
        }

        /// lord strength
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("AIC LordStrength", out baseAddress))
        {
            foreach (var model in config.AIs)
                if (model.Value.LordStrength != null)
                {
                    var realStrength = Convert.ToInt32(model.Value.LordStrength * 100);
                    if (realStrength > 100)
                        realStrength += realStrength - 100;
                    var dots = realStrength > 100 ? Math.Min((realStrength - 100) / 20, 5) : realStrength < 100 ? Math.Min(15 - realStrength / 10, 10) : 0;

                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8, dots, baseAddress.Size, $"{model.Key} LordStrength Dots");
                    WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8 + 4, realStrength, baseAddress.Size, $"{model.Key} LordStrength Value");
                }
        }

        /// personality
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("AIC Personality", out baseAddress))
        {
            var properties = typeof(AicModel.PersonalityModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null)
                .OrderBy(p => p.MetadataToken)
                .ToList();

            foreach (var model in config.AIs.Where(x => x.Key.Between(AI.Rat, AI.Abbot)))
            {
                var address = Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key - 1) * 1697);
                for (var i = 0; i < properties.Count; i++)
                    WriteIfDifferent(address + i*10, properties[i].GetValue(model.Value.Personality), baseAddress.Size, $"{model.Key} {properties[i].Name}");
            }
        }
    }

    /// ProcessAivConfig
    private static void ProcessAivConfig(AivConfigModel config)
    {
        var sourceDirectory = Path.Combine(StorageService.ConfigsPath, "aiv", config.Name);
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"AIV directory '{config.Name}' not found.");

        Directory.CreateDirectory(StorageService.AivPath);
        foreach (var file in Directory.GetFiles(sourceDirectory, "*.aiv"))
        {
            var fileName = Path.GetFileName(file);
            var destinationPath = Path.Combine(StorageService.AivPath, fileName);

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    /// ProcessGoodsConfig
    private static void ProcessGoodsConfig(GameVersion gameVersion, GoodsConfigModel config)
    {
        /// resources
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Goods Resources", out var baseAddress))
        {
            foreach (var mode in config.Goods.Where(x => x.Value.Resources != null))
            {
                var modeAddress = (((int)mode.Key) - 1) * (Enum.GetValues<Resource>().Length - 1) * 4;
                foreach (var model in mode.Value.Resources!)
                    WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()) + modeAddress - 4, model.Value, baseAddress.Size, $"Goods {mode.Key} {model.Key}");
            }
        }

        /// gold
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Goods Gold", out baseAddress))
        {
            foreach (var mode in config.Goods.Where(x => x.Value.Gold != null))
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

    /// ProcessTroopsConfig
    private static void ProcessTroopsConfig(GameVersion gameVersion, TroopsConfigModel config)
    {
        /// troops
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Troops", out var baseAddress))
        {
            foreach (var ai in config.Troops)
                foreach (var mode in ai.Value)
                    foreach (var unit in Enum.GetValues<Troop>())
                    {
                        var addressOffset = (((int)ai.Key) - 1) * Enum.GetValues<SkirmishMode>().Length * Enum.GetValues<Troop>().Length * 4 + (((int)mode.Key) - 1) * Enum.GetValues<Troop>().Length * 4 + ((int)unit) * 4;
                        WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + addressOffset, mode.Value.TryGetValue(unit, out var result) ? result : 0, baseAddress.Size, $"Troops {ai.Key} {mode.Key} {unit}");
                    }
        }
    }

    /// ProcessBuildingsConfig
    private static void ProcessBuildingsConfig(GameVersion gameVersion, BuildingsConfigModel config)
    {
        /// health
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Health", out var baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Health.HasValue))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// housing
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Housing", out baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Housing.HasValue))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Housing, baseAddress.Size, $"{model.Key} Housing");

        /// cost
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Cost", out baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Cost?.Length > 0))
                WriteIfDifferent(GetAddressByEnum<Building>(baseAddress, model.Key.ToString(), model.Value.Cost!.Length), model.Value.Cost, model.Value.Cost.Length, $"{model.Key} Cost");

        /// outposts
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Outposts", out baseAddress))
        {
            var properties = typeof(OutpostModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute(typeof(JsonIgnoreAttribute)) == null)
                .OrderBy(p => p.MetadataToken)
                .ToList();

            foreach (var model in config.Outposts.Where(x => x.Key.Between(1, 16)))
            {
                var address = Convert.ToInt32(baseAddress.Address, 16) + ((model.Key - 1) * 52);
                for (var i = 0; i < properties.Count; i++)
                    WriteIfDifferent(address + i * 4, properties[i].GetValue(model.Value), baseAddress.Size, $"Outpost {model.Key} {properties[i].Name}");
            }
        }
    }

    /// ProcessResourcesConfig
    private static void ProcessResourcesConfig(GameVersion gameVersion, ResourcesConfigModel config)
    {
        /// buy
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Resources Buy", out var baseAddress))
            foreach (var model in config.Prices.Where(x => x.Value.Buy.HasValue))
                WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Buy, baseAddress.Size, $"{model.Key} Buy");

        /// sell
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Resources Sell", out baseAddress))
            foreach (var model in config.Prices.Where(x => x.Value.Sell.HasValue))
                WriteIfDifferent(GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Sell, baseAddress.Size, $"{model.Key} Sell");
    }

    /// ProcessUnitsConfig
    private static void ProcessUnitsConfig(GameVersion gameVersion, UnitsConfigModel config)
    {
        /// speed
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units Speed", out var baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.Speed.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Speed, baseAddress.Size, $"{model.Key} Speed");

        /// canGoOnWall
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanGoOnWall", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanGoOnWall.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanGoOnWall, baseAddress.Size, $"{model.Key} CanGoOnWall");

        /// canBeMoved
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanBeMoved", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanBeMoved.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanBeMoved, baseAddress.Size, $"{model.Key} CanBeMoved");

        /// health
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units Health", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.Health.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// damageFromBow
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromBow", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromBow.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromBow, baseAddress.Size, $"{model.Key} DamageFromBow");

        /// damageFromSling
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromSling", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromSling.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromSling, baseAddress.Size, $"{model.Key} DamageFromSling");

        /// damageFromCrossbow
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromCrossbow", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromCrossbow.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromCrossbow, baseAddress.Size, $"{model.Key} DamageFromCrossbow");

        /// canMeleeDamage
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanMeleeDamage", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanMeleeDamage.HasValue))
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanMeleeDamage, baseAddress.Size, $"{model.Key} CanMeleeDamage");

        /// meleeDamage
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units MeleeDamage", out baseAddress))
        {
            var opponents = Enum.GetValues<Unit>();
            foreach (var model in config.Units.Where(x => x.Value.MeleeDamage.HasValue))
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

        /// canDigMoat
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanDigMoat", out baseAddress)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Units FocusDigMoat", out var baseAddress2))
            foreach (var model in config.Units.Where(x => x.Value.CanDigMoat.HasValue))
            {
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanDigMoat, baseAddress.Size, $"{model.Key} CanDigMoat");
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress2, model.Key.ToString()), model.Value.CanDigMoat, baseAddress2.Size, $"{model.Key} FocusDigMoat");
            }

        /// canClimbLadder
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanClimbLadder", out baseAddress)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Units FocusClimbLadder", out baseAddress2))
            foreach (var model in config.Units.Where(x => x.Value.CanClimbLadder.HasValue))
            {
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress.Size, $"{model.Key} CanClimbLadder");
                WriteIfDifferent(GetAddressByEnum<Unit>(baseAddress2, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress2.Size, $"{model.Key} FocusClimbLadder");
            }
    }

    /// ProcessSkirmishTrailConfig
    private static void ProcessSkirmishTrailConfig(GameVersion gameVersion, SkirmishTrailConfigModel config)
    {
        /// missions
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("SkirmishTrail Missions", out var baseAddress))
            foreach (var model in config.Missions.Where(x => x.Key.Between(1, 80)))
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

    /// ProcessCustomsConfig
    private static void ProcessCustomsConfig(GameVersion gameVersion, CustomsConfigModel config)
    {
        foreach (var item in config.Values.Where(x => x.IsEnabled && x.Version.In(null, gameVersion)))
        {
            StorageService.BaseAddresses[gameVersion].TryGetValue(item.Key, out var baseAddress);

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
                        WriteIfDifferent(address, newValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                    else if (item.Size == 4)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        WriteIfDifferent(address, newValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    if (item.Size == 1)
                        WriteIfDifferent(address, (byte)intValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    else if (item.Size == 4)
                        WriteIfDifferent(address, intValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
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
        var oldValue = ReadValue(newValue, size);
        
        if (!AreValuesEqual(newValue, oldValue))
        {
            Console.WriteLine($"Address {address:X}, old value: [{FormatValue(oldValue)}], new value: [{FormatValue(newValue)}], description: {description}");
            _fs.Seek(address, SeekOrigin.Begin);
            WriteValue(newValue, size);
        }
        /*
        T oldValue = newValue switch
        {
            bool => (T)(object)_reader!.ReadBoolean(),
            byte => (T)(object)_reader!.ReadByte(),
            short => (T)(object)_reader!.ReadInt16(),
            int => size == 1 ? (T)(object)(int)_reader!.ReadByte() : size == 2 ? (T)(object)(int)_reader!.ReadInt16() : (T)(object)_reader!.ReadInt32(),
            byte[] byteArray => (T)(object)_reader!.ReadBytes(byteArray.Length),
            short[] shortArray => (T)(object)Enumerable.Range(0, shortArray.Length).Select(_ => _reader!.ReadInt16()).ToArray(),
            int[] intArray => (T)(object)Enumerable.Range(0, intArray.Length).Select(_ => _reader!.ReadInt32()).ToArray(),
            string str => (T)(object)new string(_reader!.ReadChars(str.Length)),
            _ when typeof(T).IsEnum || (typeof(T) == typeof(object) && newValue?.GetType().IsEnum == true) => (T)(object)_reader!.ReadInt32(),
            _ when Nullable.GetUnderlyingType(typeof(T))?.IsEnum == true => (T)(object)_reader!.ReadInt32(),
            object obj => size == 1 ? (T)(object)(int)_reader!.ReadByte() : size == 2 ? (T)(object)(int)_reader!.ReadInt16() : (T)(object)_reader!.ReadInt32(),
            _ => throw new InvalidOperationException($"Unsupported type {typeof(T).Name}")
        };
        
        var areEqual = (newValue, oldValue) switch
        {
            (byte[] newArray, byte[] oldArray) => newArray.SequenceEqual(oldArray),
            (short[] newArray, short[] oldArray) => newArray.SequenceEqual(oldArray),
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
            else if (newValue is short shortValue)
                _writer!.Write(shortValue);
            else if (newValue is int intValue)
                _writer!.Write(intValue);
            else if (newValue is byte[] byteArray)
                byteArray.ToList().ForEach(x => _writer!.Write(x));
            else if (newValue is short[] shortArray)
                shortArray.ToList().ForEach(x => _writer!.Write(x));
            else if (newValue is int[] intArray)
                intArray.ToList().ForEach(x => _writer!.Write(x));
            else if (newValue is string strValue)
                strValue.ToList().ForEach(c => _writer!.Write((byte)c));
            else if (newValue?.GetType().IsEnum == true)
                _writer!.Write(Convert.ToInt32(newValue));
            else
                throw new InvalidOperationException("Unsupported type for writing.");
        }
        */
    }

    /// ReadValue
    private static object ReadValue(object newValue, int size)
    {
        if (newValue is bool)
            return _reader!.ReadByte() != 0;
        if (newValue is byte)
            return _reader!.ReadByte();
        if (newValue is short)
            return _reader!.ReadInt16();
        if (newValue is int)
        {
            return size switch
            {
                1 => _reader!.ReadByte(),
                2 => _reader!.ReadInt16(),
                _ => _reader!.ReadInt32()
            };
        }
        if (newValue is byte[] byteArr)
            return _reader!.ReadBytes(byteArr.Length);
        if (newValue is short[] shortArr)
        {
            var arr = new short[shortArr.Length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = _reader!.ReadInt16();
            return arr;
        }
        if (newValue is int[] intArr)
        {
            var arr = new int[intArr.Length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = size == 2 ? (int)_reader!.ReadInt16() : _reader!.ReadInt32();
            return arr;
        }
        if (newValue is string str)
            return new string(_reader!.ReadChars(str.Length));
        if (newValue.GetType().IsEnum)
            return _reader!.ReadInt32();
        if (newValue is double)
        {
            return size switch
            {
                1 => _reader!.ReadByte(),
                2 => _reader!.ReadInt16(),
                _ => _reader!.ReadInt32()
            };
        }

        throw new InvalidOperationException($"Unsupported type {newValue.GetType().Name}");
    }

    /// WriteValue
    private static void WriteValue(object newValue, int size)
    {
        if (newValue is bool b)
            _writer!.Write((byte)(b ? 1 : 0));
        else if (newValue is byte by)
            _writer!.Write(by);
        else if (newValue is short s)
            _writer!.Write(s);
        else if (newValue is int i)
        {
            switch (size)
            {
                case 1:
                    _writer!.Write((byte)i);
                    break;
                case 2:
                    _writer!.Write((short)i);
                    break;
                default:
                    _writer!.Write(i);
                    break;
            }
        }
        else if (newValue is byte[] byteArr)
            _writer!.Write(byteArr);
        else if (newValue is short[] shortArr)
        {
            foreach (var s1 in shortArr)
                _writer!.Write(s1);
        }
        else if (newValue is int[] intArr)
        {
            if (size == 2)
            {
                foreach (var i1 in intArr)
                    _writer!.Write((short)i1);
            }
            else
            {
                foreach (var i2 in intArr)
                    _writer!.Write(i2);
            }
        }
        else if (newValue is string str)
        {
            foreach (var c in str)
                _writer!.Write((byte)c);
        }
        else if (newValue.GetType().IsEnum)
        {
            _writer!.Write(Convert.ToInt32(newValue));
        }
        else if (newValue is double d)
        {
            switch (size)
            {
                case 1:
                    _writer!.Write((byte)d);
                    break;
                case 2:
                    _writer!.Write((short)d);
                    break;
                default:
                    _writer!.Write(d);
                    break;
            }
        }
        else
        {
            throw new InvalidOperationException("Unsupported type for writing.");
        }
    }

    /// AreValuesEqual
    private static bool AreValuesEqual(object newValue, object oldValue)
    {
        return (newValue, oldValue) switch
        {
            (byte[] newArr, byte[] oldArr) => newArr.SequenceEqual(oldArr),
            (short[] newArr, short[] oldArr) => newArr.SequenceEqual(oldArr),
            (int[] newArr, int[] oldArr) => newArr.SequenceEqual(oldArr),
            (string newStr, string oldStr) => newStr == oldStr,
            _ when newValue.GetType().IsEnum || oldValue.GetType().IsEnum => Convert.ToInt32(newValue) == Convert.ToInt32(oldValue),
            _ => Equals(newValue, oldValue)
        };
    }

    /// FormatValue
    private static string FormatValue<T>(T value) => value switch
    {
        byte[] byteArray => string.Join(", ", byteArray),
        short[] shortArray => string.Join(", ", shortArray),
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

    /// GetNumberOrArray
    private static object GetNumberOrArray(object? obj, int size)
    {
        if (obj is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                var number = jsonElement.GetInt32();
                return size switch
                {
                    1 => (byte)number,
                    2 => (short)number,
                    _ => number
                };
            }
            if (jsonElement.ValueKind == JsonValueKind.String && int.TryParse(jsonElement.GetString(), out int parsedValue))
            {
                return size switch
                {
                    1 => (byte)parsedValue,
                    2 => (short)parsedValue,
                    _ => parsedValue
                };
            }
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var numbers = jsonElement.EnumerateArray()
                                         .Where(e => e.ValueKind == JsonValueKind.Number)
                                         .Select(e => e.GetInt32())
                                         .ToArray();
                return size switch
                {
                    1 => numbers.Select(n => (byte)n).ToArray(),
                    2 => numbers.Select(n => (short)n).ToArray(),
                    _ => numbers
                };
            }
        }
        else if (obj is int intValue)
        {
            return size switch
            {
                1 => (byte)intValue,
                2 => (short)intValue,
                _ => intValue
            };
        }
        else if (obj is double doubleValue)
        {
            return size switch
            {
                1 => (byte)doubleValue,
                2 => (short)doubleValue,
                _ => (int)doubleValue
            };
        }
        else if (obj is string strValue && int.TryParse(strValue, out int parsedStrValue))
        {
            return size switch
            {
                1 => (byte)parsedStrValue,
                2 => (short)parsedStrValue,
                _ => parsedStrValue
            };
        }
        else if (obj is Array array)
        {
            var numbers = array.OfType<object>().Select(Convert.ToInt32).ToArray();
            return size switch
            {
                1 => numbers.Select(n => (byte)n).ToArray(),
                2 => numbers.Select(n => (short)n).ToArray(),
                _ => numbers
            };
        }

        throw new InvalidOperationException($"Unsupported value type: {obj?.GetType().Name}");
    }
}
