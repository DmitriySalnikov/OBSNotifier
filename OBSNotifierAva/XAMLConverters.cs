using Avalonia.Data.Converters;

namespace OBSNotifier
{
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            throw new InvalidOperationException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        static readonly string[] StringToTrueBool = ["true", "+", "yes", "y"];

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (parameter is string useAlpha && !StringToTrueBool.Contains(useAlpha.ToLower()))
                    return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                return new SolidColorBrush(color);
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            throw new InvalidOperationException();
        }
    }

    public class BoolInvertedConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                return !state;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                return !state;
            }
            throw new InvalidOperationException();
        }
    }

    public class InverseAlignmentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType == typeof(HorizontalAlignment) && value != null)
            {
                var hor = (HorizontalAlignment)value;
                return hor switch
                {
                    HorizontalAlignment.Left => HorizontalAlignment.Right,
                    HorizontalAlignment.Right => HorizontalAlignment.Left,
                    _ => (object)hor,
                };
            }
            if (targetType == typeof(VerticalAlignment) && value != null)
            {
                var ver = (VerticalAlignment)value;
                return ver switch
                {
                    VerticalAlignment.Top => VerticalAlignment.Bottom,
                    VerticalAlignment.Bottom => VerticalAlignment.Top,
                    _ => (object)ver,
                };
            }

            throw new InvalidOperationException("The target must be a *Alignment");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
