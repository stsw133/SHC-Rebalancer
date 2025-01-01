using System.IO;
using System.Text.Json;

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
            PropertyNameCaseInsensitive = true
        };
        var options = jsonSerializerOptions;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SkirmishType>());

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// SaveJson
    internal static void SaveJsonIntoFile<T>(T obj, string filePath)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var options = jsonSerializerOptions;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SkirmishType>());
        var json = JsonSerializer.Serialize(obj, options);

        File.WriteAllText(filePath, json);
    }
}
