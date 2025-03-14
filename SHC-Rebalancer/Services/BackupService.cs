﻿using System.IO;
using System.IO.Compression;

namespace SHC_Rebalancer;

/// BackupService
internal static class BackupService
{
    /// Exists
    internal static bool Exists(string exePath, out string backupPath)
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

        if (!string.IsNullOrEmpty(SettingsService.Instance.Settings.SelectedConfigs["air"]))
            MakeZipForAIR();
        if (!string.IsNullOrEmpty(SettingsService.Instance.Settings.SelectedConfigs["aiv"]))
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

        RestoreZipForAIR();
        RestoreZipForAIV();
    }

    /// MakeZipForAIR
    private static void MakeZipForAIR()
    {
        /// images
        var backupPath = Path.Combine(StorageService.GmPath, "gm.zip.stsw_backup");
        if (!File.Exists(backupPath))
        {
            using var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
            if (Path.Combine(StorageService.GmPath, "interface_icons2.gm1") is string filePath && File.Exists(filePath))
                zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
        }

        /// speech
        backupPath = Path.Combine(StorageService.FxSpeechPath, "speech.zip.stsw_backup");
        if (!File.Exists(backupPath))
        {
            string[] prefixes = ["all", "rt", "sn", "pg", "wf", "sa", "ca", "su", "ri", "fr", "ph", "wa", "em", "ni", "sh", "ma", "ab"];
            using var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
            foreach (var prefix in prefixes)
                foreach (var filePath in Directory.GetFiles(StorageService.FxSpeechPath, $"{prefix}_*.wav"))
                    zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            for (var i = 23; i < 38; i++)
                zipArchive.CreateEntryFromFile(Path.Combine(StorageService.FxSpeechPath, $"General_Message{i}.wav"), $"General_Message{i}.wav");
        }

        /// text
        backupPath = Path.Combine(SettingsService.Instance.Settings.GamePath, "cr.tex.stsw_backup");
        if (!File.Exists(backupPath) && File.Exists(TexService.TexFilePath))
            File.Copy(TexService.TexFilePath, backupPath, true);

        /// videos
        backupPath = Path.Combine(StorageService.BinksPath, "bik.zip.stsw_backup");
        if (!File.Exists(backupPath))
        {
            string[] prefixes = ["bad_soldier", "rt", "sn", "pg", "wf", "saladin", "bad_arab", "sultan", "richard", "fred", "phillip", "vizir", "emir", "nazir", "sheriff", "ma", "abbot"];
            using var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
            foreach (var prefix in prefixes)
                foreach (var filePath in Directory.GetFiles(StorageService.BinksPath, $"{prefix}_*.bik"))
                    zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
        }
    }

    /// RestoreZipForAIR
    private static void RestoreZipForAIR()
    {
        /// images
        if (!Directory.Exists(StorageService.GmPath))
            Directory.CreateDirectory(StorageService.GmPath);

        var zipFilePath = Path.Combine(StorageService.GmPath, "gm.zip.stsw_backup");
        if (File.Exists(zipFilePath))
        {
            using var zipArchive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in zipArchive.Entries)
                entry.ExtractToFile(Path.Combine(StorageService.GmPath, entry.FullName), overwrite: true);
        }
        File.Delete(zipFilePath);

        /// speech
        if (!Directory.Exists(StorageService.FxSpeechPath))
            Directory.CreateDirectory(StorageService.FxSpeechPath);

        zipFilePath = Path.Combine(StorageService.FxSpeechPath, "speech.zip.stsw_backup");
        if (File.Exists(zipFilePath))
        {
            using var zipArchive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in zipArchive.Entries)
                entry.ExtractToFile(Path.Combine(StorageService.FxSpeechPath, entry.FullName), overwrite: true);
        }
        File.Delete(zipFilePath);

        /// text
        if (Path.Combine(SettingsService.Instance.Settings.GamePath, "cr.tex.stsw_backup") is string backupFile && File.Exists(backupFile))
        {
            File.Copy(backupFile, TexService.TexFilePath, true);
            File.Delete(backupFile);
        }

        /// videos
        if (!Directory.Exists(StorageService.BinksPath))
            Directory.CreateDirectory(StorageService.BinksPath);

        zipFilePath = Path.Combine(StorageService.BinksPath, "bik.zip.stsw_backup");
        if (File.Exists(zipFilePath))
        {
            using var zipArchive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in zipArchive.Entries)
                entry.ExtractToFile(Path.Combine(StorageService.BinksPath, entry.FullName), overwrite: true);
        }
        File.Delete(zipFilePath);
    }

    /// MakeZipForAIV
    private static void MakeZipForAIV()
    {
        if (!Directory.Exists(StorageService.AivPath))
            return;

        var backupPath = Path.Combine(StorageService.AivPath, "aiv.zip.stsw_backup");
        if (File.Exists(backupPath))
            return;

        var filesToArchive = new List<string>();
        foreach (var ai in Enum.GetValues<AI>())
        {
            for (var i = 1; i <= 8; i++)
            {
                var fileName = $"{ai}{i}.aiv";
                var filePath = Path.Combine(StorageService.AivPath, fileName);
                if (File.Exists(filePath))
                    filesToArchive.Add(filePath);
            }
        }
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
            foreach (var entry in zipArchive.Entries)
                entry.ExtractToFile(Path.Combine(StorageService.AivPath, entry.FullName), overwrite: true);

        File.Delete(zipFilePath);
    }
}
