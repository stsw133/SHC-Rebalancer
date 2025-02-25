using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SHC_Rebalancer;
public class DivisionConverter : MarkupExtension, IMultiValueConverter
{
    private static DivisionConverter? _instance;
    public static DivisionConverter Instance => _instance ??= new DivisionConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0;

        double numerator = 0, denominator = 1, multiplier = 1;

        try
        {
            numerator = ConvertToDoubleSafe(values[0]);
            denominator = ConvertToDoubleSafe(values[1]);
            if (parameter != null)
                multiplier = ConvertToDoubleSafe(parameter);

            return denominator != 0 ? Math.Ceiling(numerator / denominator * multiplier).ToString() : "-";
        }
        catch
        {
            return "0";
        }
    }

    /// ConvertBack
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

    /// ConvertToDoubleSafe
    private double ConvertToDoubleSafe(object value)
    {
        if (value is double d) return d;
        if (value is int i) return i;
        if (value is float f) return f;
        if (value is decimal m) return (double)m;
        if (value is long l) return l;
        if (value is short s) return s;
        if (value is byte b) return b;

        if (value is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            return parsed;

        return 0;
    }
}
