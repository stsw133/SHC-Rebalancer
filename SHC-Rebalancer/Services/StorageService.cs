using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SHC_Rebalancer;
public static class Storage
{
    static Storage()
    {
        BaseAddresses = LoadBaseAddresses();
        Configs = LoadConfigs();
    }

    public static Dictionary<GameVersion, string> ExePath => new()
    {
        { GameVersion.Crusader, Path.Combine(Settings.Default.GamePath, "Stronghold Crusader.exe") },
        { GameVersion.Extreme, Path.Combine(Settings.Default.GamePath, "Stronghold_Crusader_Extreme.exe") },
    };
    public static string AivPath => Path.Combine(Settings.Default.GamePath, "aiv");
    public static string BaseAddressesPath => Path.Combine(AppContext.BaseDirectory, "Resources", "base");
    public static string ConfigsPath => Path.Combine(AppContext.BaseDirectory, "Resources", "configs");
    public static string UcpPath => Path.Combine(AppContext.BaseDirectory, "Resources", "ucp");
    
    public static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> BaseAddresses { get; set; } = [];
    public static Dictionary<string, ObservableCollection<object>> Configs { get; set; } = [];

    /// LoadBaseAddresses
    internal static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> LoadBaseAddresses()
    {
        var baseAddresses = new Dictionary<GameVersion, Dictionary<string, BaseAddressModel>>();

        foreach (var filePath in Directory.GetFiles(BaseAddressesPath, "*.json"))
        {
            var gameVersion = Enum.Parse<GameVersion>(Path.GetFileNameWithoutExtension(filePath), true);

            var versionAddresses = ReadJsonFileAsModel<List<BaseAddressModel>>(filePath, string.Empty)?.ToDictionary(x => x.Key, x => x);
            if (versionAddresses == null)
                continue;

            baseAddresses.Add(gameVersion, versionAddresses);
        }

        return baseAddresses;
    }

    /// LoadConfigs
    internal static Dictionary<string, ObservableCollection<object>> LoadConfigs(string? type = null, string? name = null)
    {
        var configs = new Dictionary<string, ObservableCollection<object>>();

        void GetConfig<T>(string t)
        {
            if (type != null && type != t)
                return;

            var directoryPath = Path.Combine(ConfigsPath, t);
            if (!Directory.Exists(directoryPath))
                return;

            configs.TryAdd(t, []);
            foreach (var filePath in Directory.GetFiles(Path.Combine(ConfigsPath, t), "*.json"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if (!string.IsNullOrEmpty(name) && name != fileName)
                    continue;

                var config = ReadJsonFileAsModel<T>(filePath, t);
                if (config == null)
                    continue;

                config.GetType().GetProperty(nameof(ConfigModel.Name))?.SetValue(config, fileName);
                configs[t].Add(config);
            }
        }

        if (type == null || type == "aiv")
            configs.TryAdd("aiv", new(Directory.GetDirectories(Path.Combine(ConfigsPath, "aiv")).Select(x => new AivConfigModel() { Name = Path.GetFileNameWithoutExtension(x) }).Cast<object>()));
        GetConfig<AicConfigModel>("aic");
        GetConfig<BuildingsConfigModel>("buildings");
        GetConfig<GoodsConfigModel>("goods");
        GetConfig<ResourcesConfigModel>("resources");
        GetConfig<SkirmishTrailConfigModel>("skirmishtrail");
        GetConfig<TroopsConfigModel>("troops");
        GetConfig<UnitsConfigModel>("units");
        GetConfig<CustomsConfigModel>("customs");

        return configs;
    }
    
    /// SaveConfigs
    internal static void SaveConfigs()
    {
        foreach (var config in Configs["aic"].Cast<AicConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "aic", config.Name + ".json"), "aic");
        foreach (var config in Configs["buildings"].Cast<BuildingsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "buildings", config.Name + ".json"), "buildings");
        foreach (var config in Configs["goods"].Cast<GoodsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "goods", config.Name + ".json"), "goods");
        foreach (var config in Configs["resources"].Cast<ResourcesConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "resources", config.Name + ".json"), "resources");
        foreach (var config in Configs["skirmishtrail"].Cast<SkirmishTrailConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "skirmishtrail", config.Name + ".json"), "skirmishtrail");
        foreach (var config in Configs["troops"].Cast<TroopsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "troops", config.Name + ".json"), "troops");
        foreach (var config in Configs["units"].Cast<UnitsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "units", config.Name + ".json"), "units");
        foreach (var config in Configs["customs"].Cast<CustomsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(ConfigsPath, "customs", config.Name + ".json"), "customs");
    }

    /// ReadJsonFileAsModel
    internal static T? ReadJsonFileAsModel<T>(string filePath, string type)
    {
        var json = File.ReadAllText(filePath);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            Converters = {
                new JsonStringEnumConverter<BlacksmithSetting>(),
                new JsonStringEnumConverter<FletcherSetting>(),
                new JsonStringEnumConverter<GameVersion>(),
                new JsonStringEnumConverter<HarassingUnit>(),
                new JsonStringEnumConverter<LordType>(),
                new JsonStringEnumConverter<PoleturnerSetting>(),
                new JsonStringEnumConverter<TargetChoice>(),
            }
        };
        if (type == "aic")
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Building>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Resource>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Unit>());
        }
        else if (type == "buildings")
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Unit>());
        }
        if (!type.In("goods", "troops"))
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SkirmishMode>());
        }
        var options = jsonSerializerOptions;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// SaveModelIntoFile
    internal static void SaveModelIntoFile<T>(T obj, string filePath, string type)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DefaultValueModifier }
            },
            Converters = {
                new JsonStringEnumConverter<BlacksmithSetting>(),
                new JsonStringEnumConverter<FletcherSetting>(),
                new JsonStringEnumConverter<GameVersion>(),
                new JsonStringEnumConverter<HarassingUnit>(),
                new JsonStringEnumConverter<LordType>(),
                new JsonStringEnumConverter<PoleturnerSetting>(),
                new JsonStringEnumConverter<TargetChoice>(),
                new SingleLineArrayConverterFactory()
            }
        };
        if (type == "aic")
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Building>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Resource>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Unit>());
        }
        else if (type == "buildings")
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Unit>());
        }
        if (!type.In("goods", "troops"))
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SkirmishMode>());
        }
        var options = jsonSerializerOptions;
        var json = JsonSerializer.Serialize(obj, options);

        File.WriteAllText(filePath, json);
    }

    /// DefaultValueModifier
    private static void DefaultValueModifier(JsonTypeInfo type_info)
    {
        foreach (var property in type_info.Properties)
            if (typeof(ICollection).IsAssignableFrom(property.PropertyType))
                property.ShouldSerialize = (_, val) => val is ICollection collection && collection.Count > 0;
    }
}
