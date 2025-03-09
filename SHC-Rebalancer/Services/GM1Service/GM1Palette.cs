namespace SHC_Rebalancer.Services.GM1Service;

/// GM1Palette
public class GM1Palette
{
    public const int ByteSize = 5120;
    public const int TableCount = 10;
    public const int ColorTableSize = 512;

    public readonly static int PixelSize = 10;
    public readonly static ushort Height = 8;
    public readonly static ushort Width = 32;

    public int ActualPalette { get; set; } = 0;
    public GM1ColorTable[] ColorTables { get; private set; } = new GM1ColorTable[TableCount];

    public GM1Palette(byte[] data)
    {
        if (data.Length != ByteSize)
            throw new ArgumentException($"Invalid palette size ({data.Length}), expected {ByteSize} bytes.");

        for (var i = 0; i < TableCount; i++)
        {
            var tableBytes = new byte[ColorTableSize];
            Buffer.BlockCopy(data, i * ColorTableSize, tableBytes, 0, ColorTableSize);
            ColorTables[i] = new GM1ColorTable(tableBytes);
        }
    }

    /// ToByteArray
    public byte[] ToByteArray()
    {
        var byteArray = new byte[ByteSize];

        for (int i = 0; i < TableCount; i++)
            Buffer.BlockCopy(ColorTables[i].GetBytes(), 0, byteArray, i * ColorTableSize, ColorTableSize);

        return byteArray;
    }
}
