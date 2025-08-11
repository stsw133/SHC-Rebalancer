using System.Collections.ObjectModel;
using System.IO;

namespace SHC_Rebalancer;

/// SkirmishTrailConfigModel
public class SkirmishTrailConfigModel : ConfigModel
{
    public string[] Maps
    {
        get
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + $"Configs\\skirmishtrail\\{Name}";
            if (Directory.Exists(directory))
                return Directory.GetFiles(directory, "*.map");
            return [];
        }
    }
    public string[] Images
    {
        get
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + $"Configs\\skirmishtrail\\{Name}\\images";
            if (Directory.Exists(directory))
                return [.. Directory.GetFiles(directory).Where(x => Path.GetExtension(x).In(".bmp", ".jpg", ".png", ".webp"))];
            return [];
        }
    }
    public ObservableCollection<string> MapMappings { get; set; } = [];
    public string MapMappingsSingleView
    {
        get => string.Join(Environment.NewLine, MapMappings);
        set => MapMappings = [.. value.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)];
    }
    public Dictionary<int, SkirmishTrailModel> Missions { get; set; } = [];
}
