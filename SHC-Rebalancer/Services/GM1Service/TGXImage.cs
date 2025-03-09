using System.Windows.Media.Imaging;

namespace SHC_Rebalancer.Services.GM1Service;

/// TGXImage
public class TGXImage
{
    public uint Height { get; set; }
    public uint Width { get; set; }
    public TGXImageHeader Header { get; set; } = new();
    public uint Offset { get; set; }
    public uint ByteSize { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public WriteableBitmap? Bitmap { get; set; }

    /// EncodeWithPalette
    internal void EncodeWithPalette(List<ushort> colors, int width, int height, GM1Palette palette, List<ushort>[]? colorTables = null)
    {
        var encoded = GM1Utils.ImgToGM1ByteArray(colors, width, height, Header.AnimatedColor, palette, colorTables);
        ImageData = [.. encoded];
    }

    /// EncodeWithoutPalette
    public void EncodeWithoutPalette(List<ushort> colors, int width, int height)
    {
        var encodedBytes = GM1Utils.ImgToGM1ByteArray(colors, width, height, 1);
        if (encodedBytes.Count < ByteSize)
            encodedBytes.AddRange(new byte[ByteSize - encodedBytes.Count]);
        else if (encodedBytes.Count > ByteSize)
            encodedBytes = [.. encodedBytes.Take((int)ByteSize)];

        ImageData = [.. encodedBytes];
    }

}
