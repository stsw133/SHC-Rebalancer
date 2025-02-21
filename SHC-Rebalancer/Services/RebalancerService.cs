using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;

/// RebalancerService
internal static class RebalancerService
{
    /// Rebalance
    internal static async Task Rebalance(CancellationToken token)
    {
        foreach (var exePath in StorageService.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            try
            {
                BinaryPatchService.Open(exePath.Value);

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} UCP config..." });
                //await Task.Delay(50, CancellationToken.None);
                if (SettingsService.Instance.Settings.IncludeUCP)
                    ProcessUcp(exePath.Key);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountUcpOperations(exePath.Key) });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Options config..." });
                await Task.Delay(50, CancellationToken.None);
				if (SettingsService.Instance.Settings.IncludeOptions)
					if (StorageService.Configs["options"].Cast<IConfigModel>().FirstOrDefault() is OptionsConfigModel optionsConfig)
						ProcessOptionsConfig(exePath.Key, optionsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<OptionsConfigModel>("options") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} AIC config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["aic"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["aic"]) is AicConfigModel aicConfig)
                    ProcessAicConfig(exePath.Key, aicConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<AicConfigModel>("aic") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Goods config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["goods"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["goods"]) is GoodsConfigModel goodsConfig)
                    ProcessGoodsConfig(exePath.Key, goodsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<GoodsConfigModel>("goods") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Troops config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["troops"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["troops"]) is TroopsConfigModel troopsConfig)
                    ProcessTroopsConfig(exePath.Key, troopsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<TroopsConfigModel>("troops") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Buildings config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["buildings"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["buildings"]) is BuildingsConfigModel buildingsConfig)
                    ProcessBuildingsConfig(exePath.Key, buildingsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<BuildingsConfigModel>("buildings") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Popularity config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["popularity"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["popularity"]) is PopularityConfigModel popularityConfig)
                    ProcessPopularityConfig(exePath.Key, popularityConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<PopularityConfigModel>("popularity") });
                
                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Resources config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["resources"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["resources"]) is ResourcesConfigModel resourcesConfig)
                    ProcessResourcesConfig(exePath.Key, resourcesConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<ResourcesConfigModel>("resources") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Units config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["units"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["units"]) is UnitsConfigModel unitsConfig)
                    ProcessUnitsConfig(exePath.Key, unitsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<UnitsConfigModel>("units") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} SkirmishTrail config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["skirmishtrail"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["skirmishtrail"]) is SkirmishTrailConfigModel skirmishtrailModel)
                    ProcessSkirmishTrailConfig(exePath.Key, skirmishtrailModel);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<SkirmishTrailConfigModel>("skirmishtrail") });

                StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing {exePath.Key} Customs config..." });
                await Task.Delay(50, CancellationToken.None);
                if (StorageService.Configs["customs"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["customs"]) is CustomsConfigModel customsConfig)
                    ProcessCustomsConfig(exePath.Key, customsConfig);
                StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<CustomsConfigModel>("customs") });
            }
            finally
            {
                BinaryPatchService.Close();
            }
        }

        StswMessanger.Instance.Send(new ProgressTextMessage { Text = $"Processing AIV config..." });
        await Task.Delay(50, CancellationToken.None);
        if (StorageService.Configs["aiv"].Cast<IConfigModel>().FirstOrDefault(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs["aiv"]) is AivConfigModel aivConfig)
            ProcessAivConfig(aivConfig);
        StswMessanger.Instance.Send(new ProgressUpdateMessage { Increment = CountConfigOperations<AivConfigModel>("aiv") });
    }

    /// ProcessUcp
    private static void ProcessUcp(GameVersion gameVersion)
    {
        var upcStarterFilePath = Path.Combine(StorageService.UcpPath, $"{gameVersion}.txt");
        if (File.Exists(upcStarterFilePath))
        {
            var fileContent = File.ReadLines(upcStarterFilePath);
            var lastAddress = Convert.ToInt32(fileContent.Last().Split(',')[0], 16);
            BinaryPatchService.ExtendFileIfNeeded(lastAddress + 1);

            foreach (var line in fileContent)
            {
                var model = line.Split(',');
                var address = Convert.ToInt32(model[0], 16);
                var value = Convert.ToByte(model[1]);

                BinaryPatchService.WriteIfDifferent(address, value, 1, "UCP compatible config");
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
                var newValue = option.Type == "select" && bool.TryParse(selectedValue?.ToString(), out var boolValue)
                    ? BinaryPatchService.GetNumberOrArray(boolValue ? model.NewValue : model.OldValue, model.Size)
                    : model.IsNewValueDynamic
                        ? BinaryPatchService.GetNumberOrArray(selectedValue, model.Size)
                        : BinaryPatchService.GetNumberOrArray(model.NewValue, model.Size);

                if (int.TryParse(newValue?.ToString(), out var intValue))
                {
                    if (model.Multiplier != null)
                        newValue = (int)(intValue * model.Multiplier);
                    if (model.Addend != null)
                        newValue = (int)(intValue + model.Addend);
                }

                BinaryPatchService.WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Option {key}");
            }
        }

        /// options (groups)
        foreach (var (groupName, optionsDict) in exclusiveGroups)
        {
            foreach (var (optionKey, option) in optionsDict)
            {
                foreach (var model in option.Modifications.Where(x => x.Version == gameVersion))
                {
                    var newValue = BinaryPatchService.GetNumberOrArray(model.OldValue, model.Size);
                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Default {optionKey}");
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
                        var newValue = selectedOption.Type == "select" && bool.TryParse(selectedValue?.ToString(), out var boolValue)
                            ? BinaryPatchService.GetNumberOrArray(boolValue ? model.NewValue : model.OldValue, model.Size)
                            : model.IsNewValueDynamic
                                ? BinaryPatchService.GetNumberOrArray(SettingsService.Instance.Settings.SelectedOptions[selectedOptionKey], model.Size)
                                : BinaryPatchService.GetNumberOrArray(model.NewValue, model.Size);

                        if (int.TryParse(newValue?.ToString(), out var intValue))
                        {
                            if (model.Multiplier != null)
                                newValue = (int)(intValue * model.Multiplier);
                            if (model.Addend != null)
                                newValue = (int)(intValue + model.Addend);
                        }

                        BinaryPatchService.WriteIfDifferent(Convert.ToInt32(model.Address, 16), newValue, model.Size, $"Selected {selectedOptionKey}");
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
                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + ((int)model.Key) - 1, (byte)model.Value.LordType, baseAddress.Size, $"{model.Key} LordType");
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

                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8, dots, baseAddress.Size, $"{model.Key} LordStrength Dots");
                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + (((int)model.Key) - 1) * 8 + 4, realStrength, baseAddress.Size, $"{model.Key} LordStrength Value");
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
                    BinaryPatchService.WriteIfDifferent(address + i*10, properties[i].GetValue(model.Value.Personality), baseAddress.Size, $"{model.Key} {properties[i].Name}");
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
                    BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()) + modeAddress - 4, model.Value, baseAddress.Size, $"Goods {mode.Key} {model.Key}");
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
                        BinaryPatchService.WriteIfDifferent(modeAddress + i * 8, mode.Value.Gold.Human[i], baseAddress.Size, $"Goods {mode.Key} Gold Human {i + 1}");

                if (mode.Value.Gold?.AI != null)
                    for (var i = 0; i < 5; i++)
                        BinaryPatchService.WriteIfDifferent(modeAddress + 4 + i * 8, mode.Value.Gold.AI[i], baseAddress.Size, $"Goods {mode.Key} Gold AI {i + 1}");
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
                        BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress.Address, 16) + addressOffset, mode.Value.TryGetValue(unit, out var result) ? result : 0, baseAddress.Size, $"Troops {ai.Key} {mode.Key} {unit}");
                    }
        }
    }

    /// ProcessBuildingsConfig
    private static void ProcessBuildingsConfig(GameVersion gameVersion, BuildingsConfigModel config)
    {
        /// health
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Health", out var baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Health.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// housing
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Housing", out baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Housing.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Building>(baseAddress, model.Key.ToString()), model.Value.Housing, baseAddress.Size, $"{model.Key} Housing");

        /// cost
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Buildings Cost", out baseAddress))
            foreach (var model in config.Buildings.Where(x => x.Value.Cost?.Length > 0))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Building>(baseAddress, model.Key.ToString(), model.Value.Cost!.Length), model.Value.Cost, baseAddress.Size, $"{model.Key} Cost");

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
                    BinaryPatchService.WriteIfDifferent(address + i * 4, properties[i].GetValue(model.Value), baseAddress.Size, $"Outpost {model.Key} {properties[i].Name}");
            }
        }
    }

    /// ProcessPopularityConfig
    private static void ProcessPopularityConfig(GameVersion gameVersion, PopularityConfigModel config)
    {
        /// religion thresholds
        if (config.Popularity.ReligionThresholds?.Length == 4
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Popularity religion thresholds A", out var baseAddress)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Popularity religion thresholds B", out var baseAddress2)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Popularity religion thresholds C", out var baseAddress3)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Popularity religion thresholds D", out var baseAddress4))
        {
            var address = Convert.ToInt32(baseAddress.Address, 16);
            BinaryPatchService.WriteIfDifferent(address, config.Popularity.ReligionThresholds[0] - 1, baseAddress.Size, $"Popularity religion threshold A1");
            BinaryPatchService.WriteIfDifferent(address + 9, config.Popularity.ReligionThresholds[1] - 1, baseAddress.Size, $"Popularity religion threshold A2");
            BinaryPatchService.WriteIfDifferent(address + 21, config.Popularity.ReligionThresholds[2] - 1, baseAddress.Size, $"Popularity religion threshold A3");
            BinaryPatchService.WriteIfDifferent(address + 35, config.Popularity.ReligionThresholds[3] - 1, baseAddress.Size, $"Popularity religion threshold A4");

            address = Convert.ToInt32(baseAddress2.Address, 16);
            BinaryPatchService.WriteIfDifferent(address, config.Popularity.ReligionThresholds[0] - 1, baseAddress.Size, $"Popularity religion threshold B1");
            BinaryPatchService.WriteIfDifferent(address + 9, config.Popularity.ReligionThresholds[1] - 1, baseAddress.Size, $"Popularity religion threshold B2");
            BinaryPatchService.WriteIfDifferent(address + 21, config.Popularity.ReligionThresholds[2] - 1, baseAddress.Size, $"Popularity religion threshold B3");
            BinaryPatchService.WriteIfDifferent(address + 35, config.Popularity.ReligionThresholds[3] - 1, baseAddress.Size, $"Popularity religion threshold B4");

            address = Convert.ToInt32(baseAddress3.Address, 16);
            for (var i = 0; i < 4; i++)
            {
                BinaryPatchService.WriteIfDifferent(address + i*15, config.Popularity.ReligionThresholds[i] - 1, baseAddress.Size, $"Popularity religion threshold C{i + 1}");
                BinaryPatchService.WriteIfDifferent(address + i*15 + 7, config.Popularity.ReligionThresholds[i], baseAddress.Size, $"Popularity religion threshold C{i + 1}");
            }

            address = Convert.ToInt32(baseAddress4.Address, 16);
            BinaryPatchService.WriteIfDifferent(address, config.Popularity.ReligionThresholds[0] - 1, baseAddress.Size, $"Popularity religion threshold D1");
            BinaryPatchService.WriteIfDifferent(address + 9, config.Popularity.ReligionThresholds[1] - 1, baseAddress.Size, $"Popularity religion threshold D2");
            BinaryPatchService.WriteIfDifferent(address + 21, config.Popularity.ReligionThresholds[2] - 1, baseAddress.Size, $"Popularity religion threshold D3");
            BinaryPatchService.WriteIfDifferent(address + 35, config.Popularity.ReligionThresholds[3] - 1, baseAddress.Size, $"Popularity religion threshold D4");
        }
    }

    /// ProcessResourcesConfig
    private static void ProcessResourcesConfig(GameVersion gameVersion, ResourcesConfigModel config)
    {
        /// buy
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Resources Buy", out var baseAddress))
            foreach (var model in config.Prices.Where(x => x.Value.Buy.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Buy, baseAddress.Size, $"{model.Key} Buy");

        /// sell
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Resources Sell", out baseAddress))
            foreach (var model in config.Prices.Where(x => x.Value.Sell.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Resource>(baseAddress, model.Key.ToString()), model.Value.Sell, baseAddress.Size, $"{model.Key} Sell");
    }

    /// ProcessUnitsConfig
    private static void ProcessUnitsConfig(GameVersion gameVersion, UnitsConfigModel config)
    {
        /// cost
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units Cost", out var baseAddress))
        {
            foreach (var model in config.Units.Where(x => x.Value.Cost.HasValue && x.Key.GetAttributeOfType<UnitCostAttribute>() != null))
            {
                if (model.Key.Between(Unit.EuropArcher, Unit.Knight) || model.Key.Between(Unit.ArabArcher, Unit.Firethrower))
                    BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Troop>(baseAddress, model.Key.ToString()), model.Value.Cost, baseAddress.Size, $"{model.Key} Cost (display)");
                else if (StorageService.BaseAddresses[gameVersion].TryGetValue($"Units Cost (display) {model.Key}", out var baseAddress3))
                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress3.Address, 16), model.Value.Cost, baseAddress3.Size, $"{model.Key} Cost (display)");

                if (!model.Key.Between(Unit.EuropArcher, Unit.Knight))
                    if (StorageService.BaseAddresses[gameVersion].TryGetValue($"Units Cost (offset) {model.Key}", out var baseAddress4))
                        BinaryPatchService.WriteIfDifferent(Convert.ToInt32(baseAddress4.Address, 16), model.Value.Cost - (int)model.Key, baseAddress4.Size, $"{model.Key} Cost (offset)");
            }
        }
        
        /// speed
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units Speed", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.Speed.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Speed, baseAddress.Size, $"{model.Key} Speed");

        /// canGoOnWall
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanGoOnWall", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanGoOnWall.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanGoOnWall, baseAddress.Size, $"{model.Key} CanGoOnWall");

        /// canBeMoved
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanBeMoved", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanBeMoved.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanBeMoved, baseAddress.Size, $"{model.Key} CanBeMoved");

        /// health
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units Health", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.Health.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.Health, baseAddress.Size, $"{model.Key} Health");

        /// damageFromBow
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromBow", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromBow.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromBow, baseAddress.Size, $"{model.Key} DamageFromBow");

        /// damageFromSling
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromSling", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromSling.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromSling, baseAddress.Size, $"{model.Key} DamageFromSling");

        /// damageFromCrossbow
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units DamageFromCrossbow", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.DamageFromCrossbow.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.DamageFromCrossbow, baseAddress.Size, $"{model.Key} DamageFromCrossbow");

        /// canMeleeDamage
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanMeleeDamage", out baseAddress))
            foreach (var model in config.Units.Where(x => x.Value.CanMeleeDamage.HasValue))
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanMeleeDamage, baseAddress.Size, $"{model.Key} CanMeleeDamage");

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

                    var address = BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString(), opponents.Length) + (i * baseAddress.Size);
                    BinaryPatchService.WriteIfDifferent(address, model.Value.MeleeDamage, baseAddress.Size, $"{model.Key} MeleeDamage vs {(Unit)i}");
                }

                /// meleeDamageVs
                foreach (var meleeDamageVs in model.Value.MeleeDamageVs)
                {
                    var address = BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString(), Enum.GetValues<Unit>().Length) + ((int)meleeDamageVs.Key * baseAddress.Size);
                    BinaryPatchService.WriteIfDifferent(address, meleeDamageVs.Value, baseAddress.Size, $"{model.Key} MeleeDamage vs {meleeDamageVs.Key}");
                }
            }
        }

        /// canDigMoat
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanDigMoat", out baseAddress)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Units FocusDigMoat", out var baseAddress2))
            foreach (var model in config.Units.Where(x => x.Value.CanDigMoat.HasValue))
            {
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanDigMoat, baseAddress.Size, $"{model.Key} CanDigMoat");
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress2, model.Key.ToString()), model.Value.CanDigMoat, baseAddress2.Size, $"{model.Key} FocusDigMoat");
            }

        /// canClimbLadder
        if (StorageService.BaseAddresses[gameVersion].TryGetValue("Units CanClimbLadder", out baseAddress)
         && StorageService.BaseAddresses[gameVersion].TryGetValue("Units FocusClimbLadder", out baseAddress2))
            foreach (var model in config.Units.Where(x => x.Value.CanClimbLadder.HasValue))
            {
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress.Size, $"{model.Key} CanClimbLadder");
                BinaryPatchService.WriteIfDifferent(BinaryPatchService.GetAddressByEnum<Unit>(baseAddress2, model.Key.ToString()), model.Value.CanClimbLadder, baseAddress2.Size, $"{model.Key} FocusClimbLadder");
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
                    BinaryPatchService.WriteIfDifferent(address + 0, Convert.ToInt32(model.Value.MapNameAddress, 16) + 0x400000, baseAddress.Size, $"Mission {model.Key}, MapNameOffset");
                    BinaryPatchService.WriteIfDifferent(Convert.ToInt32(model.Value.MapNameAddress, 16), BinaryPatchService.ConvertStringToBytesWithAutoPadding(model.Value.MapName, 4), 1, $"Mission {model.Key}, MapName");
                }
                BinaryPatchService.WriteIfDifferent(address + 4, (int?)model.Value.Difficulty, baseAddress.Size, $"Mission {model.Key}, Difficulty");
                BinaryPatchService.WriteIfDifferent(address + 8, (int?)model.Value.Type, baseAddress.Size, $"Mission {model.Key}, Type");
                BinaryPatchService.WriteIfDifferent(address + 12, model.Value.NumberOfPlayers, baseAddress.Size, $"Mission {model.Key}, NumberOfPlayers");
                BinaryPatchService.WriteIfDifferent(address + 16, model.Value.AIs?.Select(x => (int)x)?.Concat(Enumerable.Repeat(0, 8 - model.Value.AIs.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, AIs");
                BinaryPatchService.WriteIfDifferent(address + 48, model.Value.Locations?.Concat(Enumerable.Repeat(0, 8 - model.Value.Locations.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, Locations");
                BinaryPatchService.WriteIfDifferent(address + 80, model.Value.Teams?.Select(x => (int)x)?.Concat(Enumerable.Repeat(0, 8 - model.Value.Teams.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, Teams");
                BinaryPatchService.WriteIfDifferent(address + 112, model.Value.AIVs?.Concat(Enumerable.Repeat(0, 8 - model.Value.AIVs.Length))?.ToArray(), baseAddress.Size, $"Mission {model.Key}, AIVs");
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
                        BinaryPatchService.WriteIfDifferent(address, newValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                    else if (item.Size == 4)
                    {
                        var newValue = jsonValue.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                        BinaryPatchService.WriteIfDifferent(address, newValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    }
                }
                else if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out int intValue))
                {
                    if (item.Size == 1)
                        BinaryPatchService.WriteIfDifferent(address, (byte)intValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                    else if (item.Size == 4)
                        BinaryPatchService.WriteIfDifferent(address, intValue, item.Size ?? StorageService.BaseAddresses[gameVersion][item.Key].Size, item.Description ?? item.Key);
                }
            }
        }
    }

    /// CountTotalOperations
    internal static int CountTotalOperations()
    {
        int operationCount = 0;

        foreach (var exePath in StorageService.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            if (SettingsService.Instance.Settings.IncludeUCP)
                operationCount += CountUcpOperations(exePath.Key);

            operationCount += CountConfigOperations<OptionsConfigModel>("options");
            operationCount += CountConfigOperations<AicConfigModel>("aic");
            operationCount += CountConfigOperations<GoodsConfigModel>("goods");
            operationCount += CountConfigOperations<TroopsConfigModel>("troops");
            operationCount += CountConfigOperations<BuildingsConfigModel>("buildings");
            operationCount += CountConfigOperations<PopularityConfigModel>("popularity");
            operationCount += CountConfigOperations<ResourcesConfigModel>("resources");
            operationCount += CountConfigOperations<UnitsConfigModel>("units");
            operationCount += CountConfigOperations<SkirmishTrailConfigModel>("skirmishtrail");
            operationCount += CountConfigOperations<CustomsConfigModel>("customs");
        }

        operationCount += CountConfigOperations<AivConfigModel>("aiv");

        return operationCount;
    }

    /// CountUcpOperations
    private static int CountUcpOperations(GameVersion gameVersion)
    {
        var upcStarterFilePath = Path.Combine(StorageService.UcpPath, $"{gameVersion}.txt");
        if (File.Exists(upcStarterFilePath))
            return File.ReadLines(upcStarterFilePath).Count() / 100;
        
        return 0;
    }

    /// CountConfigOperations
    private static int CountConfigOperations<T>(string key) where T : IConfigModel
    {
        if (!StorageService.Configs.TryGetValue(key, out var value))
        {
            Console.WriteLine($"[Error] Key '{key}' does not exists in StorageService.Configs!");
            return 0;
        }

        IConfigModel? configModel = null;

        if (key == "options")
            configModel = value.Cast<IConfigModel>().FirstOrDefault();
        else if (SettingsService.Instance.Settings.SelectedConfigs.TryGetValue(key, out var selectedConfigName))
            configModel = StorageService.Configs[key].Cast<IConfigModel>().FirstOrDefault(x => x.Name == selectedConfigName);

        if (configModel is not T config)
            return 0;

        var count = 0;
        foreach (var property in typeof(T).GetProperties())
        {
            if (property.PropertyType.GetInterface(nameof(IEnumerable<T>)) != null)
            {
                var propValue = property.GetValue(config);
                if (propValue is IDictionary dictionary)
                    count += dictionary.Count;
                else if (propValue is IEnumerable<object> list)
                    count += list.Count();
            }
        }

        return count;
    }
}

public class ProgressUpdateMessage : IStswMessage
{
    public int Increment { get; set; }
}

public class ProgressTextMessage : IStswMessage
{
    public string? Text { get; set; }
}
