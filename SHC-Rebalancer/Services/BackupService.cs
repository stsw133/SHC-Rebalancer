using System.IO;
using System.IO.Compression;

namespace SHC_Rebalancer;

/// BackupService
internal static class BackupService
{
    /// Exists
    private static bool Exists(string exePath, out string backupPath)
    {
        var directoryPath = Directory.GetParent(exePath)!.FullName;
        var fileName = Path.GetFileNameWithoutExtension(exePath);
        backupPath = Path.Combine(directoryPath, fileName + ".exe.stsw_backup");

        return File.Exists(backupPath);
    }

    /// Make
    internal static void Make()
    {
        foreach (var exePath in StorageService.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            if (!Exists(exePath.Value, out var backupPath))
                File.Copy(exePath.Value, backupPath, true);
        }

        MakeZipForAIV();
    }

    /// Restore
    internal static void Restore()
    {
        foreach (var exePath in StorageService.ExePath)
        {
            if (!File.Exists(exePath.Value))
                continue;

            if (Exists(exePath.Value, out var backupPath))
            {
                File.Copy(backupPath, exePath.Value, true);
                File.Delete(backupPath);
            }
        }

        RestoreZipForAIV();
    }

    /// MakeZipForAIV
    private static void MakeZipForAIV()
    {
        if (!Directory.Exists(StorageService.AivPath))
            return;

        var filesToArchive = new List<string>();
        foreach (var enumValue in Enum.GetValues(typeof(AI)))
        {
            for (var i = 1; i <= 8; i++)
            {
                var fileName = $"{enumValue}{i}.aiv";
                var filePath = Path.Combine(StorageService.AivPath, fileName);
                if (File.Exists(filePath))
                    filesToArchive.Add(filePath);
            }
        }

        var backupPath = Path.Combine(StorageService.AivPath, "aiv.zip.stsw_backup");
        if (File.Exists(backupPath))
            return;

        using var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
        foreach (var file in filesToArchive)
            zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
    }

    /// RestoreZipForAIV
    private static void RestoreZipForAIV()
    {
        var zipFilePath = Path.Combine(StorageService.AivPath, "aiv.zip.stsw_backup");
        if (!File.Exists(zipFilePath))
            return;

        if (!Directory.Exists(StorageService.AivPath))
            Directory.CreateDirectory(StorageService.AivPath);

        using (var zipArchive = ZipFile.OpenRead(zipFilePath))
        {
            foreach (var entry in zipArchive.Entries)
            {
                var fullDestPath = Path.Combine(StorageService.AivPath, entry.FullName);
                entry.ExtractToFile(fullDestPath, overwrite: true);
            }
        }

        File.Delete(zipFilePath);
    }
}
