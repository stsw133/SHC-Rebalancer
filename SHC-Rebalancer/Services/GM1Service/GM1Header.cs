namespace SHC_Rebalancer.Services.GM1Service;

/// GM1Header
public class GM1Header
{
    public const int ByteSize = 88;

    public enum GM1DataType : uint
    {
        Interface = 1,
        Animations = 2,
        TilesObject = 3,
        Font = 4,
        NoCompression1 = 5,
        TGXConstSize = 6,
        NoCompression2 = 7
    };

    public GM1Header(byte[] byteArray)
    {
        if (byteArray.Length != ByteSize)
            throw new ArgumentException($"Invalid header size ({byteArray.Length}), expected {ByteSize} bytes.");

        var pos = 0;
        Unknown00 = BitConverter.ToUInt32(byteArray, 0);
        Unknown01 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown02 = BitConverter.ToUInt32(byteArray, pos += 4);
        PictureCount = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown04 = BitConverter.ToUInt32(byteArray, pos += 4);
        DataType = (GM1DataType)BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown06 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown07 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown08 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown09 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown10 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown11 = BitConverter.ToUInt32(byteArray, pos += 4);
        Width = BitConverter.ToUInt32(byteArray, pos += 4);
        Height = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown14 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown15 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown16 = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown17 = BitConverter.ToUInt32(byteArray, pos += 4);
        OriginX = BitConverter.ToUInt32(byteArray, pos += 4);
        OriginY = BitConverter.ToUInt32(byteArray, pos += 4);
        DataSize = BitConverter.ToUInt32(byteArray, pos += 4);
        Unknown21 = BitConverter.ToUInt32(byteArray, pos += 4);
    }

    /// ToByteArray
    public byte[] ToByteArray()
    {
        var buffer = new byte[ByteSize];
        var pos = 0;

        Buffer.BlockCopy(BitConverter.GetBytes(Unknown00),      0, buffer, pos += 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown01),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown02),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(PictureCount),   0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown04),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((uint)DataType), 0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown06),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown07),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown08),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown09),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown10),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown11),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Width),          0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Height),         0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown14),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown15),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown16),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown17),      0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(OriginX),        0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(OriginY),        0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(DataSize),       0, buffer, pos += 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(Unknown21),      0, buffer, pos += 4, 4);

        return buffer;
    }

    public string? Name { get; set; }
    public uint Unknown00 { get; set; }
    public uint Unknown01 { get; set; }
    public uint Unknown02 { get; set; }
    public uint PictureCount { get; set; }
    public uint Unknown04 { get; set; }
    public GM1DataType DataType { get; set; }
    public uint Unknown06 { get; set; }
    public uint Unknown07 { get; set; }
    public uint Unknown08 { get; set; }
    public uint Unknown09 { get; set; }
    public uint Unknown10 { get; set; }
    public uint Unknown11 { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Unknown14 { get; set; }
    public uint Unknown15 { get; set; }
    public uint Unknown16 { get; set; }
    public uint Unknown17 { get; set; }
    public uint OriginX { get; set; }
    public uint OriginY { get; set; }
    public uint DataSize { get; set; }
    public uint Unknown21 { get; set; }
}
