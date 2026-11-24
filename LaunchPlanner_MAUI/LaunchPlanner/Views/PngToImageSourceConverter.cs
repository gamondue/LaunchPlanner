using System.Globalization;

namespace LaunchPlanner.Views
{
    // Trasforma i byte PNG dell'icona di un evento in un'immagine per la XAML
    public sealed class PngToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is byte[] { Length: > 0 } png
                ? ImageSource.FromStream(() => new MemoryStream(png))
                : null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
