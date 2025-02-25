using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;

namespace SHC_Rebalancer;
public class PathToNameConverter : MarkupExtension, IValueConverter
{
    private static PathToNameConverter? _instance;
    public static PathToNameConverter Instance => _instance ??= new PathToNameConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        return Path.GetFileNameWithoutExtension(value.ToString());
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
