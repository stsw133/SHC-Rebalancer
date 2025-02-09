﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace SHC_Rebalancer;

/// FinderService
public static class FinderService
{
    /// Find
    public static void Find(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, int filterSize, string? filterAddress, int filterSkips, string? filterValues, int filterLimit)
    {
        var fileBytes = File.ReadAllBytes(StorageService.ExePath[gameVersion]);
        finderData.Clear();

        if (!string.IsNullOrWhiteSpace(filterValues))
            FindPatternInFile(finderData, gameVersion, fileBytes, filterSize, filterValues, filterLimit);
        else if (!string.IsNullOrWhiteSpace(filterAddress))
            FindAddresses(finderData, gameVersion, StorageService.ExePath[gameVersion], filterSize, filterAddress, filterSkips, filterLimit);
    }

    /// FindAddresses
    private static void FindAddresses(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, string filePath, int filterSize, string filterAddress, int filterSkips, int filterLimit)
    {
        if (!long.TryParse(filterAddress.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long startAddress))
            return;

        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new BinaryReader(fs);

        if (startAddress >= reader.BaseStream.Length)
            return;

        reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);

        var usedAddresses = StorageService.BaseAddresses[gameVersion]
            .Select(x => new BaseAddressModel
            {
                Address = x.Value.Address,
                EndAddress = x.Value.EndAddress,
                Key = x.Key
            })
            .Union(
                StorageService.Configs["options"]
                    .Cast<OptionsConfigModel>()
                    .SelectMany(config => config.Options)
                    .SelectMany(option => option.Value.Modifications
                        .Where(mod => mod.Version == gameVersion),
                        (option, mod) => new BaseAddressModel
                        {
                            Address = mod.Address,
                            EndAddress = mod.EndAddress,
                            Key = option.Key
                        }
                    )
            );

        for (var i = 0; i < filterLimit && reader.BaseStream.Position < reader.BaseStream.Length; i++)
        {
            var address = reader.BaseStream.Position;
            var value = filterSize == 1 ? reader.ReadByte() : filterSize == 2 ? reader.ReadInt16() : reader.ReadInt32();

            finderData.Add(new FinderDataModel
            {
                Address = address.ToString("X8"),
                Value = value,
                IsInConfigFile = usedAddresses.Any(x => address.Between(Convert.ToInt32(x.Address, 16), Convert.ToInt32(x.EndAddress ?? x.Address, 16))),
                Description = usedAddresses.FirstOrDefault(x => address.Between(Convert.ToInt32(x.Address, 16), Convert.ToInt32(x.EndAddress ?? x.Address, 16)))?.Key
            });

            if (filterSkips != 0)
                reader.BaseStream.Seek(filterSkips, SeekOrigin.Current);
        }
    }

    /// FindPatternInFile
    private static void FindPatternInFile(ObservableCollection<FinderDataModel> finderData, GameVersion gameVersion, byte[] fileBytes, int filterSize, string filterValues, int filterLimit)
    {
        var usedAddresses = StorageService.BaseAddresses[gameVersion]
            .Select(x => new BaseAddressModel
            {
                Address = x.Value.Address,
                EndAddress = x.Value.EndAddress,
                Key = x.Key
            })
            .Union(
                StorageService.Configs["options"]
                    .Cast<OptionsConfigModel>()
                    .SelectMany(config => config.Options)
                    .SelectMany(option => option.Value.Modifications
                        .Where(mod => mod.Version == gameVersion),
                        (option, mod) => new BaseAddressModel
                        {
                            Address = mod.Address,
                            EndAddress = mod.EndAddress,
                            Key = option.Key
                        }
                    )
            );

        var pattern = filterValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                  .Select(x =>
                                  {
                                      if (x.StartsWith("'") && x.EndsWith("'") && x.Length == 3)
                                          return (byte)x[1];
                                      else if (x == "?")
                                          return null;
                                      else
                                          return filterSize == 1 ? Convert.ToByte(x) : filterSize == 2 ? (object)Convert.ToInt16(x) : Convert.ToInt32(x);
                                  })
                                  .ToList();

        var found = 0;

        for (var i = 0; i <= fileBytes.Length - pattern.Count * filterSize; i += filterSize)
        {
            if (IsPatternMatch(fileBytes, i, pattern, filterSize))
            {
                for (var k = 0; k < pattern.Count; k++)
                {
                    var address = i + k * filterSize;
                    var value = filterSize == 1 ? fileBytes[address] : BitConverter.ToInt32(fileBytes, address);
                    finderData.Add(new FinderDataModel
                    {
                        Address = $"0x{address:X}",
                        Value = value,
                        IsInConfigFile = usedAddresses.Any(x => address.Between(Convert.ToInt32(x.Address, 16), Convert.ToInt32(x.EndAddress ?? x.Address, 16))),
                        Description = usedAddresses.FirstOrDefault(x => address.Between(Convert.ToInt32(x.Address, 16), Convert.ToInt32(x.EndAddress ?? x.Address, 16)))?.Key
                    });
                }

                found++;
                if (found >= filterLimit)
                    break;
            }
        }
    }

    /// IsPatternMatch
    private static bool IsPatternMatch(byte[] fileBytes, int startIndex, List<object?> pattern, int filterSize)
    {
        for (var j = 0; j < pattern.Count; j++)
        {
            if (pattern[j] == null)
                continue;

            if (filterSize == 1)
            {
                if ((byte?)pattern[j] != fileBytes[startIndex + j])
                    return false;
            }
            else if (filterSize == 2)
            {
                var value = BitConverter.ToInt16(fileBytes, startIndex + j * 2);
                if ((short?)pattern[j] != value)
                    return false;
            }
            else if (filterSize == 4)
            {
                var value = BitConverter.ToInt32(fileBytes, startIndex + j * 4);
                if ((int?)pattern[j] != value)
                    return false;
            }
        }
        return true;
    }
}
