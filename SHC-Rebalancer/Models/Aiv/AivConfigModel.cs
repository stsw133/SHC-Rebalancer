using System.IO;

namespace SHC_Rebalancer;

/// AivConfigModel
public class AivConfigModel : ConfigModel
{
    public string[] Castles
    {
        get
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + $"Configs\\aiv\\{Name}";
            if (Directory.Exists(directory))
                return Directory.GetFiles(directory, "*.aiv");
            return [];
        }
    }
    public string[] Images
    {
        get
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + $"Configs\\aiv\\{Name}\\images";
            if (Directory.Exists(directory))
                return [..Directory.GetFiles(directory).Where(x => Path.GetExtension(x).In(".bmp", ".jpg", ".png", ".webp"))];
            return [];
        }
    }
    public bool ReplaceAllCastles { get; set; } = true;
}
