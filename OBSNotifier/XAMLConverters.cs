using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OBSNotifier
{
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
        static string[] StringToTrueBool = new string[] { "true", "+", "yes", "y" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (parameter is string useAlpha && !StringToTrueBool.Contains(useAlpha.ToLower()))
                    return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                return new SolidColorBrush(color);
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                return !state;
            }
            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(HorizontalAlignment))
            {
                var hor = (HorizontalAlignment)value;
                switch (hor)
                {
                    case HorizontalAlignment.Left:
                        return HorizontalAlignment.Right;
                    case HorizontalAlignment.Right:
                        return HorizontalAlignment.Left;
                    default:
                        return hor;
                }
            }
            if (targetType == typeof(VerticalAlignment))
            {
                var ver = (VerticalAlignment)value;
                switch (ver)
                {
                    case VerticalAlignment.Top:
                        return VerticalAlignment.Bottom;
                    case VerticalAlignment.Bottom:
                        return VerticalAlignment.Top;
                    default:
                        return ver;
                }
            }

            throw new InvalidOperationException("The target must be a *Alignment");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
