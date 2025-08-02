using System.IO;
using SHC_Rebalancer.Services.GM1Service;

namespace SHC_Rebalancer;

/// GM1Service
public static class GM1Service
{
    /// ReplaceImageInGM1
    public static void ReplaceImageInGM1(string gm1FilePath, Dictionary<int, string> imageReplacements)
    {
        if (!File.Exists(gm1FilePath))
            throw new FileNotFoundException("GM1 file not found.", gm1FilePath);

        var gm1Bytes = File.ReadAllBytes(gm1FilePath);
        var decodedFile = new GM1DecodedFile();
        decodedFile.DecodeGm1File(gm1Bytes, Path.GetFileName(gm1FilePath));

        foreach (var replacement in imageReplacements)
        {
            var index = replacement.Key;
            var pngFilePath = replacement.Value;

            if (!File.Exists(pngFilePath))
                continue;

            if (index < 0 || index >= decodedFile.ImagesTGX.Count)
                continue;

            var tgxImage = decodedFile.ImagesTGX[index];

            int width = 0, height = 0;
            var colors = GM1Utils.LoadImage(pngFilePath, ref width, ref height, type: 1);

            tgxImage.Header.Width = (ushort)width;
            tgxImage.Header.Height = (ushort)height;

            tgxImage.Header.OffsetX = (ushort)Math.Max(0, (tgxImage.Width - width) / 2);
            tgxImage.Header.OffsetY = (ushort)Math.Max(0, (tgxImage.Height - height) / 2);

            tgxImage.EncodeWithoutPalette(colors, width, height);
        }

        uint offset = 0;
        for (int i = 0; i < decodedFile.ImagesTGX.Count; i++)
        {
            decodedFile.ImagesTGX[i].Offset = offset;
            decodedFile.ImagesTGX[i].ByteSize = (uint)decodedFile.ImagesTGX[i].ImageData.Length;
            offset += decodedFile.ImagesTGX[i].ByteSize;
        }

        decodedFile.Header.DataSize = offset;

        var newGm1Bytes = decodedFile.GetNewGM1Bytes();
        File.WriteAllBytes(gm1FilePath, newGm1Bytes);
    }

}
