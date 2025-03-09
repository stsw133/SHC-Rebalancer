namespace SHC_Rebalancer.Services.GM1Service;

/// GM1ColorTable
public class GM1ColorTable
{
    public const int ByteSize = 512;
    public const int ColorCount = 256;

    public ushort[] Colors { get; private set; } = new ushort[ColorCount];

    public GM1ColorTable(byte[] byteArray)
    {
        if (byteArray.Length != ByteSize)
            throw new ArgumentException($"Invalid byte array length ({byteArray.Length}). Expected {ByteSize} bytes.");

        for (var i = 0; i < ColorCount; i++)
            Colors[i] = BitConverter.ToUInt16(byteArray, i * 2);
    }

    public GM1ColorTable(ushort[] ushortArray)
    {
        if (ushortArray.Length != ColorCount)
            throw new ArgumentException($"Invalid color array length ({ushortArray.Length}). Expected {ColorCount} colors.");

        Colors = (ushort[])ushortArray.Clone();
    }

    /// GetBytes
    public byte[] GetBytes()
    {
        var byteArray = new byte[ByteSize];
        Buffer.BlockCopy(Colors, 0, byteArray, 0, ByteSize);
        return byteArray;
    }

    /// Clone
    public GM1ColorTable Clone() => new(Colors);
}
