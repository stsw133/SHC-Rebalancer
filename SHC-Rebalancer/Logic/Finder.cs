using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace SHC_Rebalancer;
public static class Finder
{
    /// Find
    public static void Find(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, int filterSize, string? filterAddress, int filterSkips, string? filterValues)
    {
        var filePath = gameVersion switch
        {
            GameVersion.Crusader => Settings.Default.CrusaderPath,
            GameVersion.Extreme => Settings.Default.ExtremePath,
            _ => throw new NotSupportedException("Incorrect game version!")
        };
        
        var fileBytes = File.ReadAllBytes(filePath);
        finderData.Clear();

        if (!string.IsNullOrWhiteSpace(filterValues))
            FindPatternInFile(finderData, gameVersion, fileBytes, filterSize, filterValues);
        else if (!string.IsNullOrWhiteSpace(filterAddress))
            FindAddresses(finderData, gameVersion, filePath, filterSize, filterAddress, filterSkips);
    }

    /// FindPatternInFile
    private static void FindPatternInFile(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, byte[] fileBytes, int filterSize, string filterValues)
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
                        IsInConfigFile = Storage.BaseAddresses[gameVersion].Any(x => address.Between(Convert.ToInt32(x.Value.Address), Convert.ToInt32(x.Value.EndAddress ?? x.Value.Address)))
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
    private static void FindAddresses(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, string filePath, int filterSize, string filterAddress, int filterSkips)
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
                IsInConfigFile = Storage.BaseAddresses[gameVersion].Any(x => address.Between(Convert.ToInt32(x.Value.Address, 16), Convert.ToInt32(x.Value.EndAddress ?? x.Value.Address, 16)))
            });

            if (filterSkips != 0)
                reader.BaseStream.Seek(filterSkips, SeekOrigin.Current);
        }
    }
}
