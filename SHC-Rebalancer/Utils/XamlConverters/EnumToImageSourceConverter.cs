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
            nameof(AI) => "/Images/ai/",
            nameof(AIForTroops) => "/Images/ai/",
            nameof(BlacksmithSetting) => "/Images/resources/",
            nameof(Building) => "/Images/buildings/",
            nameof(FletcherSetting) => "/Images/resources/",
            nameof(HarassingUnit) => "/Images/units/",
            nameof(LordType) => "/Images/lords/",
            nameof(PlayerColor) => "/Images/colors/",
            nameof(PoleturnerSetting) => "/Images/resources/",
            nameof(Resource) => "/Images/resources/",
            nameof(SkirmishDifficulty) => "/Images/skirmishdifficulties/",
            nameof(SkirmishMode) => "/Images/skirmishmodes/",
            nameof(SkirmishTeam) => "/Images/teams/",
            nameof(Troop) => "/Images/units/",
            nameof(Unit) => "/Images/units/",
            nameof(WallDecoration) => "/Images/walldecorations/",
            _ => throw new NotImplementedException()
        };
        var relativePath = $"{basePath}{value}.png";
        
        return new BitmapImage(new Uri(relativePath, UriKind.RelativeOrAbsolute));
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
