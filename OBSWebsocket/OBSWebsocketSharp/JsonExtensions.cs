using System;
using System.Text.Json;

namespace OBSWebsocketSharp.Extensions
{
    public static class JsonExtensions
    {
        public static T EnumFromString<T>(string name) where T : new()
        {
            if (Enum.TryParse(typeof(T), name, out object? result))
                return (T)result;
            return (T)Enum.ToObject(typeof(T), long.MaxValue);
        }

        public static bool HasProperty(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement _))
            {
                return true;
            }
            return false;
        }

        public static string? ReadString(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                return e.GetString();
            }
            return null;
        }

        public static string[] ReadStringArray(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                List<string> a = [];
                foreach (var s in e.EnumerateArray())
                {
                    a.Add(s.GetString() ?? "");
                }
                return [.. a];
            }
            return [];
        }

        public static string? ReadRawString(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                return e.GetRawText().Replace("\"", "");
            }
            return null;
        }

        public static string RawString(this JsonElement element)
        {
            return element.GetRawText().Replace("\"", "");
        }

        public static bool? ReadBool(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                return e.GetBoolean();
            }
            return null;
        }

        public static string? ReadBoolString(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                return e.GetBoolean().ToString().ToLower();
            }
            return null;
        }

        public static double? ReadDouble(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                if (e.TryGetDouble(out double val))
                {
                    return val;
                }
            }
            return null;
        }

        public static long? ReadNumber(this JsonElement element, string prop)
        {
            if (element.TryGetProperty(prop, out JsonElement e))
            {
                if (e.TryGetInt64(out long val))
                {
                    return val;
                }
            }
            return null;
        }
    }
}
