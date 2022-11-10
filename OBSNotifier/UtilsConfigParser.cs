using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace OBSNotifier
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigIgnoreAttribute : Attribute { }

    public static partial class Utils
    {
        /// <summary>
        /// <para>
        /// Parse a simple format for storing config,
        /// for example in the <see cref="Plugins.OBSNotifierPluginSettings.AdditionalData"/>.
        /// Using reflection, this method will automatically set the values from the config string
        /// to the fields and properties of the passed class object.
        /// </para>
        /// <para>
        /// Example config:
        /// <code>
        /// Blocks = 3
        /// BackgroundColor = #112233
        /// Height = 64.5
        /// Margin = 0,4,0,4
        /// SomeText = Text with = in the middle
        /// </code>
        /// </para>
        /// <para>
        /// Any public field or public properties of a class with a getter and setter can be changed during parsing.
        /// You can use the <see cref="ConfigIgnoreAttribute"/> attribute to ignore fields and properties of the class.
        /// </para>
        /// <para>
        /// The data type after the `=` sign is determined based on the type of the field or property of the class with the same name.
        /// </para>
        /// <para>
        /// Supported types:
        /// <see cref="string"/>,
        /// <see cref="bool"/>,
        /// <see cref="int"/>,
        /// <see cref="uint"/>,
        /// <see cref="long"/>,
        /// <see cref="ulong"/>,
        /// <see cref="byte"/>,
        /// <see cref="sbyte"/>,
        /// <see cref="short"/>,
        /// <see cref="ushort"/>,
        /// <see cref="double"/>,
        /// <see cref="float"/>,
        /// <see cref="Thickness"/>,
        /// <see cref="Color"/>,
        /// <see cref="SolidColorBrush"/>
        /// </para>
        /// <para>
        /// Only single-line <see cref="string"/> are available in config string,
        /// but when parsing the sequence <c>"|n|"</c> will be replaced by <c>"\n"</c>.
        /// </para>
        /// <para>
        /// All settings are optional.
        /// </para>
        /// <seealso cref="ConfigSerializeObject{T}(T)"/>
        /// </summary>
        /// <typeparam name="T">Any class with fields and/or properties</typeparam>
        /// <param name="data">Config string</param>
        /// <param name="configToUpdate">Any object that will be updated during the parsing process</param>
        public static void ConfigParseString<T>(string data, ref T configToUpdate) where T : class
        {
            const string errorMods = "Invalid modifiers for the {0} \"{1}\". Must be public, non static, and be able to set and get the value.";
            var dataType = typeof(T);
            var lines = Regex.Replace(data, @"\r\n|\n\r|\r", "\n").Split('\n');
            foreach (var line in lines)
            {
                var args = line.Split(new char[] { '=' }, 2);
                if (args.Length == 2)
                {
                    var propName = args[0].Trim();
                    var propVal = args[1].Trim();

                    // Properties
                    {
                        var prop = dataType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
                        if (prop != null && prop.GetCustomAttribute<ConfigIgnoreAttribute>() == null)
                        {
                            prop.SetValue(configToUpdate, ParsePluginConfigValue(prop.PropertyType, propVal, prop.GetValue(configToUpdate)));
                            continue;
                        }
                        else if (dataType.GetProperty(propName) != null)
                        {
                            App.Log(string.Format(errorMods, new object[] { "property", propName }));
                            continue;
                        }
                    }

                    // Fields
                    {
                        var field = dataType.GetField(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField);
                        if (field != null && field.GetCustomAttribute<ConfigIgnoreAttribute>() == null)
                        {
                            field.SetValue(configToUpdate, ParsePluginConfigValue(field.FieldType, propVal, field.GetValue(configToUpdate)));
                            continue;
                        }
                        else if (dataType.GetField(propName) != null)
                        {
                            App.Log(string.Format(errorMods, new object[] { "field", propName }));
                            continue;
                        }
                    }
                }
            }
        }

        static object ParsePluginConfigValue(Type type, string data, object def)
        {
            if (type == typeof(string))
            {
                return data.Replace("|n|", "\n");
            }
            else if (type == typeof(bool))
            {
                var l = data.ToLower();
                return l == "yes" || l == "y" || l == "true" || l == "+";
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(data, out int val))
                    return val;
            }
            else if (type == typeof(uint))
            {
                if (uint.TryParse(data, out uint val))
                    return val;
            }
            else if (type == typeof(long))
            {
                if (long.TryParse(data, out long val))
                    return val;
            }
            else if (type == typeof(ulong))
            {
                if (ulong.TryParse(data, out ulong val))
                    return val;
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(data, out byte val))
                    return val;
            }
            else if (type == typeof(sbyte))
            {
                if (sbyte.TryParse(data, out sbyte val))
                    return val;
            }
            else if (type == typeof(short))
            {
                if (short.TryParse(data, out short val))
                    return val;
            }
            else if (type == typeof(ushort))
            {
                if (ushort.TryParse(data, out ushort val))
                    return val;
            }
            else if (type == typeof(double))
            {
                if (double.TryParse(data, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val))
                    return val;
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(data, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float val))
                    return val;
            }
            else if (type == typeof(Thickness))
            {
                var split = data.Split(',');
                if (split.Length == 1)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val))
                        return new Thickness(val);
                }
                else if (split.Length == 2)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val2))
                    {
                        return new Thickness(val1, val2, val1, val2);
                    }
                }
                else if (split.Length == 3)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val2) &&
                        double.TryParse(split[2].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val3))
                    {
                        return new Thickness(val1, val2, val3, val2);
                    }
                }
                else if (split.Length == 4)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val1) &&
                        double.TryParse(split[1].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val2) &&
                        double.TryParse(split[2].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val3) &&
                        double.TryParse(split[3].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double val4))
                    {
                        return new Thickness(val1, val2, val3, val4);
                    }
                }
            }
            else if (type == typeof(Color))
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(data);
                }
                catch { }
            }
            else if (type == typeof(SolidColorBrush))
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(data));
                }
                catch { }
            }

            return def;
        }

        /// <summary>
        /// <para>
        /// Serialize a class object into a simple config format, for example,
        /// can be used for <see cref="Plugins.OBSNotifierPluginSettings.AdditionalData"/>.
        /// Using reflection, this method will automatically get the values from the fields and properties
        /// of the passed class object and write them to the config string.
        /// </para>
        /// <para>
        /// Example class:
        /// <code>
        /// class ConfigData
        /// {
        ///     int NotVisible;
        ///     [ConfigIgnore]
        ///     public int IgnoreIt;
        ///     public int Count = 1;
        ///     public Color BGColor = Colors.Red;
        ///     public bool PropWillBeFirst { get; set; } = true;
        /// }
        /// </code>
        /// Produces config:
        /// <code>
        /// PropWillBeFirst = True
        /// Count = 1
        /// BGColor = #FFFF0000
        /// </code>
        /// </para>
        /// <para>
        /// Any public field or public properties of a class with a getter and a setter will be used during serialization.
        /// You can use the <see cref="ConfigIgnoreAttribute"/> attribute to ignore fields and properties of the class.
        /// </para>
        /// <para>
        /// Most types will be converted to strings using <see cref="Convert.ToString(object, IFormatProvider)"/>,
        /// but not all will be successfully read by the parser <see cref="ConfigParseString{T}(string, ref T)"/>
        /// </para>
        /// <para>
        /// For the <see cref="string"/> type, the newlines will be replaced with <c>"|n|"</c>.
        /// </para>
        /// <seealso cref="ConfigParseString{T}(string, ref T)"/>
        /// </summary>
        /// <typeparam name="T">Any class with fields and/or properties</typeparam>
        /// <param name="configData">Any object that will be updated during the parsing process</param>
        /// <returns>Config string</returns>
        public static string ConfigSerializeObject<T>(T configData) where T : class
        {
            var sb = new StringBuilder();
            var type = typeof(T);

            PropertyInfo[] props;
            FieldInfo[] fields;

            // write arrays of members to the cache to avoid random sorting as much as possible.
            if (cachedMembers.ContainsKey(type))
            {
                props = cachedMembers[type].Item1;
                fields = cachedMembers[type].Item2;
            }
            else
            {
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                    .Where((p) => p.GetCustomAttribute<ConfigIgnoreAttribute>() == null).ToArray();
                fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                    .Where((f) => f.GetCustomAttribute<ConfigIgnoreAttribute>() == null).ToArray();

                cachedMembers.Add(type, (props, fields));
            }

            var total = props.Length + fields.Length;
            uint shown = 0;

            foreach (var prop in props)
            {
                shown++;
                sb.Append(prop.Name);
                sb.Append(" = ");
                sb.Append(SerializePluginConfigValue(prop.PropertyType, prop.GetValue(configData)));
                if (shown < total)
                    sb.AppendLine();
            }
            foreach (var field in fields)
            {
                shown++;
                sb.Append(field.Name);
                sb.Append(" = ");
                sb.Append(SerializePluginConfigValue(field.FieldType, field.GetValue(configData)));
                if (shown < total)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
        static readonly Dictionary<Type, (PropertyInfo[], FieldInfo[])> cachedMembers = new Dictionary<Type, (PropertyInfo[], FieldInfo[])>();

        static string SerializePluginConfigValue(Type type, object value)
        {
            if (type == typeof(string))
            {
                return Regex.Replace((string)value, @"\r\n|\n\r|\r|\n", "|n|");
            }
            else if (type == typeof(double))
            {
                return ((double)value).ToString("0.0###", CultureInfo.InvariantCulture);
            }
            else if (type == typeof(float))
            {
                return ((float)value).ToString("0.0##", CultureInfo.InvariantCulture);
            }
            else if (type == typeof(Thickness))
            {
                var t = (Thickness)value;
                return FormattableString.Invariant($"{t.Left:0.####}, {t.Top:0.####}, {t.Right:0.####}, {t.Bottom:0.####}");
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// It's just a combination of the <see cref="ConfigParseString{T}(string, ref T)"/> and <see cref="ConfigSerializeObject{T}(T)"/>
        /// to fix errors and remove/add properties in the configuration string.
        /// </summary>
        /// <typeparam name="T">Class of cofig</typeparam>
        /// <param name="data">Config string</param>
        /// <returns>New fixed config string</returns>
        public static string ConfigFixString<T>(string data) where T : class, new()
        {
            var c = new T();
            ConfigParseString(data, ref c);
            return ConfigSerializeObject(c);
        }
    }
}
