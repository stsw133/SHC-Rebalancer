using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SHC_Rebalancer.Services.GM1Service;

/// Utility
internal static class GM1Utils
{
    public static GM1Header.GM1DataType datatype;

    /// LoadImageData
    internal static Image<Rgba32> LoadImageData(string filePath) => Image.Load<Rgba32>(filePath);

    /// LoadImage
    internal static List<ushort> LoadImage(
        string filename,
        ref int width,
        ref int height,
        int animatedColor = 1,
        int pixelsize = 1,
        uint type = 0,
        int offsetx = 0,
        int offsety = 0)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(filename);
        return LoadImage(image, ref width, ref height, animatedColor, pixelsize, type, offsetx, offsety);
    }

    /// LoadImage
    internal static List<ushort> LoadImage(
        Image<Rgba32> image,
        ref int width,
        ref int height,
        int animatedColor = 1,
        int pixelsize = 1,
        uint type = 0,
        int offsetx = 0,
        int offsety = 0)
    {
        List<ushort> colors = [];

        try
        {
            if (width == 0) width = image.Width;
            if (height == 0) height = image.Height;

            GM1Header.GM1DataType dataType = (GM1Header.GM1DataType)type;
            byte a = (animatedColor >= 1
                || dataType == GM1Header.GM1DataType.TilesObject
                || dataType == GM1Header.GM1DataType.Font
                || dataType == GM1Header.GM1DataType.Animations
                || dataType == GM1Header.GM1DataType.TGXConstSize
                || dataType == GM1Header.GM1DataType.NoCompression1
                || dataType == GM1Header.GM1DataType.NoCompression2
                || dataType == GM1Header.GM1DataType.Interface) ? byte.MaxValue : byte.MinValue;

            for (int y = offsety; y < height + offsety; y += pixelsize)
            {
                for (int x = offsetx; x < width + offsetx; x += pixelsize)
                {
                    Rgba32 pixel = image[x, y];

                    if (pixel.A == 0)
                    {
                        colors.Add(32767);
                    }
                    else
                    {
                        colors.Add(GM1Converters.ToArgb1555(pixel.R, pixel.G, pixel.B, a));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, "Error");
        }

        return colors;
    }

    /// LoadImageAsBitmap
    internal unsafe static WriteableBitmap LoadImageAsBitmap(
        Image<Rgba32> image,
        ref int width,
        ref int height,
        int offsetx = 0,
        int offsety = 0)
    {
        if (width == 0) width = image.Width;
        if (height == 0) height = image.Height;

        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        bitmap.Lock();

        try
        {
            var pointer = (uint*)bitmap.BackBuffer;
            var stride = bitmap.BackBufferStride / 4; // Każdy piksel zajmuje 2 bajty

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rgba32 pixel = image[x + offsetx, y + offsety];

                    ushort encodedColor = GM1Converters.ToArgb1555(
                        pixel.R,
                        pixel.G,
                        pixel.B,
                        (byte)(pixel.A > 127 ? 255 : 0) // Obsługa alfa
                    );

                    pointer[(stride * y) + x] = encodedColor;
                }
            }

            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
        }
        finally
        {
            bitmap.Unlock();
        }

        return bitmap;
    }

    /// ImgToGM1ByteArray
    internal static List<byte> ImgToGM1ByteArray(List<ushort> colors, int width, int height, int animatedColor, GM1Palette palette = null, List<ushort>[] paletteImages = null)
    {
        int transparent = 32767;
        ushort alpha = (animatedColor == 0) ? (ushort)0b1000_0000_0000_0000 : (ushort)0b0;
        List<byte> array = [];
        byte length;
        byte header;
        int countSamePixel;
        bool newline = false;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width;)
            {
                countSamePixel = 0;

                for (int z = j; z < width; z++)
                {
                    if (colors[i * width + z] == transparent)
                    {
                        newline = true;
                        countSamePixel++;
                    }
                    else
                    {
                        newline = false;
                        break;
                    }
                }

                if (newline == true && countSamePixel == width)
                {
                    if (datatype == GM1Header.GM1DataType.TGXConstSize || datatype == GM1Header.GM1DataType.TilesObject || datatype == GM1Header.GM1DataType.Interface)
                    {
                        if (!CheckIfAllLinesUnderTransparent(colors, transparent, (i + 1) * width) || datatype == GM1Header.GM1DataType.TilesObject)
                        {
                            header = 0b0010_0000;
                            var dummy = countSamePixel;
                            while (dummy / 32 > 0)
                            {
                                length = 0b0001_1111;
                                array.Add((byte)(header | length));

                                dummy -= 32;
                            }
                            if (dummy != 0)
                            {
                                length = (byte)(dummy - 1);
                                array.Add((byte)(header | length));
                            }
                        }
                    }

                    array.Add(0b1000_0000);

                    j = width;
                    continue;
                }
                else
                {
                    var dummy = countSamePixel;
                    header = 0b0010_0000;
                    while (dummy / 32 > 0)
                    {
                        length = 0b0001_1111;
                        array.Add((byte)(header | length));
                        dummy -= 32;
                    }
                    if (dummy != 0)
                    {
                        length = (byte)(dummy - 1);
                        array.Add((byte)(header | length));
                    }
                    j += countSamePixel;
                    if (j == width)
                    {
                        array.Add(0b1000_0000);

                        continue;
                    }

                    countSamePixel = 1;
                    int repeatingPixel = 1;

                    for (int z = j + 1; z < width; z++)
                    {
                        if (colors[i * width + z] != transparent && colors[i * width + z - 1] != colors[i * width + z])
                        {
                            if (repeatingPixel > 2)
                            {
                                break;
                            }
                            else if (repeatingPixel > 1)
                            {
                                countSamePixel += repeatingPixel - 1;
                                repeatingPixel = 1;
                            }

                            countSamePixel++;
                        }
                        else if (colors[i * width + z] != transparent && colors[i * width + z - 1] == colors[i * width + z])
                        {
                            repeatingPixel++;
                            if (countSamePixel > 1 && repeatingPixel > 2)
                            {
                                countSamePixel--;
                                repeatingPixel = 1;
                                break;
                            }
                            if (z == width - 1)
                            {
                                countSamePixel++;
                            }
                        }
                        else
                        {
                            if (repeatingPixel < 3)
                            {
                                countSamePixel += repeatingPixel - 1;
                            }
                            break;
                        }

                    }
                    if (repeatingPixel > 2)
                    {
                        countSamePixel = repeatingPixel;
                        header = 0b0100_0000;
                        dummy = countSamePixel;

                        while (dummy / 32 > 0)
                        {
                            length = 0b0001_1111;
                            array.Add((byte)(header | length));

                            var color = (ushort)(colors[j + i * width + 1] | alpha);
                            if (palette == null)
                            {
                                array.AddRange(BitConverter.GetBytes(color));
                            }
                            else
                            {
                                byte positioninColortable = FindColorPositionInPalette(color, j + i * width + 1, palette, paletteImages);
                                array.Add(positioninColortable);
                            }

                            dummy -= 32;
                        }
                        if (dummy != 0)
                        {
                            length = (byte)(dummy - 1);
                            array.Add((byte)(header | length));

                            var color = (ushort)(colors[j + i * width + 1] | alpha);
                            if (palette == null)
                            {
                                array.AddRange(BitConverter.GetBytes(color));
                            }
                            else
                            {
                                byte positioninColortable = FindColorPositionInPalette(color, j + i * width + 1, palette, paletteImages);
                                array.Add(positioninColortable);
                            }
                        }
                    }
                    else
                    {
                        header = 0b0000_0000;
                        dummy = countSamePixel;
                        int zaehler = 0;
                        while (dummy / 32 > 0)
                        {
                            length = 0b0001_1111;
                            array.Add((byte)(header | length));
                            for (int a = 0; a < 32; a++)
                            {
                                var color = (ushort)(colors[j + zaehler + i * width] | alpha);
                                if (palette == null)
                                {
                                    array.AddRange(BitConverter.GetBytes(color));
                                }
                                else
                                {
                                    byte positioninColortable = FindColorPositionInPalette(color, j + zaehler + i * width, palette, paletteImages);
                                    array.Add(positioninColortable);
                                }

                                dummy--;
                                zaehler++;
                            }
                        }
                        if (dummy != 0)
                        {
                            length = (byte)(dummy - 1);
                            array.Add((byte)(header | length));
                            for (int a = 0; a < dummy; a++)
                            {
                                var color = (ushort)(colors[j + zaehler + i * width] | alpha);
                                if (palette == null)
                                {
                                    array.AddRange(BitConverter.GetBytes(color));
                                }
                                else
                                {
                                    byte positioninColortable = FindColorPositionInPalette(color, j + zaehler + i * width, palette, paletteImages);
                                    array.Add(positioninColortable);
                                }

                                zaehler++;
                            }
                        }
                    }

                    j += countSamePixel;
                    if (j == width)
                    {
                        array.Add(0b1000_0000);
                    }
                }
            }
        }

        return array;
    }

    /// FindColorPositionInPalette
    private static byte FindColorPositionInPalette(ushort color, int position, GM1Palette palette, List<ushort>[] paletteImages)
    {
        byte newPosition = 0;
        if (paletteImages == null)
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                if (color == palette.ColorTables[0].Colors[i])
                {
                    newPosition = i;
                    break;
                }
            }
        }
        else
        {
            List<byte> positions = new List<byte>();
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                if (color == palette.ColorTables[0].Colors[i])
                {
                    positions.Add(i);
                }
            }
            if (positions.Count > 1 || positions.Count == 0)
            {
                for (int j = 0; j < 9; j++)
                {
                    List<byte> otherPositions = new List<byte>();
                    for (byte i = 0; i < byte.MaxValue; i++)
                    {
                        if (paletteImages[j][position] == palette.ColorTables[j + 1].Colors[i])
                        {
                            otherPositions.Add(i);
                        }
                    }
                    if (otherPositions.Count == 1)
                    {
                        newPosition = otherPositions[0];
                        break;
                    }
                    else if (otherPositions.Count > 1)
                    {
                        newPosition = otherPositions[0];
                    }
                }
            }
            else
            {
                newPosition = positions[0];
            }
        }
        if (newPosition == 0)
        {

        }
        return newPosition;
    }

    /// CheckIfAllLinesUnderTransparent
    private static bool CheckIfAllLinesUnderTransparent(List<ushort> colors, int transparent, int offset)
    {
        for (int i = offset; i < colors.Count; i++)
        {
            if (colors[i] != transparent)
            {
                return false;
            }
        }
        return true;
    }

    public static int YOffsetBefore { get; internal set; }
    public static int XOffsetBefore { get; internal set; }

    private static int biggestHeight = 0;

    /// ConvertImgToTiles
    internal static List<TGXImage> ConvertImgToTiles(List<ushort> list, ushort width, ushort height, List<TGXImage> oldList)
    {
        List<TGXImage> newImageList = new List<TGXImage>();

        int partwidth = width / 30;
        int totalTiles = partwidth * partwidth;

        int savedOffsetX = width / 2;
        int xOffset = savedOffsetX;
        int yOffset = height - 16;
        int partsPerLine = 1;
        int counter = 0;

        List<byte> arrayByte;
        var halfreached = false;
        datatype = GM1Header.GM1DataType.TilesObject;

        int[] array = [
            2, 6, 10, 14, 18, 22, 26, 30,
            30, 26, 22, 18, 14, 10, 6, 2
        ];

        for (int part = 0; part < totalTiles; part++)
        {
            counter++;

            arrayByte = [];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < array[y]; x++)
                {
                    int number = ((width * (y + yOffset)) + x + xOffset - array[y] / 2);
                    var color = list[number];
                    arrayByte.AddRange(BitConverter.GetBytes(color));

                    list[number] = 32767;
                }
            }

            var newImage = new TGXImage
            {
                Header = new TGXImageHeader()
            };
            newImage.Header.Direction = 0;
            newImage.Header.Height = 16;
            newImage.Header.Width = 30;
            //newImage.OffsetX = (ushort)(xOffset + XOffsetBefore);
            //newImage.OffsetY = (ushort)(yOffset + YOffsetBefore);
            newImage.Header.SubParts = (byte)totalTiles;
            newImage.Header.ImagePart = (byte)part;

            if (totalTiles == 1) halfreached = true;

            if (halfreached)
            {
                if (counter == 1)
                {
                    if (part == totalTiles - 1)
                    {
                        newImage.Header.BuildingWidth = 30;
                        newImage.Header.Direction = 1;
                        int imageOnTopwidth = 30;
                        int imageOnTopheight = yOffset + 7;
                        int imageOnTopOffsetX = xOffset - 15;
                        List<ushort> colorListImgOnTop = GetColorList(list, imageOnTopwidth, imageOnTopheight, imageOnTopOffsetX, width);

                        if (colorListImgOnTop.Count != 0)
                        {
                            var byteArrayImgonTop = ImgToGM1ByteArray(colorListImgOnTop, imageOnTopwidth, colorListImgOnTop.Count / imageOnTopwidth, 1);
                            arrayByte.AddRange(byteArrayImgonTop);
                            newImage.Header.TileOffset = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 10 - 16 - 1);
                            if (newImage.Header.TileOffset == ushort.MaxValue) newImage.Header.TileOffset = 0;
                            newImage.Header.Height = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 9);
                        }
                    }
                    else
                    {
                        newImage.Header.BuildingWidth = 16;
                        newImage.Header.Direction = 2;
                        int imageOnTopwidth = 16;
                        int imageOnTopheight = yOffset + 7;
                        int imageOnTopOffsetX = xOffset - 15;
                        List<ushort> colorListImgOnTop = GetColorList(list, imageOnTopwidth, imageOnTopheight, imageOnTopOffsetX, width);

                        if (colorListImgOnTop.Count != 0)
                        {
                            var byteArrayImgonTop = ImgToGM1ByteArray(colorListImgOnTop, imageOnTopwidth, colorListImgOnTop.Count / imageOnTopwidth, 1);
                            arrayByte.AddRange(byteArrayImgonTop);
                            newImage.Header.TileOffset = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 10 - 16 - 1);
                            if (newImage.Header.TileOffset == ushort.MaxValue) newImage.Header.TileOffset = 0;
                            newImage.Header.Height = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 9);
                        }
                    }
                }
                else if (counter == partsPerLine)
                {
                    newImage.Header.BuildingWidth = 16;
                    newImage.Header.Direction = 3;
                    int imageOnTopwidth = 16;
                    int imageOnTopheight = yOffset + 7;
                    int imageOnTopOffsetX = xOffset;
                    List<ushort> colorListImgOnTop = GetColorList(list, imageOnTopwidth, imageOnTopheight, imageOnTopOffsetX - 1, width);

                    if (colorListImgOnTop.Count != 0)
                    {
                        var byteArrayImgonTop = ImgToGM1ByteArray(colorListImgOnTop, imageOnTopwidth, colorListImgOnTop.Count / imageOnTopwidth, 1);
                        arrayByte.AddRange(byteArrayImgonTop);
                        newImage.Header.Height = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 9);
                        newImage.Header.TileOffset = (ushort)(colorListImgOnTop.Count / imageOnTopwidth + 10 - 16 - 1);
                        if (newImage.Header.TileOffset == ushort.MaxValue) newImage.Header.TileOffset = 0;
                    }

                    newImage.Header.HorizontalOffset = 14;
                }
            }

            newImageList.Add(newImage);
            newImage.ImageData = arrayByte.ToArray();
            xOffset += 32;

            if (counter == partsPerLine)
            {
                yOffset -= 8;
                counter = 0;

                xOffset = savedOffsetX;
                if (partsPerLine == partwidth - 1 && !halfreached)
                {
                    halfreached = true;
                    partsPerLine += 2;
                    xOffset = -1;
                }

                if (!halfreached)
                {
                    partsPerLine++;
                    xOffset -= 16;
                }
                else
                {
                    xOffset += 16;
                    partsPerLine--;
                }

                savedOffsetX = xOffset;
            }
        }

        XOffsetBefore += width;
        if (height > biggestHeight) biggestHeight = height;
        if (XOffsetBefore > 4000)
        {
            XOffsetBefore = 0;
            YOffsetBefore += biggestHeight;
        }

        return newImageList;
    }

    /// GetColorList
    private static List<ushort> GetColorList(List<ushort> list, int width, int height, int OffsetX, int orgWidth)
    {
        List<ushort> colorList = [];
        bool allTransparent = true;
        for (int y = 0; y < height; y++)
        {
            List<ushort> dummy = [];
            for (int x = OffsetX; x < OffsetX + width; x++)
            {
                if (list[orgWidth * y + x] != 32767 || y > height - 8)
                {
                    dummy.Add(list[orgWidth * y + x]);
                    allTransparent = false;
                }
                else
                {
                    dummy.Add(32767);
                }
            }
            if (!allTransparent) colorList.AddRange(dummy);
        }

        return colorList;
    }

    /// GetDiamondWidth
    internal static int GetDiamondWidth(int anzParts)
    {
        int width = 0;
        int actualParts = 0;
        int corner = 1;
        while (true)
        {
            if (anzParts - actualParts - corner == 0)
            {
                width = corner - corner / 2;
                break;
            }
            else if (anzParts - actualParts - corner < 0)
            {
                //error
                break;
            }
            actualParts += corner;
            corner += 2;
        }

        return width;
    }
}
