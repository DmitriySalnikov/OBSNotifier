using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LocalizationGenerator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
    internal class Program
    {
        static int indent = 0;

        class LineIndent : IDisposable
        {
            public LineIndent() => indent++;
            public void Dispose() => indent--;
        }

        class BlockIndent : IDisposable
        {
            readonly StringBuilder? sb = null;
            readonly bool endWithSemicolon = false;

            public BlockIndent(StringBuilder sb, string? blockCondition = null, bool endWithSemicolon = false)
            {
                this.sb = sb;
                this.endWithSemicolon = endWithSemicolon;
                if (blockCondition != null)
                    Line(sb, blockCondition);
                Line(sb, "{");
                indent++;
            }

            public void Dispose()
            {
                indent--;
                if (sb != null)
                    Line(sb, "}" + (endWithSemicolon ? ";" : ""));
            }
        }

        static void Line(StringBuilder sb, string? line = null)
        {
            if (!string.IsNullOrEmpty(line))
            {
                sb.AppendLine(string.Join("", Enumerable.Repeat("    ", indent)) + line);
            }
            else
            {
                sb.AppendLine();
            }
        }

        static string SnakeToPascal(string snake)
        {
            if (string.IsNullOrWhiteSpace(snake))
                return "";
            return string.Join("", snake.Split('_').Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1)));
        }

        static string SnakeToCamel(string snake)
        {
            if (string.IsNullOrWhiteSpace(snake))
                return "";

            bool isFirst = true;
            return string.Join("", snake.Split('_').Select(s =>
            {
                if (isFirst)
                {
                    isFirst = false; return s;
                };
                return s.Substring(0, 1).ToUpper() + s.Substring(1);
            }));
        }

        static readonly Regex formattingRegex = new(@"\{([0-9])\}");
        static List<string> GetReplaceTemplates(string text)
        {
            return formattingRegex.Matches(text).Select(m => m.Value).Distinct().ToList();
        }

        static bool IsFormatable(string text)
        {
            return GetReplaceTemplates(text).Count > 0;
        }

        static bool ThrowIfNotMatchFormatting(string text1, string text2, string lang)
        {
            var res = GetReplaceTemplates(text1).Count == GetReplaceTemplates(text2).Count;
            if (res)
                return res;
            throw new Exception($"The number of formatting arguments for the \"{lang}\" language does not match the number of arguments for the default language ({DefaultLocale}).");
        }

        static readonly string BaseLocalizationFile = "Localization/strings.json";
        static readonly string LocalizationStatus = "Localization/localization_status.txt";
        static readonly string OutputFile = "OBSNotifierAva/Localization.cs";
        static readonly string DefaultLocale = "en";
        static readonly string DefaultNamespace = "OBSNotifier";

        static void Main()
        {
            JsonDocument json;
            using (var f = File.OpenText(Path.Combine(Environment.CurrentDirectory, BaseLocalizationFile)))
            {
                json = JsonDocument.Parse(f.ReadToEnd()) ?? throw new NullReferenceException();
            }

            var base_only_name = Path.GetFileNameWithoutExtension(BaseLocalizationFile);
            Dictionary<string, JsonDocument> localized_json = [];
            foreach (var l in Directory.EnumerateFiles("Localization"))
            {
                if (Path.GetExtension(l) != Path.GetExtension(BaseLocalizationFile))
                {
                    continue;
                }

                var file = Path.GetFileNameWithoutExtension(l);
                var parts = file.Split(".");
                if (parts.Length != 2 && parts[0] == base_only_name)
                {
                    if (file == base_only_name)
                    {
                        continue;
                    }
                    throw new Exception("Invalid file name. It must be in the format \"file_name.locale.json\"");
                }

                if (parts[0] != base_only_name)
                {
                    continue;
                }

                // Throw if locale is not valid
                CultureInfo.GetCultureInfo(parts[1]);

                using (var f = File.OpenText(Path.Combine(Environment.CurrentDirectory, l)))
                {
                    localized_json.Add(parts[1], JsonDocument.Parse(f.ReadToEnd()) ?? throw new NullReferenceException());
                }
            }

            StringBuilder sb = new();
            using (var _i1 = new BlockIndent(sb, $"namespace {DefaultNamespace}"))
            {
                using var _i2 = new BlockIndent(sb, $"namespace Tr");

                using (var _i3 = new BlockIndent(sb, "internal static class TrUtils"))
                {
                    using (var _i3_2 = new BlockIndent(sb, "static string GetLocale()"))
                    {
                        Line(sb, "return System.Globalization.CultureInfo.CurrentUICulture.Name;");
                    }
                    Line(sb);

                    // TODO detect language changes by avalonia in runtime
                    using (var _i4 = new BlockIndent(sb, "public static string GetTranslation(ref Dictionary<string, string> dict)"))
                    {
                        using (var _i5 = new BlockIndent(sb, "if (dict.TryGetValue(GetLocale(), out string? val))"))
                        {
                            Line(sb, "return val;");
                        }
                        Line(sb, $"return dict[\"{DefaultLocale}\"];");
                    }
                }
                Line(sb);

                using (var _i7 = new BlockIndent(sb, "public static class TrResources"))
                {
                    using (var _i4 = new BlockIndent(sb, "public static string LocalizationStatus"))
                    {

                        using (var f = File.OpenText(Path.Combine(Environment.CurrentDirectory, LocalizationStatus)))
                        {
                            Line(sb, $"get => @\"{f.ReadToEnd()}\";");
                        }
                    }
                }

                ProcessBlock(sb, "", json.RootElement, localized_json.Select(j => new { j.Key, Value = j.Value.RootElement }).ToDictionary(k => k.Key, v => v.Value));
            }

            File.WriteAllText(OutputFile, sb.ToString(), Encoding.UTF8);

            // TODO add progress
        }

        static void ProcessBlock(StringBuilder sb, string className, JsonElement je, Dictionary<string, JsonElement> lje)
        {
            List<JsonProperty> nextLevel = [];
            var _i1 = className != "" ? new BlockIndent(sb, $"public static class {className}") : null;

            foreach (var e in je.EnumerateObject())
            {
                if (e.Value.ValueKind == JsonValueKind.String)
                {

                    var locs = string.Join(", ",
                    lje.Prepend(new(DefaultLocale, je))
                    .Where(le => { if (le.Key == DefaultLocale) return true; return le.Value.TryGetProperty(e.Name, out JsonElement l) && l.GetRawText() != e.Value.GetRawText() && ThrowIfNotMatchFormatting(e.Value.GetRawText(), l.GetRawText(), le.Key); })
                    .Select(le => $"{{ \"{le.Key}\", {le.Value.GetProperty(e.Name).GetRawText()} }}")
                    );

                    string name = SnakeToPascal(e.Name); ;
                    string p_name = SnakeToCamel(e.Name);

                    Line(sb, "/// <summary>");
                    Line(sb, $"/// {e.Value.GetRawText()}");
                    Line(sb, "/// </summary>");
                    if (IsFormatable(e.Value.GetRawText()))
                    {
                        var rep = GetReplaceTemplates(e.Value.GetRawText());
                        var decl = string.Join(", ", Enumerable.Range(0, rep.Count).Select(r => $"string arg{r}"));
                        var args = string.Join(", ", Enumerable.Range(0, rep.Count).Select(r => $"arg{r}"));
                        Line(sb, $"public static string {name}({decl}) => string.Format(TrUtils.GetTranslation(ref {p_name}), {args});");
                    }
                    else
                    {
                        Line(sb, $"public static string {name} {{ get => TrUtils.GetTranslation(ref {p_name}); }}");
                    }
                    Line(sb, $"static Dictionary<string, string> {p_name} = new() {{ {locs} }};");
                    Line(sb);
                }
                else if (e.Value.ValueKind == JsonValueKind.Object)
                {
                    nextLevel.Add(e);
                }
            }

            _i1?.Dispose();
            Line(sb);

            foreach (var e in nextLevel)
            {
                ProcessBlock(sb, className + SnakeToPascal(e.Name), e.Value, lje.Where(j => j.Value.TryGetProperty(e.Name, out JsonElement val)).Select(j => new { j.Key, Value = j.Value.GetProperty(e.Name) }).ToDictionary(k => k.Key, v => v.Value));
            }
        }
    }
}
