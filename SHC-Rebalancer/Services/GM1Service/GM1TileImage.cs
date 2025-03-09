using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SHC_Rebalancer.Services.GM1Service;

/// GM1TileImage
public class GM1TileImage : IDisposable
{
    public static int DefaultPadding = 500;

    public GM1TileImage(int width, int height)
    {
        Width = width;
        Height = height;
        _pixelData = new uint[width * height];
    }

    /// AddDiamondTile
    internal void AddDiamondTile(byte[] pixelData, int xOffset, int yOffset)
    {
        int[] rowLengths = [2, 6, 10, 14, 18, 22, 26, 30, 30, 26, 22, 18, 14, 10, 6, 2];
        int bytePos = 0;

        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < rowLengths[y]; x++)
            {
                var pos = ((Width * (y + yOffset)) + x + xOffset + 15 - rowLengths[y] / 2);
                _pixelData[pos] = GM1Converters.ToBgra8888(BitConverter.ToUInt16(pixelData, bytePos));
                bytePos += 2;
            }
        }
    }

    /// AddTileOnTop
    internal void AddTileOnTop(byte[] pixelData, int offsetX, int offsetY)
    {
        uint x = 0, y = 0;
        ushort pixelColor;
        uint colorValue;

        for (var bytePos = 512; bytePos < pixelData.Length;)
        {
            byte token = pixelData[bytePos++];
            byte tokenType = (byte)(token >> 5);
            byte length = (byte)((token & 31) + 1);

            switch (tokenType)
            {
                case 0:
                    for (byte i = 0; i < length; i++)
                    {
                        pixelColor = BitConverter.ToUInt16(pixelData, bytePos);
                        bytePos += 2;
                        colorValue = GM1Converters.ToBgra8888(pixelColor);
                        _pixelData[(Width * (y + offsetY)) + x + offsetX] = colorValue;
                        x++;
                    }
                    break;
                case 1:
                    x += length;
                    break;
                case 2:
                    pixelColor = BitConverter.ToUInt16(pixelData, bytePos);
                    bytePos += 2;
                    colorValue = GM1Converters.ToBgra8888(pixelColor);
                    for (byte i = 0; i < length; i++)
                    {
                        _pixelData[(Width * (y + offsetY)) + x + offsetX] = colorValue;
                        x++;
                    }
                    break;
                case 4:
                    y++;
                    x = 0;
                    break;
                default:
                    break;
            }
        }
    }

    /// GenerateBitmap
    internal unsafe void GenerateBitmap()
    {
        Dispose();
        if (_adjustedHeight == int.MaxValue)
            _adjustedHeight = DefaultPadding;
        
        Height -= _adjustedHeight;
        _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
        _bitmap.Lock();

        try
        {
            var pixelBuffer = (uint*)_bitmap.BackBuffer;
            int index = 0;
            for (var i = Width * _adjustedHeight; i < _pixelData.Length; i++)
                pixelBuffer[index++] = _pixelData[i];
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
        }
        finally
        {
            _bitmap.Unlock();
        }
    }

    /// Dispose
    public void Dispose()
    {
        _bitmap = null;
    }


    private uint[] _pixelData;
    public WriteableBitmap? Bitmap { get => _bitmap; set => _bitmap = value; }
    private WriteableBitmap? _bitmap;
    public int Width { get; }
    public int Height { get; private set; }
    public int AdjustedHeight { get => _adjustedHeight; set => _adjustedHeight = value; }
    private int _adjustedHeight = int.MaxValue;
}
