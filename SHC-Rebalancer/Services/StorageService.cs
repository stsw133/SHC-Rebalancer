using System.Collections;
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

    public static string PathBaseAddresses => Path.Combine(AppContext.BaseDirectory, "Resources", "base");
    public static string PathConfigs => Path.Combine(AppContext.BaseDirectory, "Resources", "configs");
    
    public static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> BaseAddresses { get; set; } = [];
    public static Dictionary<string, List<object>> Configs { get; set; } = [];

    /// LoadBaseAddresses
    internal static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> LoadBaseAddresses()
    {
        var baseAddresses = new Dictionary<GameVersion, Dictionary<string, BaseAddressModel>>();

        foreach (var filePath in Directory.GetFiles(PathBaseAddresses, "*.json"))
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
    internal static Dictionary<string, List<object>> LoadConfigs(string? type = null)
    {
        var configs = new Dictionary<string, List<object>>();

        void GetConfig<T>(string t)
        {
            if (type == null || type == t)
            {
                configs.TryAdd(t, []);
                foreach (var filePath in Directory.GetFiles(Path.Combine(PathConfigs, t), "*.json"))
                {
                    var config = ReadJsonFileAsModel<T>(filePath, t);
                    if (config == null)
                        continue;

                    var name = Path.GetFileNameWithoutExtension(filePath);
                    config.GetType().GetProperty("Name")?.SetValue(config, name);

                    configs[t].Add(config);
                }
            }
        }

        GetConfig<AicConfigModel>("aic");
        GetConfig<BuildingsConfigModel>("buildings");
        GetConfig<ResourcesConfigModel>("resources");
        GetConfig<SkirmishTrailConfigModel>("skirmishtrail");
        GetConfig<UnitsConfigModel>("units");
        GetConfig<OthersConfigModel>("others");

        return configs;
    }
    
    /// SaveConfigs
    internal static void SaveConfigs()
    {
        foreach (var config in Configs["aic"].Cast<AicConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "aic", config.Name + ".json"), "aic");
        foreach (var config in Configs["buildings"].Cast<BuildingsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "buildings", config.Name + ".json"), "buildings");
        foreach (var config in Configs["resources"].Cast<ResourcesConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "resources", config.Name + ".json"), "resources");
        foreach (var config in Configs["skirmishtrail"].Cast<SkirmishTrailConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "skirmishtrail", config.Name + ".json"), "skirmishtrail");
        foreach (var config in Configs["units"].Cast<UnitsConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "units", config.Name + ".json"), "units");
        foreach (var config in Configs["others"].Cast<OthersConfigModel>())
            SaveModelIntoFile(config, Path.Combine(PathConfigs, "others", config.Name + ".json"), "others");
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
                new JsonStringEnumConverter<GameVersion>(),
                new JsonStringEnumConverter<HarassingUnit>(),
                new JsonStringEnumConverter<SkirmishMode>(),
                new JsonStringEnumConverter<TargetChoice>(),
            }
        };
        if (type == "aic")
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Building>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Resource>());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Unit>());
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
                new JsonStringEnumConverter<GameVersion>(),
                new JsonStringEnumConverter<HarassingUnit>(),
                new JsonStringEnumConverter<SkirmishMode>(),
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
