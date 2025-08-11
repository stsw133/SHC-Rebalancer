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
    public static string GameLanguage => TexService.GetTranslationAtIndex(6);

    public static string AivPath => Path.Combine(SettingsService.Instance.Settings.GamePath, "aiv");
    public static string BaseAddressesPath => Path.Combine(AppContext.BaseDirectory, "Configs", "_base");
    public static string BinksPath => Path.Combine(SettingsService.Instance.Settings.GamePath, "binks");
    public static string ConfigsPath => Path.Combine(AppContext.BaseDirectory, "Configs");
    public static string FxSpeechPath => Path.Combine(SettingsService.Instance.Settings.GamePath, "fx", "speech");
    public static string GmPath => Path.Combine(SettingsService.Instance.Settings.GamePath, "gm");
    public static string UcpPath => Path.Combine(AppContext.BaseDirectory, "Configs", "_ucp");
    
    public static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> BaseAddresses { get; set; } = [];
    public static Dictionary<string, ObservableCollection<object>> Configs { get; set; } = [];
    public static IEnumerable<string> CustomAIs { get; set; } = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs/air")).Select(x => Path.GetFileName(x));

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

    /// KnownConfigTypes
    internal static (string key, Type modelType)[] KnownConfigTypes =
    [
        ("options", typeof(OptionsConfigModel)),
        ("aic", typeof(AicConfigModel)),
        ("air", typeof(AirConfigModel)),
        ("aiv", typeof(AivConfigModel)),
        ("goods", typeof(GoodsConfigModel)),
        ("troops", typeof(TroopsConfigModel)),
        ("buildings", typeof(BuildingsConfigModel)),
        ("outposts", typeof(OutpostsConfigModel)),
        ("popularity", typeof(PopularityConfigModel)),
        ("resources", typeof(ResourcesConfigModel)),
        ("units", typeof(UnitsConfigModel)),
        ("skirmishtrail", typeof(SkirmishTrailConfigModel)),
        ("customs", typeof(CustomsConfigModel))
    ];

    /// LoadConfigs
    internal static Dictionary<string, ObservableCollection<object>> LoadConfigs(string? type = null, string? name = null)
    {
        var configs = new Dictionary<string, ObservableCollection<object>>();

        foreach (var (folderKey, modelType) in KnownConfigTypes)
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

    /// SaveConfigs
    internal static void SaveConfigs()
    {
        foreach (var (folderKey, modelType) in KnownConfigTypes)
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
            Converters =
            {
                new JsonStringEnumConverter()
            },
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
