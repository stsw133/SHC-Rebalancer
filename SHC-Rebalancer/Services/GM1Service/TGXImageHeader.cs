namespace SHC_Rebalancer.Services.GM1Service;

/// TGXImageHeader
public class TGXImageHeader
{
    public const int ByteSize = 16;

    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort OffsetX { get; set; }
    public ushort OffsetY { get; set; }
    public byte ImagePart { get; set; }
    public byte SubParts { get; set; }
    public ushort TileOffset { get; set; }
    public byte Direction { get; set; }
    public byte HorizontalOffset { get; set; }
    public byte BuildingWidth { get; set; }
    public byte AnimatedColor { get; set; }

    public TGXImageHeader() { }

    public TGXImageHeader(byte[] byteArray)
    {
        if (byteArray.Length != ByteSize)
            throw new ArgumentException($"Invalid header size ({byteArray.Length}), expected {ByteSize} bytes.");

        Width = BitConverter.ToUInt16(byteArray, 0);
        Height = BitConverter.ToUInt16(byteArray, 2);
        OffsetX = BitConverter.ToUInt16(byteArray, 4);
        OffsetY = BitConverter.ToUInt16(byteArray, 6);
        ImagePart = byteArray[8];
        SubParts = byteArray[9];
        TileOffset = BitConverter.ToUInt16(byteArray, 10);
        Direction = byteArray[12];
        HorizontalOffset = byteArray[13];
        BuildingWidth = byteArray[14];
        AnimatedColor = byteArray[15];
    }

    /// ToByteArray
    internal byte[] ToByteArray() => [.. (List<byte>)[
            ..BitConverter.GetBytes(Width),
            ..BitConverter.GetBytes(Height),
            ..BitConverter.GetBytes(OffsetX),
            ..BitConverter.GetBytes(OffsetY),
            ImagePart,
            SubParts,
            ..BitConverter.GetBytes(TileOffset),
            Direction,
            HorizontalOffset,
            BuildingWidth,
            AnimatedColor
        ]];
}
