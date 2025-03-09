namespace SHC_Rebalancer.Services.GM1Service;

/// GM1DecodedFile
class GM1DecodedFile : IDisposable
{
    private int _position = 0;

    internal GM1Header Header { get; private set; }
    internal GM1Palette? Palette { get; private set; }
    internal List<TGXImage> ImagesTGX { get => _imagesTGX; }
    private List<TGXImage> _imagesTGX;
    public byte[] FileArray { get => _fileArray; set => _fileArray = value; }
    private byte[] _fileArray;
    internal List<GM1TileImage> TilesImages { get => _tilesImage; set => _tilesImage = value; }
    private List<GM1TileImage> _tilesImage;

    /// DecodeGm1File
    public bool DecodeGm1File(byte[] array, string name)
    {
        _fileArray = array;
        if (Header == null)
        {
            var headerByteArray = new byte[GM1Header.ByteSize];
            Array.Copy(array, 0, headerByteArray, 0, GM1Header.ByteSize);
            Header = new GM1Header(headerByteArray)
            {
                Name = name
            };

            if (Header.DataType == GM1Header.GM1DataType.Animations)
            {
                var paletteByteArray = new byte[GM1Palette.ByteSize];
                Array.Copy(array, GM1Header.ByteSize, paletteByteArray, 0, GM1Palette.ByteSize);
                Palette = new GM1Palette(paletteByteArray);
            }
        }

        _position = (GM1Header.ByteSize + GM1Palette.ByteSize);

        _imagesTGX = [];
        _tilesImage = [];

        try
        {
            switch (Header.DataType)
            {
                case GM1Header.GM1DataType.Animations:
                case GM1Header.GM1DataType.Interface:
                case GM1Header.GM1DataType.TGXConstSize:
                case GM1Header.GM1DataType.Font:
                    CreateImages(array);
                    return true;
                case GM1Header.GM1DataType.NoCompression1:
                case GM1Header.GM1DataType.NoCompression2:
                    CreateNoCompressionImages(array);
                    return true;
                case GM1Header.GM1DataType.TilesObject:
                    CreateTileImage(array);
                    return true;
            }
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, "Error");
            return false;
        }

        return false;
    }

    /// GetNewGM1Bytes
    public byte[] GetNewGM1Bytes()
    {
        List<byte> newFile = [];
        var headerBytes = Header.ToByteArray();
        newFile.AddRange(headerBytes);

        if (Palette == null)
            newFile.AddRange(new byte[GM1Palette.ByteSize]);
        else
            newFile.AddRange(Palette.ToByteArray());

        for (int i = 0; i < Header.PictureCount; i++)
            newFile.AddRange(BitConverter.GetBytes(_imagesTGX[i].Offset));

        for (int i = 0; i < Header.PictureCount; i++)
            newFile.AddRange(BitConverter.GetBytes(_imagesTGX[i].ByteSize));

        for (int i = 0; i < Header.PictureCount; i++)
            newFile.AddRange(_imagesTGX[i].Header.ToByteArray());

        for (int i = 0; i < Header.PictureCount; i++)
            newFile.AddRange(_imagesTGX[i].ImageData);

        return [.. newFile];
    }

    /// CreateImages
    private void CreateImages(byte[] array)
    {
        CreateOffsetAndSizeInByteArrayList(array);
        CreateImgHeader(array);

        for (uint i = 0; i < Header.PictureCount; i++)
            _imagesTGX[(int)i].Bitmap = GM1Converters.DecodeGM1Image(
                _imagesTGX[(int)i].ImageData,
                _imagesTGX[(int)i].Header.Width,
                _imagesTGX[(int)i].Header.Height,
                Palette?.ColorTables[Palette.ActualPalette]);
    }

    /// CreateNoCompressionImages
    private void CreateNoCompressionImages(byte[] array)
    {
        CreateOffsetAndSizeInByteArrayList(array);
        CreateImgHeader(array);

        for (int i = 0; i < Header.PictureCount; i++)
            _imagesTGX[i].Bitmap = GM1Converters.ByteArrayToBitmap(
                _imagesTGX[i].ImageData,
                _imagesTGX[i].Header.Width,
                _imagesTGX[i].Header.Height);
    }

    private List<TGXImage> newTileList = [];

    /// ConvertImgToTiles
    internal void ConvertImgToTiles(List<ushort> list, ushort width, ushort height)
    {
        newTileList.AddRange(GM1Utils.ConvertImgToTiles(list, width, height, ImagesTGX));
    }

    /// CreateImgHeader
    private void CreateImgHeader(byte[] array)
    {
        for (int i = 0; i < Header.PictureCount; i++)
        {
            byte[] imageHeaderByteArray = new byte[TGXImageHeader.ByteSize];
            Array.Copy(array, _position + (i * TGXImageHeader.ByteSize), imageHeaderByteArray, 0, TGXImageHeader.ByteSize);
            _imagesTGX[i].Header = new TGXImageHeader(imageHeaderByteArray);
        }

        _position += (int)Header.PictureCount * TGXImageHeader.ByteSize;

        foreach (var image in _imagesTGX)
        {
            image.ImageData = new byte[(int)image.ByteSize];
            Buffer.BlockCopy(array, _position + (int)image.Offset, image.ImageData, 0, (int)image.ByteSize);
        }
    }

    /// CreateTileImage
    private void CreateTileImage(byte[] byteArray)
    {
        Dispose();
        CreateOffsetAndSizeInByteArrayList(byteArray);
        CreateImgHeader(byteArray);

        int offsetX = 0, offsetY = 0;
        int width = 0;
        int counter = -1;
        int itemsPerRow = 1;
        int actualItemsPerRow = 0;
        int safeoffset = 0;
        bool halfReached = false;
        int partsBefore = 0;

        for (int i = 0; i < _imagesTGX.Count; i++)
        {
            if (_imagesTGX[i].Header.ImagePart == 0)
            {
                width = GM1Utils.GetDiamondWidth(_imagesTGX[i].Header.SubParts);

                partsBefore += _imagesTGX[i].Header.SubParts;

                _tilesImage.Add(new GM1TileImage(width * 30 + ((width - 1) * 2), width * 16 + _imagesTGX[partsBefore - 1].Header.TileOffset + GM1TileImage.DefaultPadding)); // gap 2 pixels
                counter++;
                itemsPerRow = 1;
                actualItemsPerRow = 0;
                offsetY = _tilesImage[counter].Height - 16;
                offsetX = (width / 2) * 30 + (width - 1) - ((width % 2 == 0) ? 15 : 0);
                safeoffset = offsetX;
                halfReached = false;
            }

            if (_imagesTGX[i].ImageData.Length > 512)
            {
                int right = 0;
                if (_imagesTGX[i].Header.Direction == 3)
                {
                    right = 14;
                }

                _tilesImage[counter].AddTileOnTop(_imagesTGX[i].ImageData, offsetX + right, offsetY - _imagesTGX[i].Header.TileOffset);

                if (_tilesImage[counter].AdjustedHeight > offsetY - _imagesTGX[i].Header.TileOffset)
                {
                    _tilesImage[counter].AdjustedHeight = offsetY - _imagesTGX[i].Header.TileOffset;
                }
            }

            _tilesImage[counter].AddDiamondTile(_imagesTGX[i].ImageData, offsetX, offsetY);

            offsetX += 32;
            actualItemsPerRow++;
            if (actualItemsPerRow == itemsPerRow)
            {
                offsetX = safeoffset;

                offsetY -= 8;
                if (itemsPerRow < width)
                {
                    if (halfReached)
                    {
                        itemsPerRow--;
                        offsetX += 16;
                    }
                    else
                    {
                        itemsPerRow++;
                        offsetX -= 16;
                    }
                }
                else
                {
                    itemsPerRow--;
                    offsetX += 16;
                    halfReached = true;
                }

                safeoffset = offsetX;
                actualItemsPerRow = 0;
            }
        }

        foreach (var image in _tilesImage)
        {
            image.GenerateBitmap();
        }
    }

    /// CreateOffsetAndSizeInByteArrayList
    private void CreateOffsetAndSizeInByteArrayList(byte[] byteArray)
    {
        for (int i = 0; i < Header.PictureCount; i++)
        {
            var image = new TGXImage
            {
                Offset = BitConverter.ToUInt32(byteArray, _position + i * 4)
            };
            _imagesTGX.Add(image);
        }

        _position += (int)Header.PictureCount * 4;

        for (int i = 0; i < Header.PictureCount; i++)
            _imagesTGX[i].ByteSize = BitConverter.ToUInt32(byteArray, _position + i * 4);

        _position += (int)Header.PictureCount * 4;
    }

    /// SetNewTileList
    public void SetNewTileList()
    {
        if (_imagesTGX.Count <= newTileList.Count)
            for (int i = 0; i < _imagesTGX.Count; i++)
                newTileList[i].Header.AnimatedColor = _imagesTGX[i].Header.AnimatedColor;

        _imagesTGX = newTileList;
        Header.PictureCount = (uint)_imagesTGX.Count;
        newTileList = [];
    }

    /// Dispose
    public void Dispose()
    {
        if (_tilesImage != null)
            foreach (GM1TileImage image in _tilesImage)
                image.Dispose();
    }
}
