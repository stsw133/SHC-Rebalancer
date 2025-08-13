namespace SHC_Rebalancer;

/// <summary>
/// Utility functions for various operations.
/// </summary>
internal static class Fn
{
    /// <summary>
    /// Converts an integer to its ASCII character representation or a string to its ASCII integer value.
    /// </summary>
    /// <param name="value">The value to convert. Can be an <see cref="int"/> or a <see cref="string"/>.</param>"/>
    /// <returns>The ASCII character as a <see cref="string"/> if the input is an <see cref="int"/>, or the ASCII integer value as an <see cref="int"/> if the input is a <see cref="string"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is neither an <see cref="int"/> nor a <see cref="string"/>, or when the string is empty or null.</exception>
    public static object? AsciiConvert(object? value)
    {
        if (value is null)
            return null;

        if (value is int intValue)
        {
            //if (intValue == '\n' || intValue == '\r' || intValue == 133)
            //    return "⏎"; // temporary solution to avoid new line in DataGrid

            if (intValue < char.MinValue || intValue > char.MaxValue)
                return "�";

            return ((char)intValue).ToString();
        }

        if (value is string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                throw new ArgumentException("String cannot be null or empty.", nameof(value));

            return (int)strValue[0];
        }

        throw new ArgumentException("Not supported type. Expected int or string.", nameof(value));
    }
}
