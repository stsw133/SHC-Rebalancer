using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SHC_Rebalancer;

/// StorageService
internal static class StorageService
{
    static StorageService()
    {
        BaseAddresses = LoadBaseAddresses();
        Configs = LoadConfigs();
    }

    public static Dictionary<GameVersion, string> ExePath => new()
    {
        { GameVersion.Crusader, Path.Combine(SettingsService.Instance.Settings.GamePath, "Stronghold Crusader.exe") },
        { GameVersion.Extreme, Path.Combine(SettingsService.Instance.Settings.GamePath, "Stronghold_Crusader_Extreme.exe") },
    };
    public static string AivPath => Path.Combine(SettingsService.Instance.Settings.GamePath, "aiv");
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
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            if (!Enum.TryParse<GameVersion>(fileNameWithoutExt, ignoreCase: true, out var gameVersion))
                continue;

            var versionAddresses = ReadJsonFileAsModel<List<BaseAddressModel>>(filePath, string.Empty)?.ToDictionary(x => x.Key, x => x);
            if (versionAddresses != null)
                baseAddresses.Add(gameVersion, versionAddresses);
        }

        return baseAddresses;
    }
    
    /// LoadConfigs
    internal static Dictionary<string, ObservableCollection<object>> LoadConfigs(string? type = null, string? name = null)
    {
        var configs = new Dictionary<string, ObservableCollection<object>>();

        if (type == null || type == "aiv")
            LoadAivConfigs(configs);

        var knownTypes = new (string key, Type modelType)[]
        {
            ("options", typeof(OptionsConfigModel)),
            ("aic", typeof(AicConfigModel)),
            ("goods", typeof(GoodsConfigModel)),
            ("troops", typeof(TroopsConfigModel)),
            ("buildings", typeof(BuildingsConfigModel)),
            ("popularity", typeof(PopularityConfigModel)),
            ("resources", typeof(ResourcesConfigModel)),
            ("units", typeof(UnitsConfigModel)),
            ("skirmishtrail", typeof(SkirmishTrailConfigModel)),
            ("customs", typeof(CustomsConfigModel))
        };

        foreach (var (folderKey, modelType) in knownTypes)
        {
            if (type != null && type != folderKey)
                continue;

            configs.TryAdd(folderKey, []);

            var directoryPath = Path.Combine(ConfigsPath, folderKey);
            if (!Directory.Exists(directoryPath))
                continue;

            foreach (var filePath in Directory.GetFiles(directoryPath, "*.json"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                if (!string.IsNullOrEmpty(name) && name != fileName)
                    continue;

                var config = ReadJsonFileAsModel(filePath, folderKey, modelType);
                if (config == null)
                    continue;

                config.GetType().GetProperty(nameof(ConfigModel.Name))?.SetValue(config, fileName);

                configs[folderKey].Add(config);
            }
        }

        return configs;
    }

    /// LoadAivConfigs
    private static void LoadAivConfigs(Dictionary<string, ObservableCollection<object>> configs)
    {
        configs.TryAdd("aiv", []);

        var aivRoot = Path.Combine(ConfigsPath, "aiv");
        if (!Directory.Exists(aivRoot))
            return;

        foreach (var directoryPath in Directory.GetDirectories(aivRoot))
        {
            var aivs = new Dictionary<AI, ObservableCollection<AivModel>>();
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.aiv"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var aiName = string.Concat(fileName.Where(char.IsLetter)).Capitalize();
                var ai = Enum.Parse<AI>(aiName);

                aivs.TryAdd(ai, []);
                aivs[ai].Add(new AivModel
                {
                    Name = fileName,
                    FilePath = filePath
                });
            }

            var images = new Dictionary<string, string>();
            if (Directory.Exists(Path.Combine(directoryPath, "Images")))
                images = Directory.GetFiles(Path.Combine(directoryPath, "Images"), "*.webp").Select(x => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(x), x)).ToDictionary();

            configs["aiv"].Add(new AivConfigModel
            {
                Name = new DirectoryInfo(directoryPath).Name,
                AIs = aivs,
                Images = images
            });
        }
    }

    /// SaveConfigs
    internal static void SaveConfigs()
    {
        var knownTypes = new (string key, Type modelType)[]
        {
            ("options", typeof(OptionsConfigModel)),
            ("aic", typeof(AicConfigModel)),
            ("goods", typeof(GoodsConfigModel)),
            ("troops", typeof(TroopsConfigModel)),
            ("buildings", typeof(BuildingsConfigModel)),
            ("popularity", typeof(PopularityConfigModel)),
            ("resources", typeof(ResourcesConfigModel)),
            ("units", typeof(UnitsConfigModel)),
            ("skirmishtrail", typeof(SkirmishTrailConfigModel)),
            ("customs", typeof(CustomsConfigModel))
        };

        foreach (var (folderKey, modelType) in knownTypes)
        {
            if (!Configs.ContainsKey(folderKey))
                continue;

            var folderPath = Path.Combine(ConfigsPath, folderKey);
            Directory.CreateDirectory(folderPath);

            foreach (var configObj in Configs[folderKey])
            {
                var nameProp = configObj.GetType().GetProperty(nameof(ConfigModel.Name));
                if (nameProp == null)
                    continue;

                var configName = nameProp.GetValue(configObj)?.ToString();
                if (string.IsNullOrWhiteSpace(configName))
                    continue;

                var filePath = Path.Combine(folderPath, configName + ".json");
                SaveModelIntoFile(configObj, filePath, folderKey);
            }
        }
    }

    /// ReadJsonFileAsModel
    private static object? ReadJsonFileAsModel(string filePath, string folderKey, Type modelType)
    {
        var json = File.ReadAllText(filePath).Replace("\r\n", " ").Replace("\t", " ");
        var options = GetJsonSerializerOptions(folderKey, forReading: true);

        return JsonSerializer.Deserialize(json, modelType, options);
    }

    /// ReadJsonFileAsModel
    internal static T? ReadJsonFileAsModel<T>(string filePath, string type)
    {
        var obj = ReadJsonFileAsModel(filePath, type, typeof(T));
        return (T?)obj;
    }

    /// SaveModelIntoFile
    internal static void SaveModelIntoFile(object obj, string filePath, string folderKey)
    {
        var options = GetJsonSerializerOptions(folderKey, forReading: false);
        var json = JsonSerializer.Serialize(obj, obj.GetType(), options);

        File.WriteAllText(filePath, json);
    }

    /// GetJsonSerializerOptions
    private static JsonSerializerOptions GetJsonSerializerOptions(string folderKey, bool forReading)
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = forReading,
            PropertyNameCaseInsensitive = forReading,
            DefaultIgnoreCondition = forReading ? JsonIgnoreCondition.Never : JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = !forReading,
            TypeInfoResolver = forReading ? null : new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DefaultValueModifier }
            }
        };
        if (!forReading)
            options.Converters.Add(new SingleLineArrayConverterFactory());

        options.Converters.Add(new JsonStringEnumConverter<BlacksmithSetting>());
        options.Converters.Add(new JsonStringEnumConverter<FletcherSetting>());
        options.Converters.Add(new JsonStringEnumConverter<GameVersion>());
        options.Converters.Add(new JsonStringEnumConverter<HarassingUnit>());
        options.Converters.Add(new JsonStringEnumConverter<LordType>());
        options.Converters.Add(new JsonStringEnumConverter<PoleturnerSetting>());
        options.Converters.Add(new JsonStringEnumConverter<TargetChoice>());

        if (folderKey == "aic")
        {
            options.Converters.Add(new JsonStringEnumConverter<Building>());
            options.Converters.Add(new JsonStringEnumConverter<Resource>());
            options.Converters.Add(new JsonStringEnumConverter<Unit>());
        }
        else if (folderKey == "buildings")
        {
            options.Converters.Add(new JsonStringEnumConverter<Unit>());
        }

        if (!folderKey.In("goods", "troops"))
        {
            options.Converters.Add(new JsonStringEnumConverter<SkirmishMode>());
        }

        return options;
    }

    /// DefaultValueModifier
    private static void DefaultValueModifier(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
            if (typeof(ICollection).IsAssignableFrom(property.PropertyType))
                property.ShouldSerialize = (_, val) => val is ICollection collection && collection.Count > 0;
    }
}
