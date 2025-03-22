using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace SHC_Rebalancer;

/// FinderService
internal static class FinderService
{
    /// Find
    internal static void Find(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, int filterSize, bool displayAsChar, string? filterAddress, int filterSkips, string? filterValues, int filterLimit)
    {
        finderData.Clear();

        var exePath = StorageService.ExePath[gameVersion];

        if (!string.IsNullOrWhiteSpace(filterValues))
            FindPatternInFile(finderData, gameVersion, File.ReadAllBytes(exePath), filterSize, displayAsChar, filterValues, filterLimit);
        else if (!string.IsNullOrWhiteSpace(filterAddress))
            FindAddresses(finderData, gameVersion, exePath, filterSize, displayAsChar, filterAddress, filterSkips, filterLimit);
    }

    /// FindAddresses
    private static void FindAddresses(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, string filePath, int filterSize, bool displayAsChar, string filterAddress, int filterSkips, int filterLimit)
    {
        if (!long.TryParse(filterAddress.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long startAddress))
            return;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        if (startAddress >= reader.BaseStream.Length)
            return;

        reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);

        var usedAddresses = GetUsedAddresses(gameVersion);

        for (var i = 0; i < filterLimit && reader.BaseStream.Position < reader.BaseStream.Length; i++)
        {
            var address = reader.BaseStream.Position;

            var value = filterSize switch
            {
                1 => reader.ReadByte(),
                2 => reader.ReadInt16(),
                4 => reader.ReadInt32(),
                _ => reader.ReadByte()
            };

            var desc = usedAddresses
                .FirstOrDefault(x => address.Between(
                    Convert.ToInt32(x.Address, 16),
                    Convert.ToInt32(x.EndAddress ?? x.Address, 16)))
                ?.Key;
            var isInConfig = desc != null;

            finderData.Add(new FinderDataModel
            {
                Address = address.ToString("X8"),
                Value = displayAsChar ? ((char)value).ToString() : value,
                IsInConfigFile = isInConfig,
                Description = desc
            });

            if (filterSkips != 0)
                reader.BaseStream.Seek(filterSkips, SeekOrigin.Current);
        }
    }

    /// FindPatternInFile
    private static void FindPatternInFile(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, byte[] fileBytes, int filterSize, bool displayAsChar, string filterValues, int filterLimit)
    {
        var usedAddresses = GetUsedAddresses(gameVersion);
        var pattern = ParsePattern(filterValues, filterSize);

        var found = 0;
        var maxIndex = fileBytes.Length - pattern.Count * filterSize;

        for (var i = 0; i <= maxIndex; i += filterSize)
        {
            if (IsPatternMatch(fileBytes, i, pattern, filterSize))
            {
                for (var k = 0; k < pattern.Count; k++)
                {
                    var address = i + k * filterSize;
                    var value = ReadValueAsInt(fileBytes, address, filterSize);

                    var desc = usedAddresses
                        .FirstOrDefault(x => address.Between(
                            Convert.ToInt32(x.Address, 16),
                            Convert.ToInt32(x.EndAddress ?? x.Address, 16)))
                        ?.Key;
                    var isInConfig = desc != null;

                    finderData.Add(new FinderDataModel
                    {
                        Address = $"0x{address:X}",
                        Value = displayAsChar ? ((char)value).ToString() : value,
                        IsInConfigFile = isInConfig,
                        Description = desc
                    });
                }

                found++;
                if (found >= filterLimit)
                    break;
            }
        }
    }

    /// GetUsedAddresses
    private static IEnumerable<BaseAddressModel> GetUsedAddresses(GameVersion gameVersion)
    {
        var fromBaseAddresses = StorageService.BaseAddresses[gameVersion]
            .Select(x => new BaseAddressModel
            {
                Address = x.Value.Address,
                EndAddress = x.Value.EndAddress,
                Key = x.Key
            });

        var fromOptions = StorageService.Configs["options"]
            .Cast<OptionsConfigModel>()
            .SelectMany(config => config.Options)
            .SelectMany(option => option.Value.Modifications
                .Where(mod => mod.Version == gameVersion),
                (option, mod) => new BaseAddressModel
                {
                    Address = mod.Address,
                    EndAddress = mod.EndAddress,
                    Key = option.Key
                });

        return fromBaseAddresses.Union(fromOptions);
    }

    /// IsPatternMatch
    private static bool IsPatternMatch(byte[] fileBytes, int startIndex, List<object?> pattern, int filterSize)
    {
        for (var j = 0; j < pattern.Count; j++)
        {
            if (pattern[j] == null)
                continue;

            var offset = startIndex + j * filterSize;
            var value = ReadValueAsInt(fileBytes, offset, filterSize);

            if (filterSize == 1)
            {
                if ((byte)pattern[j]! != (byte)value)
                    return false;
            }
            else if (filterSize == 2)
            {
                if ((short)pattern[j]! != (short)value)
                    return false;
            }
            else
            {
                if ((int)pattern[j]! != value)
                    return false;
            }
        }
        return true;
    }

    /// ParsePattern
    private static List<object?> ParsePattern(string filterValues, int filterSize)
    {
        return [.. filterValues
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x =>
            {
                if (x.StartsWith("'") && x.EndsWith("'") && x.Length == 3)
                    return (object?)(byte)x[1];

                if (x == "?")
                    return null;

                return filterSize switch
                {
                    1 => (object?)byte.Parse(x),
                    2 => (object?)short.Parse(x),
                    4 => (object?)int.Parse(x),
                    _ => throw new NotSupportedException($"filterSize={filterSize} not supported."),
                };
            })];
    }

    /// ReadValueAsInt
    private static int ReadValueAsInt(byte[] fileBytes, int offset, int size) => size switch
    {
        1 => fileBytes[offset],
        2 => BitConverter.ToInt16(fileBytes, offset),
        4 => BitConverter.ToInt32(fileBytes, offset),
        _ => throw new NotSupportedException($"filterSize={size} not supported.")
    };
}
