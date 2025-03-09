using System.Text;
using System.IO;

namespace SHC_Rebalancer;

/// TexService
public static class TexService
{
    /// TexFilePath
    public static string TexFilePath => Path.Combine(SettingsService.Instance.Settings.GamePath, "cr.tex");

    /// ConvertTexToTxt
    public static void ConvertTexToTxt(string outputPath)
    {
        var fileData = File.ReadAllBytes(TexFilePath);

        var textBlock = new byte[fileData.Length - 1040];
        Array.Copy(fileData, 1040, textBlock, 0, textBlock.Length);

        var content = Encoding.Unicode.GetString(textBlock);
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\x00+", "\n");

        File.WriteAllText(outputPath, content, Encoding.UTF8);
    }

    /// ReplaceLinesInTex
    public static void ReplaceLinesInTex(Dictionary<int, string> replacements)
    {
        var fileData = File.ReadAllBytes(TexFilePath);
        var keys = ReadKeys(fileData);

        var textBlock = new byte[fileData.Length - 1040];
        Array.Copy(fileData, 1040, textBlock, 0, textBlock.Length);

        var content = Encoding.Unicode.GetString(textBlock);
        var lines = ParseLinesPreservingZeros(content);
        var cumulativeCharShift = 0;

        foreach (var lineIndex in replacements.Keys.OrderBy(i => i))
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                continue;

            var oldLine = lines[lineIndex];
            var newText = replacements[lineIndex];
            var oldLen = oldLine.Text.Length;
            var newLen = newText.Length;
            var diff = newLen - oldLen;

            if (diff == 0)
            {
                lines[lineIndex] = (newText, oldLine.Delimiter, oldLine.OriginalStartIndex);
                continue;
            }

            var actualCharOffset = oldLine.OriginalStartIndex + cumulativeCharShift;
            lines[lineIndex] = (newText, oldLine.Delimiter, oldLine.OriginalStartIndex);

            for (var i = 0; i < keys.Length; i++)
                if (keys[i] >= actualCharOffset)
                    keys[i] += diff;

            cumulativeCharShift += diff;
        }

        var finalContent = ReassembleContent(lines);
        var replacedTextBlock = Encoding.Unicode.GetBytes(finalContent);
        var updatedKeysBytes = new byte[1040];
        for (var i = 0; i < keys.Length; i++)
            Array.Copy(BitConverter.GetBytes(keys[i]), 0, updatedKeysBytes, i * 4, 4);

        using var fs = new FileStream(TexFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(updatedKeysBytes, 0, updatedKeysBytes.Length);
        fs.Write(replacedTextBlock, 0, replacedTextBlock.Length);
    }

    /// ReplaceTextInTex
    public static void ReplaceTextInTex(Dictionary<string, string> replacements)
    {
        var fileData = File.ReadAllBytes(TexFilePath);
        var keys = ReadKeys(fileData);

        var textBlock = new byte[fileData.Length - 1040];
        Array.Copy(fileData, 1040, textBlock, 0, textBlock.Length);

        var content = Encoding.Unicode.GetString(textBlock);
        foreach (var kvp in replacements)
            content = ReplaceWithOffsets(content, kvp.Key, kvp.Value, keys);

        var replacedTextBlock = Encoding.Unicode.GetBytes(content);
        var updatedKeysBytes = new byte[1040];
        for (int i = 0; i < keys.Length; i++)
            Array.Copy(BitConverter.GetBytes(keys[i]), 0, updatedKeysBytes, i * 4, 4);

        using var fs = new FileStream(TexFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(updatedKeysBytes, 0, updatedKeysBytes.Length);
        fs.Write(replacedTextBlock, 0, replacedTextBlock.Length);
    }

    /// GetHeaderAtIndex6
    public static string GetTranslationAtIndex(int index)
    {
        var fileData = File.ReadAllBytes(TexFilePath);
        var keys = ReadKeys(fileData);

        if (index < 0 || index >= keys.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        var startOffset = keys[index];
        var endOffset = (index + 1 < keys.Length) ? keys[index + 1] : (fileData.Length - 1040) / 2;

        var lengthInChars = endOffset - startOffset;
        if (lengthInChars <= 0)
            return string.Empty;

        var byteOffset = 1040 + startOffset * 2;
        var textBytes = new byte[lengthInChars * 2];
        Array.Copy(fileData, byteOffset, textBytes, 0, textBytes.Length);

        var content = Encoding.Unicode.GetString(textBytes);
        return content.TrimEnd('\x00');
    }

    /// ReplaceWithOffsets
    private static string ReplaceWithOffsets(string originalContent, string oldValue, string newValue, int[] keys)
    {
        if (string.IsNullOrEmpty(oldValue))
            return originalContent;

        var singleReplacementByteDiff = (newValue.Length - oldValue.Length);
        var sb = new StringBuilder();
        var cumulativeByteShift = 0;
        var searchStartIndex = 0;

        while (true)
        {
            int foundIndex = originalContent.IndexOf(oldValue, searchStartIndex, StringComparison.Ordinal);
            if (foundIndex == -1)
            {
                sb.Append(originalContent, searchStartIndex, originalContent.Length - searchStartIndex);
                break;
            }

            sb.Append(originalContent, searchStartIndex, foundIndex - searchStartIndex);
            sb.Append(newValue);

            var actualByteOffset = foundIndex + cumulativeByteShift;

            if (singleReplacementByteDiff != 0)
                for (var i = 0; i < keys.Length; i++)
                    if (keys[i] >= actualByteOffset)
                        keys[i] += singleReplacementByteDiff;

            cumulativeByteShift += singleReplacementByteDiff;
            searchStartIndex = foundIndex + oldValue.Length;
        }

        return sb.ToString();
    }

    /// ParseLinesPreservingZeros
    private static List<(string Text, string Delimiter, int OriginalStartIndex)> ParseLinesPreservingZeros(string content)
    {
        var result = new List<(string, string, int)>();

        var i = 0;
        var length = content.Length;

        while (i < length)
        {
            var lineStartIndex = i;
            while (i < length && content[i] != '\x00')
                i++;
            var lineText = content[lineStartIndex..i];

            var delimiterStartIndex = i;
            while (i < length && content[i] == '\x00')
                i++;
            var delimiter = content[delimiterStartIndex..i];

            result.Add((lineText, delimiter, lineStartIndex));
        }

        return result;
    }

    /// ReassembleContent
    private static string ReassembleContent(List<(string Text, string Delimiter, int OriginalStartIndex)> lines)
    {
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.Append(line.Text);
            sb.Append(line.Delimiter);
        }
        return sb.ToString();
    }

    /// ReadKeys
    private static int[] ReadKeys(byte[] fileData)
    {
        var keys = new int[260];
        for (var i = 0; i < 260; i++)
            keys[i] = BitConverter.ToInt32(fileData, i * 4);
        return keys;
    }
}
