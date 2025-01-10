using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSNotifier
{
    partial class Settings
    {
        class FloatJsonConverter : JsonConverter<float>
        {
            public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return (float)reader.GetDouble();
            }

            public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
            {
                writer.WriteRawValue(FormattableString.Invariant($"{value:0.###}"));
            }
        }

        class DoubleJsonConverter : JsonConverter<double>
        {
            public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetDouble();
            }

            public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
            {
                writer.WriteRawValue(FormattableString.Invariant($"{value:0.####}"));
            }
        }

        class CultureInfoJsonConverter : JsonConverter<CultureInfo>
        {
            public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var cultureName = reader.GetString();
                return CultureInfo.GetCultureInfo(cultureName ?? "en");
            }

            public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Name);
            }
        }

        class RectJsonConverter : JsonConverter<Rect>
        {
            public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 4)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val2) &&
                        double.TryParse(split[2].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val3) &&
                        double.TryParse(split[3].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val4))
                    {
                        return new Rect(val1, val2, val3, val4);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.X:0.####}, {value.Y:0.####}, {value.Width:0.####}, {value.Height:0.####}"));
            }
        }

        class PixelRectJsonConverter : JsonConverter<PixelRect>
        {
            public override PixelRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 4)
                {
                    if (int.TryParse(split[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val1) &&
                        int.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val2) &&
                        int.TryParse(split[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val3) &&
                        int.TryParse(split[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val4))
                    {
                        return new PixelRect(val1, val2, val3, val4);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, PixelRect value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.X}, {value.Y}, {value.Width}, {value.Height}"));
            }
        }

        class ThicknessJsonConverter : JsonConverter<Thickness>
        {
            public override Thickness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 4)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val2) &&
                        double.TryParse(split[2].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val3) &&
                        double.TryParse(split[3].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val4))
                    {
                        return new Thickness(val1, val2, val3, val4);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, Thickness value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.Left:0.####}, {value.Top:0.####}, {value.Right:0.####}, {value.Bottom:0.####}"));
            }
        }

        class PointJsonConverter : JsonConverter<Point>
        {
            public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 2)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val2))
                    {
                        return new Point(val1, val2);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.X:0.####}, {value.Y:0.####}"));
            }
        }

        class PixelPointJsonConverter : JsonConverter<PixelPoint>
        {
            public override PixelPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 2)
                {
                    if (int.TryParse(split[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val1) &&
                        int.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val2))
                    {
                        return new PixelPoint(val1, val2);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, PixelPoint value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.X}, {value.Y}"));
            }
        }

        class SizeJsonConverter : JsonConverter<Size>
        {
            public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 2)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val2))
                    {
                        return new Size(val1, val2);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.Width:0.####}, {value.Height:0.####}"));
            }
        }

        class PixelSizeJsonConverter : JsonConverter<PixelSize>
        {
            public override PixelSize Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var split = reader.GetString()?.Split(',');
                if (split != null && split.Length == 2)
                {
                    if (int.TryParse(split[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val1) &&
                        int.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val2))
                    {
                        return new PixelSize(val1, val2);
                    }
                }
                return new();
            }

            public override void Write(Utf8JsonWriter writer, PixelSize value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(FormattableString.Invariant($"{value.Width}, {value.Height}"));
            }
        }

        class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return Color.Parse(reader.GetString() ?? "#000");
                }
                catch
                {
                    return Colors.Black;
                }
            }

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
