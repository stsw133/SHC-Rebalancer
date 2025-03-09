using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace SHC_Rebalancer;
public class AiNameToImageSourceConverter : MarkupExtension, IValueConverter
{
    private static AiNameToImageSourceConverter? _instance;
    public static AiNameToImageSourceConverter Instance => _instance ??= new AiNameToImageSourceConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        if (Enum.TryParse<AI>(value.ToString(), out var ai) && ai == 0)
            return $"/Images/ai/{parameter}.png";

        var relativePath = $"Resources\\configs\\air\\{value}\\avatar_small.png";
        var absolutePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (Path.Exists(absolutePath))
            return new BitmapImage(new Uri(absolutePath, UriKind.Absolute));

        return $"/Images/ai/{value}.png";
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
