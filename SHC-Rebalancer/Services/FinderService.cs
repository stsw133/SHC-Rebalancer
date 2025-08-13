using System.Globalization;
using System.IO;
using System.Text;

namespace SHC_Rebalancer;

/// FinderService
internal static class FinderService
{
    /// Find
    internal static IEnumerable<FinderDataModel> Find(GameVersion gameVersion, int filterSize, bool displayAsChar, string? filterAddress, int filterSkips, string? filterValues, int filterLimit)
    {
        var exePath = StorageService.ExePath[gameVersion];

        if (!string.IsNullOrWhiteSpace(filterAddress))
            return FindAddresses(gameVersion, exePath, filterSize, displayAsChar, filterAddress, filterSkips, filterLimit); 
        else if (!string.IsNullOrWhiteSpace(filterValues))
            return FindPatternInFile(gameVersion, File.ReadAllBytes(exePath), filterSize, displayAsChar, filterValues, filterLimit);
        else
            return [];
    }

    /// FindAddresses
    private static IEnumerable<FinderDataModel> FindAddresses(GameVersion gameVersion, string filePath, int filterSize, bool displayAsChar, string filterAddress, int filterSkips, int filterLimit)
    {
        if (!long.TryParse(filterAddress.Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long startAddress))
            yield break;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        if (startAddress >= reader.BaseStream.Length)
            yield break;

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

            yield return new FinderDataModel
            {
                Address = address.ToString("X8"),
                Value = displayAsChar ? ((char)value).ToString() : value,
                IsInConfigFile = isInConfig,
                Description = desc
            };

            if (filterSkips != 0)
                reader.BaseStream.Seek(filterSkips, SeekOrigin.Current);
        }
    }

    /// FindPatternInFile
    private static IEnumerable<FinderDataModel> FindPatternInFile(GameVersion gameVersion, byte[] fileBytes, int filterSize, bool displayAsChar, string filterValues, int filterLimit)
    {
        /// 1) compile pattern to int?[] and prepare only non-null indices/values
        var pattern = ParsePattern(filterValues, filterSize);
        var idx = new List<int>(pattern.Length);
        var vals = new List<int>(pattern.Length);
        for (var j = 0; j < pattern.Length; j++)
            if (pattern[j].HasValue)
            {
                idx.Add(j);
                vals.Add(pattern[j]!.Value);
            }
        var idxArr = idx.Count > 0 ? idx.ToArray() : [];
        var valsArr = vals.Count > 0 ? vals.ToArray() : [];

        /// 2) select the first constant element for pre-check (if exists)
        int m0 = -1, v0 = 0;
        if (idxArr.Length > 0)
        {
            m0 = idxArr[0];
            v0 = valsArr[0];
        }

        /// 3) compile ranges of descriptions (only for hits)
        var ranges = GetUsedAddresses(gameVersion)
            .Select(x => (
                Start: Convert.ToInt32(x.Address, 16),
                End: Convert.ToInt32(x.EndAddress ?? x.Address, 16),
                x.Key))
            .ToArray();

        var found = 0;
        var step = filterSize;
        var lastStart = fileBytes.Length - pattern.Length * filterSize;

        /// 4) fast path: pattern is all wildcards — matches everywhere
        if (idxArr.Length == 0)
        {
            for (var i = 0; i <= lastStart && found < filterLimit; i += step)
            {
                for (var k = 0; k < pattern.Length; k++)
                {
                    var address = i + k * filterSize;
                    var value = ReadValueAsInt(fileBytes, address, filterSize);

                    string? desc = null;
                    for (var r = 0; r < ranges.Length; r++)
                    {
                        var rr = ranges[r];
                        if (address >= rr.Start && address <= rr.End) { desc = rr.Key; break; }
                    }

                    yield return new FinderDataModel
                    {
                        Address = $"0x{address:X}",
                        Value = displayAsChar ? ((char)value).ToString() : value,
                        IsInConfigFile = desc != null,
                        Description = desc
                    };
                }
                found++;
            }
            yield break;
        }

        /// 5) Iterate through the file bytes, checking for matches
        for (int i = 0; i <= lastStart; i += step)
        {
            /// PRE-CHECK: check only the first value from the pattern
            int preOffset = i + m0 * filterSize;
            int preValue = ReadValueAsInt(fileBytes, preOffset, filterSize);
            if (preValue != v0) continue;

            /// full match check if pre-check passed
            if (!IsPatternMatch(fileBytes, i, idxArr, valsArr, filterSize))
                continue;

            /// report the entire "piece" of the pattern (including wildcards)
            for (int k = 0; k < pattern.Length; k++)
            {
                var address = i + k * filterSize;
                var value = ReadValueAsInt(fileBytes, address, filterSize);

                string? desc = null;
                for (int r = 0; r < ranges.Length; r++)
                {
                    var rr = ranges[r];
                    if (address >= rr.Start && address <= rr.End) { desc = rr.Key; break; }
                }

                yield return new FinderDataModel
                {
                    Address = $"0x{address:X}",
                    Value = displayAsChar ? ((char)value).ToString() : value,
                    IsInConfigFile = desc != null,
                    Description = desc
                };
            }

            if (++found >= filterLimit)
                yield break;
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
    private static bool IsPatternMatch(byte[] src, int startIndex, int[] idx, int[] vals, int size)
    {
        for (int m = 0; m < idx.Length; m++)
        {
            int offset = startIndex + idx[m] * size;
            int v = ReadValueAsInt(src, offset, size);
            if (v != vals[m]) return false;
        }
        return true;
    }

    /// NormalizeToFilterSize
    private static int NormalizeToFilterSize(int value, int filterSize) => filterSize switch
    {
        1 => checked((byte)value),
        2 => checked((short)value),
        4 => value,
        _ => throw new NotSupportedException($"filterSize={filterSize} not supported.")
    };

    /// ParseEscapedChar
    private static char ParseEscapedChar(string s)
    {
        if (s.Length == 1)
            return s[0];

        return ParseEscapedString(s) switch
        {
            var str when str.Length > 0 => str[0],
            _ => '\0'
        };
    }

    /// ParseEscapedString
    private static string ParseEscapedString(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c != '\\') { sb.Append(c); continue; }
            if (i + 1 >= s.Length) { sb.Append('\\'); break; }

            var n = s[++i];
            sb.Append(n switch
            {
                '\\' => '\\',
                '"' => '"',
                '\'' => '\'',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                // \xHH (optional)
                'x' when i + 2 < s.Length && IsHex(s[i + 1]) && IsHex(s[i + 2]) =>
                    (char)Convert.ToInt32(s.Substring(i + 1, 2), 16),
                _ => n
            });

            if (n == 'x' && i + 2 < s.Length && IsHex(s[i + 1]) && IsHex(s[i + 2]))
                i += 2;
        }
        return sb.ToString();

        static bool IsHex(char ch) =>
            (ch >= '0' && ch <= '9') ||
            (ch >= 'a' && ch <= 'f') ||
            (ch >= 'A' && ch <= 'F');
    }

    /// ParsePattern
    private static int?[] ParsePattern(string filterValues, int filterSize)
    {
        var list = new List<int?>();

        foreach (var token in Tokenize(filterValues))
        {
            var x = token.Trim();
            if (x.Length == 0) continue;

            if (x == "?")
            {
                list.Add(null);
                continue;
            }

            // 'a'
            if (x.Length >= 3 && x.StartsWith('\'') && x.EndsWith('\''))
            {
                var inner = x[1..^1];
                var ch = ParseEscapedChar(inner);
                list.Add(NormalizeToFilterSize(ch, filterSize));
                continue;
            }

            // "abc"
            if (x.Length >= 2 && x.StartsWith('"') && x.EndsWith('"'))
            {
                var inner = x[1..^1];
                var text = ParseEscapedString(inner);
                foreach (var ch in text)
                    list.Add(NormalizeToFilterSize(ch, filterSize));
                continue;
            }

            // dec/hex number
            int number = x.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.Parse(x.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture);

            list.Add(NormalizeToFilterSize(number, filterSize));
        }

        return [.. list];
    }

    /// ReadValueAsInt
    private static int ReadValueAsInt(byte[] fileBytes, int offset, int size) => size switch
    {
        1 => fileBytes[offset],
        2 => BitConverter.ToInt16(fileBytes, offset),
        4 => BitConverter.ToInt32(fileBytes, offset),
        _ => throw new NotSupportedException($"filterSize={size} not supported.")
    };

    /// Tokenize
    private static IEnumerable<string> Tokenize(string input)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inDq = false, inSq = false, escape = false;

        foreach (var ch in input)
        {
            if (escape)
            {
                sb.Append(ch);
                escape = false;
                continue;
            }

            if (ch == '\\')
            {
                sb.Append(ch);
                escape = true;
                continue;
            }

            if (ch == '"' && !inSq) { inDq = !inDq; sb.Append(ch); continue; }
            if (ch == '\'' && !inDq) { inSq = !inSq; sb.Append(ch); continue; }

            if (ch == ',' && !inDq && !inSq)
            {
                var piece = sb.ToString().Trim();
                if (piece.Length > 0) list.Add(piece);
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        var last = sb.ToString().Trim();
        if (last.Length > 0) list.Add(last);

        return list;
    }
}
