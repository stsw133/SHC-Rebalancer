using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SHC_Rebalancer;
public class EnumAttributeConverter : MarkupExtension, IValueConverter
{
    private static EnumAttributeConverter? _instance;
    public static EnumAttributeConverter Instance => _instance ??= new EnumAttributeConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return GetDefaultValue(targetType);

        var enumType = value.GetType();
        if (!enumType.IsEnum)
            throw new ArgumentException("Value must be an enum.");

        var attributeTypeName = parameter.ToString() ?? string.Empty;
        var field = enumType.GetField(value.ToString()!);
        if (field == null)
            return GetDefaultValue(targetType);

        var attributeType = field.DeclaringType?.Assembly.GetTypes().FirstOrDefault(t => (attributeTypeName + "Attribute").In(t.Name, t.FullName));
        if (attributeType == null)
            return GetDefaultValue(targetType);

        var hasAttribute = field.GetCustomAttribute(attributeType) != null;
        return targetType == typeof(Visibility) ? (hasAttribute ? Visibility.Visible : Visibility.Collapsed) : hasAttribute;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    /// GetDefaultValue
    private object? GetDefaultValue(Type targetType)
    {
        if (targetType == typeof(Visibility))
            return Visibility.Collapsed;
        if (targetType == typeof(bool))
            return false;
        return null;
    }
}
