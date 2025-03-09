using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace SHC_Rebalancer.Services.GM1Service;

/// GM1Utils
internal static class GM1Converters
{
    /// ByteArrayToBitmap
    internal static unsafe WriteableBitmap ByteArrayToBitmap(byte[] byteArray, int width, int height)
    {
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        bitmap.Lock();

        try
        {
            var pointer = (uint*)bitmap.BackBuffer;
            var pos = 0;

            for (var bytePos = 0; bytePos < byteArray.Length; bytePos += 2)
                pointer[pos++] = ToBgra8888(BitConverter.ToUInt16(byteArray, bytePos));
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        }
        finally
        {
            bitmap.Unlock();
        }

        return bitmap;
    }

    /// ColorListToByteArray
    internal static byte[] ColorListToByteArray(List<ushort> colors, int width, int height)
    {
        var length = width * height;
        var byteArray = new byte[length * 2];
        Buffer.BlockCopy(colors.ToArray(), 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    /// DecodeGM1Image
    public static unsafe WriteableBitmap DecodeGM1Image(byte[] byteArray, int width, int height, GM1ColorTable colorTable)
    {
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        bitmap.Lock();

        try
        {
            var pixelData = (uint*)bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride / 4;
            int position = 0;
            int nextLine = stride;

            for (var bytePos = 0; bytePos < byteArray.Length; bytePos++)
            {
                var token = byteArray[bytePos];
                var tokenType = token >> 5;
                var length = (token & 31) + 1;

                if (tokenType == 4)
                {
                    position = nextLine;
                    nextLine += stride;
                }
                else if (tokenType == 1)
                {
                    position += length;
                }
                else
                {
                    var readLength = (tokenType == 0) ? length : 1;
                    var writeLength = (tokenType == 2) ? length : 1;

                    for (var i = 0; i < readLength; i++)
                    {
                        ushort colorArgb1555 = colorTable != null
                            ? colorTable.Colors[byteArray[bytePos++]]
                            : BitConverter.ToUInt16(byteArray, bytePos += 2);

                        uint convertedColor = ToBgra8888(colorArgb1555);

                        for (int j = 0; j < writeLength; j++)
                        {
                            if (position < stride * height)
                                pixelData[position] = convertedColor;
                            position++;
                        }
                    }
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        }
        finally
        {
            bitmap.Unlock();
        }

        return bitmap;
    }

    /// FromArgb1555
    internal static void FromArgb1555(ushort color, out byte r, out byte g, out byte b, out byte a)
    {
        a = (byte)((color & 0x8000) != 0 ? 255 : 0);
        r = (byte)((color >> 10) & 0x1F << 3);
        g = (byte)((color >> 5) & 0x1F << 3);
        b = (byte)((color & 0x1F) << 3);
    }

    /// ToArgb1555
    internal static ushort ToArgb1555(byte r, byte g, byte b, byte a) => (ushort)((a > 127 ? 0x8000 : 0) | ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3));

    /// ToBgra8888
    internal static uint ToBgra8888(ushort color)
    {
        FromArgb1555(color, out var r, out var g, out var b, out var a);
        return (uint)(b | g << 8 | r << 16 | a << 24);
    }

    /// ToRgba8888
    internal static uint ToRgba8888(ushort color)
    {
        FromArgb1555(color, out var r, out var g, out var b, out var a);
        return (uint)(r | g << 8 | b << 16 | a << 24);
    }
}
