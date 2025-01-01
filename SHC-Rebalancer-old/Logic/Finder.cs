using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace SHC_Rebalancer_old;
public static class Finder
{
    /// Find
    public static void Find(ObservableCollection<FinderDataModel> finderData, StrongholdType gameType, int filterSize, string? filterAddress, int filterSkips, string? filterValues)
    {
        var filePath = gameType switch
        {
            StrongholdType.Stronghold => Settings.Default.StrongholdPath,
            StrongholdType.Crusader => Settings.Default.CrusaderPath,
            StrongholdType.Extreme => Settings.Default.ExtremePath,
            _ => throw new NotSupportedException("Game type is not selected!")
        };

        var configPath = App.GetConfigPath(gameType, gameType switch
        {
            StrongholdType.Stronghold => Settings.Default.StrongholdConfig,
            StrongholdType.Crusader => Settings.Default.CrusaderConfig,
            StrongholdType.Extreme => Settings.Default.ExtremeConfig,
            _ => throw new NotImplementedException()
        });

        var configAddresses = GetConfigAddresses(configPath);
        var fileBytes = File.ReadAllBytes(filePath);
        finderData.Clear();

        if (!string.IsNullOrWhiteSpace(filterValues))
        {
            FindPatternInFile(finderData, fileBytes, filterSize, filterValues, configAddresses);
        }
        else if (!string.IsNullOrWhiteSpace(filterAddress))
        {
            FindAddresses(finderData, filePath, filterSize, filterAddress, filterSkips, configAddresses);
        }
    }

    /// GetConfigAddresses
    private static HashSet<int> GetConfigAddresses(string configPath)
    {
        var json = File.ReadAllText(configPath);
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter<SkirmishType>());
        var configData = JsonSerializer.Deserialize<ConfigDataModel>(json, options);

        ArgumentNullException.ThrowIfNull(configData);

        return new HashSet<int>(configData.BaseAddresses.Concat(configData.Other).Select(x => Convert.ToInt32(x.Address, 16)));
    }

    /// FindPatternInFile
    private static void FindPatternInFile(ObservableCollection<FinderDataModel> finderData, byte[] fileBytes, int filterSize, string filterValues, HashSet<int> configAddresses)
    {
        var pattern = filterValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                  .Select(x => filterSize == 1 ? (object)Convert.ToByte(x) : Convert.ToInt32(x))
                                  .ToList();

        var stepSize = filterSize == 1 ? 1 : 4;
        for (var i = 0; i <= fileBytes.Length - pattern.Count * stepSize; i += stepSize)
        {
            if (IsPatternMatch(fileBytes, i, pattern, stepSize))
            {
                for (var k = 0; k < pattern.Count; k++)
                {
                    int address = i + k * stepSize;
                    int value = filterSize == 1 ? fileBytes[address] : BitConverter.ToInt32(fileBytes, address);
                    finderData.Add(new FinderDataModel
                    {
                        Address = $"0x{address:X}",
                        Value = value,
                        IsInConfigFile = configAddresses.Contains(address)
                    });
                }
            }
        }
    }

    /// IsPatternMatch
    private static bool IsPatternMatch(byte[] fileBytes, int startIndex, List<object> pattern, int filterSize)
    {
        for (var j = 0; j < pattern.Count; j++)
        {
            if (filterSize == 1)
            {
                if ((byte)pattern[j] != fileBytes[startIndex + j])
                    return false;
            }
            else if (filterSize == 4)
            {
                int value = BitConverter.ToInt32(fileBytes, startIndex + j * 4);
                if ((int)pattern[j] != value)
                    return false;
            }
        }
        return true;
    }

    /// FindAddresses
    private static void FindAddresses(ObservableCollection<FinderDataModel> finderData, string filePath, int filterSize, string filterAddress, int filterSkips, HashSet<int> configAddresses)
    {
        if (!long.TryParse(filterAddress.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long startAddress))
            return;

        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new BinaryReader(fs);

        if (startAddress >= reader.BaseStream.Length)
            return;

        reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);

        for (var i = 0; i < 50 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
        {
            var address = reader.BaseStream.Position;
            var value = filterSize == 1 ? reader.ReadByte() : reader.ReadInt32();

            finderData.Add(new FinderDataModel
            {
                Address = address.ToString("X8"),
                Value = value,
                IsInConfigFile = configAddresses.Contains((int)address)
            });

            if (filterSkips != 0)
                reader.BaseStream.Seek(filterSkips, SeekOrigin.Current);
        }
    }
}
