using System.IO;

namespace SHC_Rebalancer;
public static class Backup
{
    /// Exists
    public static bool Exists(string exePath, out string backupPath)
    {
        var directoryPath = Directory.GetParent(exePath)!.FullName;
        var fileName = Path.GetFileNameWithoutExtension(exePath);
        backupPath = Path.Combine(directoryPath, fileName + ".exe.stsw_backup");

        if (File.Exists(backupPath))
            return true;

        return false;
    }
    
    /// Make
    public static void Make(string exePath)
    {
        if (!File.Exists(exePath))
            return;

        if (!Exists(exePath, out var backupPath))
            File.Copy(exePath, backupPath, true);
    }

    /// Restore
    public static void Restore(string exePath)
    {
        if (!File.Exists(exePath))
            return;

        if (Exists(exePath, out var backupPath))
            File.Copy(backupPath, exePath, true);

        File.Delete(backupPath);
    }
}
