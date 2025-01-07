using System.Collections;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SHC_Rebalancer;
public static class Storage
{
    public static string PathRebalances => Path.Combine(AppContext.BaseDirectory, "Resources/rebalance");
    public static string PathBaseAddresses => Path.Combine(AppContext.BaseDirectory, "Resources/rebalance/base");
    public static Dictionary<string, RebalanceModel> Rebalances { get; set; } = [];
    public static Dictionary<GameVersion, Dictionary<string, BaseAddressModel>> BaseAddresses { get; set; } = [];

    /// LoadBaseAddresses
    internal static void LoadBaseAddresses()
    {
        var baseAddresses = new Dictionary<GameVersion, Dictionary<string, BaseAddressModel>>();

        foreach (var filePath in Directory.GetFiles(PathBaseAddresses, "*.json"))
        {
            var gameVersion = Enum.Parse<GameVersion>(Path.GetFileNameWithoutExtension(filePath), true);

            var versionAddresses = ReadJsonIntoList<List<BaseAddressModel>>(filePath)?.ToDictionary(x => x.Key, x => x);
            if (versionAddresses == null)
                continue;

            baseAddresses.Add(gameVersion, versionAddresses);
        }

        BaseAddresses = baseAddresses;
    }
    
    /// LoadRebalances
    internal static void LoadRebalances()
    {
        var rebalances = new Dictionary<string, RebalanceModel>();

        foreach (var filePath in Directory.GetFiles(PathRebalances, "*.json"))
        {
            var rebalance = ReadJsonIntoList<RebalanceModel>(filePath);
            if (rebalance == null)
                continue;

            rebalances.Add(Path.GetFileNameWithoutExtension(filePath), rebalance);
        }

        Rebalances = rebalances;
    }

    /// ReadJson
    internal static T? ReadJsonIntoList<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            Converters = {
                new JsonStringEnumConverter<SkirmishMode>(),
            }
        };
        var options = jsonSerializerOptions;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// SaveJson
    internal static void SaveJsonIntoFile<T>(T obj, string filePath)
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
                new JsonStringEnumConverter<SkirmishMode>(),
                new SingleLineArrayConverterFactory()
            }
        };
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
