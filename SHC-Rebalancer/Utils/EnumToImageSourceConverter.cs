using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace SHC_Rebalancer;
public class EnumToImageSourceConverter : MarkupExtension, IValueConverter
{
    private static EnumToImageSourceConverter? _instance;
    public static EnumToImageSourceConverter Instance => _instance ??= new EnumToImageSourceConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || !value.GetType().IsEnum)
            return null;

        var basePath = value.GetType().Name switch
        {
            nameof(Resource) => "/Resources/resources/",
            nameof(Building) => "/Resources/buildings/",
            nameof(Unit) => "/Resources/units/",
            _ => throw new NotImplementedException()
        };
        var relativePath = $"{basePath}{value}.png";
        
        return new BitmapImage(new Uri(relativePath, UriKind.RelativeOrAbsolute));
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
