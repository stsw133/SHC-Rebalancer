using System.IO;
using System.Text;
using System.Text.Json;

namespace SHC_Rebalancer;

/// BinaryPatchService
internal static class BinaryPatchService
{
    private static FileStream? _fs;
    private static BinaryReader? _reader;
    private static BinaryWriter? _writer;

    /// Open
    internal static void Open(string path)
    {
        _fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        _reader = new BinaryReader(_fs);
        _writer = new BinaryWriter(_fs);
    }

    /// Close
    internal static void Close()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _fs?.Dispose();

        _writer = null;
        _reader = null;
        _fs = null;
    }

    /// ExtendFileIfNeeded
    internal static void ExtendFileIfNeeded(long requiredLength)
    {
        if (_fs == null)
            throw new InvalidOperationException("File not opened.");

        if (requiredLength > _fs.Length)
            _fs.SetLength(requiredLength);
    }

    /// WriteIfDifferent
    internal static void WriteIfDifferent(int address, object? newValue, int size, string? description)
    {
        if (newValue == null || address == default || _fs == null || _reader == null || _writer == null)
            return;

        if (address + size > _fs.Length)
        {
            Console.WriteLine($"Address {address:X} is out of bounds (File Length: {_fs.Length}). Extending file...");
            _fs.SetLength(address + size);
        }

        _fs.Seek(address, SeekOrigin.Begin);
        var oldValue = ReadValueAsBytes(newValue, size);
        var newValueBytes = ConvertToBytes(newValue, size);

        if (!oldValue.SequenceEqual(newValueBytes))
        {
            Console.WriteLine(
                $"Address {address:X}, " +
                $"old value: [{FormatValue(oldValue)}], " +
                $"new value: [{FormatValue(newValueBytes)}], " +
                $"description: {description}");

            _fs.Seek(address, SeekOrigin.Begin);
            _writer.Write(newValueBytes);
        }
    }

    /// ReadValueAsBytes
    private static byte[] ReadValueAsBytes(object newValue, int size)
    {
        if (newValue is string str)
        {
            var tmp = ConvertStringToBytesWithAutoPadding(str, 4);
            return _reader!.ReadBytes(tmp.Length);
        }
        else if (newValue is Array arr)
        {
            var totalBytes = arr.Length * size;
            return _reader!.ReadBytes(totalBytes);
        }
        else
        {
            return _reader!.ReadBytes(size);
        }
    }

    /// ConvertToBytes
    private static byte[] ConvertToBytes(object newValue, int size)
    {
        if (newValue is string s)
        {
            return ConvertStringToBytesWithAutoPadding(s, 4);
        }

        if (newValue is Array arr)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            foreach (var element in arr)
            {
                var singleBytes = ConvertSingleValueToBytes(element, size);
                bw.Write(singleBytes);
            }
            return ms.ToArray();
        }

        return ConvertSingleValueToBytes(newValue, size);
    }

    /// ConvertSingleValueToBytes
    private static byte[] ConvertSingleValueToBytes(object? value, int size)
    {
        if (value == null)
            return [];

        int intValue;

        if (value is IConvertible conv && value is not string)
        {
            var d = conv.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            intValue = (int)d;
        }
        else
        {
            if (value is bool b) intValue = b ? 1 : 0;
            else if (int.TryParse(value.ToString(), out var parsedInt)) intValue = parsedInt;
            else throw new InvalidOperationException("Unsupported type in ConvertSingleValueToBytes.");
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        switch (size)
        {
            case 1:
                bw.Write((byte)intValue);
                break;
            case 2:
                bw.Write((short)intValue);
                break;
            case 4:
                bw.Write(intValue);
                break;
            default:
                throw new NotSupportedException($"Size={size} not supported by this method.");
        }

        return ms.ToArray();
    }

    /// ConvertStringToBytesWithAutoPadding
    internal static byte[] ConvertStringToBytesWithAutoPadding(string input, int alignment)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (alignment <= 0)
            throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be greater than 0.");

        var stringBytes = Encoding.ASCII.GetBytes(input);
        var requiredLength = stringBytes.Length + 1;
        var totalLength = (requiredLength + alignment - 1) / alignment * alignment;

        var result = new byte[totalLength];
        Array.Copy(stringBytes, result, stringBytes.Length);

        return result;
    }

    /// FormatValue
    private static string FormatValue(byte[] valueBytes) => string.Join(", ", valueBytes.Select(b => b.ToString("X2")));

    /// GetAddressByEnum
    internal static int GetAddressByEnum<T>(BaseAddressModel baseAddress, string key, int skipBy = 1) where T : Enum
    {
        try
        {
            return Convert.ToInt32(baseAddress.Address, 16) + (Convert.ToInt32(Enum.Parse(typeof(T), key)) * baseAddress.Size * skipBy);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating address for {key}: {ex.Message}");
            throw;
        }
    }

    /// GetNumberOrArray
    internal static object GetNumberOrArray(object? obj, int size)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (obj is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Array)
            {
                var numbers = json.EnumerateArray()
                                  .Where(e => e.ValueKind == JsonValueKind.Number)
                                  .Select(e => e.GetInt32())
                                  .ToArray();
                return ConvertArray(numbers, size);
            }

            if (json.ValueKind == JsonValueKind.Number)
            {
                var singleNumber = json.GetInt32();
                return ConvertSingleValueToType(singleNumber, size);
            }

            if (json.ValueKind == JsonValueKind.String && int.TryParse(json.GetString(), out int parsedJsonString))
                return ConvertSingleValueToType(parsedJsonString, size);

            throw new InvalidOperationException($"Unsupported JsonElement kind: {json.ValueKind}");
        }

        if (obj is Array arr)
        {
            var numbers = arr.OfType<object>().Select(ConvertSingleNumber).ToArray();
            return ConvertArray(numbers, size);
        }

        var singleVal = ConvertSingleNumber(obj);
        return ConvertSingleValueToType(singleVal, size);
    }

    /// ConvertSingleNumber
    private static int ConvertSingleNumber(object obj)
    {
        if (obj is bool b)
            return b ? 1 : 0;

        if (obj is IConvertible conv && obj is not string)
        {
            var d = conv.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
            return (int)d;
        }

        if (int.TryParse(obj.ToString(), out int parsed))
            return parsed;

        throw new InvalidOperationException(
            $"Unsupported value for single-number conversion: {obj?.GetType().Name}");
    }

    /// ConvertArray
    private static object ConvertArray(IEnumerable<int> numbers, int size) => size switch
    {
        1 => numbers.Select(n => (byte)n).ToArray(),
        2 => numbers.Select(n => (short)n).ToArray(),
        4 => numbers.ToArray(),
        _ => throw new NotSupportedException($"Size={size} not supported for array.")
    };

    /// ConvertSingleValueToType
    private static object ConvertSingleValueToType(int value, int size) => size switch
    {
        1 => (byte)value,
        2 => (short)value,
        4 => value,
        _ => throw new NotSupportedException($"Size={size} not supported for single value.")
    };
}
