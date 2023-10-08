using OBSWebsocketSharp.Extensions;
using System.Text;
using System.Text.Json;

namespace ProtocolGenerator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
    internal class Generator
    {
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

        static readonly string RootDir = "Generated";
        static readonly string DefaultNS = "OBSWebsocketSharp";
        static int indent = 0;

        static void Main(string[] args)
        {
            Directory.CreateDirectory(RootDir);
            JsonDocument json = JsonDocument.Parse(ResFiles.protocol) ?? throw new NullReferenceException();
            var protocol = json.RootElement;

            GenerateEnums(protocol.GetProperty("enums"));
            GenerateEvents(protocol.GetProperty("events"));
            GenerateRequest(protocol.GetProperty("requests"));
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

        static void Summary(StringBuilder sb, string? comment = null, string? remarks = null, IEnumerable<(string, string)>? paramsStrs = null, string? returnStr = null)
        {
            void print_docs_comments(string start_tag, string text, string end_tag)
            {
                if (text.Contains('\n'))
                {
                    Line(sb, "/// " + start_tag);
                    foreach (var line in text.Split('\n'))
                    {
                        Line(sb, $"/// {line}");
                    }
                    Line(sb, "/// " + end_tag);
                }
                else
                {
                    Line(sb, $"/// {start_tag}{text}{end_tag}");
                }
            }

            if (!string.IsNullOrEmpty(comment))
            {
                print_docs_comments("<summary>", comment, "</summary>");
            }

            if (paramsStrs != null)
            {
                foreach (var param in paramsStrs)
                {
                    print_docs_comments($"<param name=\"{param.Item1}\">", param.Item2, "</param>");
                }
            }

            if (!string.IsNullOrEmpty(returnStr))
            {
                print_docs_comments($"<returns>", returnStr, "</returns>");
            }

            if (!string.IsNullOrEmpty(remarks))
            {
                print_docs_comments($"<remarks>", remarks, "</remarks>");
            }
        }

        static string JsonTypeToCS(string type, bool isInput = false)
        {
            switch (type)
            {
                case "String":
                    return "string";
                case "Number":
                    return "double";
                case "Any":
                case "Object":
                    if (isInput)
                    {
                        return "JsonObject";
                    }
                    else
                    {
                        return "JsonElement";
                    }
                case "Boolean":
                    return "bool";
            }

            const string arrPrefix = "Array<";
            if (type.StartsWith(arrPrefix))
            {
                var innerType = JsonTypeToCS(type[arrPrefix.Length..^1], isInput);

                // TODO test with array type
                if (innerType == "JsonElement" || innerType == "JsonObject")
                    return innerType;
                else
                    return $"{innerType}[]";
            }

            return $".{type}.";
        }

        static string GetJsonConvertMethodFromCSType(string type)
        {
            switch (type)
            {
                case "string":
                    return "ReadString";
                case "string[]":
                    return "ReadStringArray";
                case "double":
                    return "ReadDouble";
                case "bool":
                    return "ReadBool";
                case "JsonObject":
                case "JsonElement":
                    return "";
            }

            return $".{type}.";
        }

        static Dictionary<string, string> EnumStringMap = new(){
           {"outputState", "ObsOutputState"},
           {"mediaAction", "ObsMediaInputAction"},
        };

        static bool IsEnumString(string name, string type)
        {
            if (type.ToLower() != "string")
                return false;

            return EnumStringMap.ContainsKey(name);
        }

        static string GetValueString(string name, string type)
        {
            if (IsEnumString(name, type))
            {
                return $"Enum.GetName(typeof({EnumStringMap[name]}), {name})";
            }

            return name;
        }

        static string GetValueType(string name, string type)
        {
            if (IsEnumString(name, type))
            {
                return EnumStringMap[name];
            }

            return type;
        }

        static void GenerateEnums(JsonElement enums)
        {
            StringBuilder sb = new();
            using (var _i1 = new BlockIndent(sb, $"namespace {DefaultNS}"))
            {
                foreach (var enum_obj in enums.EnumerateArray())
                {
                    using (var _i2 = new BlockIndent(sb, $"public enum {enum_obj.ReadString("enumType")}"))
                    {
                        JsonElement constants = enum_obj.GetProperty("enumIdentifiers");
                        foreach (var enumConstant in constants.EnumerateArray())
                        {
                            JsonElement enumId = enumConstant.GetProperty("enumIdentifier");
                            string constName = enumId.GetString() ?? throw new NullReferenceException("enumIdentifier");
                            JsonElement value = enumConstant.GetProperty("enumValue");
                            string initVersion = enumConstant.ReadString("initialVersion") ?? "0.0.0";

                            Summary(sb, comment: enumConstant.ReadString("description"), remarks: $"Since version {initVersion}");

                            Line(sb, $"[EnumElementMetadata({int.Parse(enumConstant.ReadRawString("rpcVersion") ?? "0")}, {enumConstant.ReadBoolString("deprecated")}, \"{initVersion}\", \"{enumConstant.ReadRawString("enumValue")}\", {(value.RawString() == constName).ToString().ToLower()})]");

                            if (value.ValueKind == JsonValueKind.Number)
                            {
                                Line(sb, $"{constName} = {value.GetInt64()},");
                            }
                            else if (value.ValueKind == JsonValueKind.String)
                            {
                                if (long.TryParse(value.GetString(), out long num))
                                {
                                    Line(sb, $"{constName} = {num},");
                                }
                                else if (value.GetString() == constName)
                                {
                                    Line(sb, $"{constName},");
                                }
                                else
                                {
                                    Line(sb, $"{constName} = {value.GetString()},");
                                }
                            }
                            Line(sb);
                        }
                    }
                    Line(sb);
                }
            }

            File.WriteAllText(Path.Combine(RootDir, "Enums.cs"), sb.ToString());
        }

        struct eventData
        {
            public string name;
            public string desc;
            public string remarks;
            public string meta;
            public bool hasDataClass;
        }

        static void GenerateEvents(JsonElement events)
        {
            StringBuilder sb = new();

            Line(sb, "using System.Text.Json;");
            Line(sb, "using System.Text.Json.Serialization;");
            Line(sb, "using OBSWebsocketSharp.Extensions;");
            Line(sb);
            using (var _i1 = new BlockIndent(sb, $"namespace {DefaultNS}"))
            {
                List<eventData> eventsData = [];

                foreach (var event_obj in events.EnumerateArray())
                {
                    string initVersion = event_obj.ReadString("initialVersion") ?? "0.0.0";
                    string eventName = event_obj.ReadString("eventType") ?? throw new NullReferenceException("eventType");
                    JsonElement dataFields = event_obj.GetProperty("dataFields");
                    var hasDataClass = dataFields.GetArrayLength() > 0;

                    eventsData.Add(new eventData()
                    {
                        name = eventName,
                        desc = event_obj.ReadString("description") ?? "",
                        remarks = $"Since version {initVersion}",
                        meta = $"[EventDataMetadata(EventSubscription.{event_obj.ReadString("eventSubscription")}, {event_obj.ReadNumber("complexity")}, {int.Parse(event_obj.ReadRawString("rpcVersion") ?? "0")}, {event_obj.ReadBoolString("deprecated")}, \"{initVersion}\", \"{event_obj.ReadString("category")}\")]",
                        hasDataClass = hasDataClass,
                    });

                    if (hasDataClass)
                    {
                        List<(string, string, string)> fields = [];
                        foreach (var dataField in dataFields.EnumerateArray())
                        {
                            fields.Add((dataField.ReadString("valueName") ?? ".NO_NAME.", dataField.ReadString("valueType") ?? ".NO_TYPE.", dataField.ReadString("valueDescription") ?? ""));
                        }

                        using (var _i3 = new BlockIndent(sb, $"public class {eventName}Data"))
                        {
                            foreach (var field in fields)
                            {
                                Summary(sb, comment: field.Item3);

                                Line(sb, $"public {JsonTypeToCS(field.Item2)} {field.Item1} = default!;");

                                if (IsEnumString(field.Item1, field.Item2))
                                {
                                    Summary(sb, comment: field.Item3 + $"\n\nSame as <see cref=\"{field.Item1}\"/>, but converted to enum.");
                                    var enumType = EnumStringMap[field.Item1];
                                    Line(sb, "[JsonIgnore]");
                                    Line(sb, $"public {enumType} {field.Item1}Enum {{ get => JsonExtensions.EnumFromString<{enumType}>({field.Item1}); }}");
                                }
                            }
                        }
                    }

                    Line(sb);
                }

                using (var _i4 = new BlockIndent(sb, "public class OBSEvents"))
                {
                    foreach (var data in eventsData)
                    {
                        Summary(sb, comment: data.desc, remarks: data.remarks);
                        Line(sb, data.meta);

                        if (data.hasDataClass)
                            Line(sb, $"public event EventHandler<{data.name}Data>? {data.name};");
                        else
                            Line(sb, $"public event EventHandler? {data.name};");
                        Line(sb);
                    }

                    using (var _i6 = new BlockIndent(sb, "internal void ProcessEventData(JsonElement json)"))
                    {
                        Line(sb, "string eventType = json.ReadString(\"eventType\") ?? \"\";");
                        Line(sb, "JsonElement data = default;");
                        using (var _i6_2 = new BlockIndent(sb, "if (json.TryGetProperty(\"eventData\", out var val))"))
                        {
                            Line(sb, "data = val;");
                        }
                        Line(sb, "string failedToDeserialize = \"Failed to deserialize data for the {0} event.\";");
                        using (var _i6_1 = new BlockIndent(sb, "JsonSerializerOptions serializerOptions = new()", endWithSemicolon: true))
                        {
                            Line(sb, "IncludeFields = true,");
                        }
                        Line(sb);

                        using (var _i7 = new BlockIndent(sb, "switch (eventType)"))
                        {
                            foreach (var data in eventsData)
                            {
                                Line(sb, $"case nameof({data.name}):");
                                using (var _i8 = new LineIndent())
                                using (var _i9 = new BlockIndent(sb))
                                {
                                    if (data.hasDataClass)
                                        Line(sb, $"{data.name}?.Invoke(this, JsonSerializer.Deserialize<{data.name}Data>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof({data.name}))));");
                                    else
                                        Line(sb, $"{data.name}?.Invoke(this, EventArgs.Empty);");
                                    Line(sb, "break;");
                                }
                            }

                            using (var _i10 = new BlockIndent(sb, "default:"))
                            {
                                Line(sb, "Console.WriteLine($\"Unsupported Event: {eventType}\\n{data.GetRawText()}\");");
                                Line(sb, "break;");
                            }
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(RootDir, "Events.cs"), sb.ToString());
        }

        struct requestArgs
        {
            public string name;
            public string type;
            public string desc;
            public string? restrictions;
            public bool optional;
            public string? optionalHint;
        }

        struct responseData
        {
            public string name;
            public string type;
            public string desc;
        }

        /// <summary>
        /// <see cref=""/>
        /// </summary>
        /// <param name="requests"></param>
        /// <exception cref="NullReferenceException"></exception>
        static void GenerateRequest(JsonElement requests)
        {
            StringBuilder sb = new();

            Line(sb, "using System.Text.Json;");
            Line(sb, "using System.Text.Json.Nodes;");
            Line(sb, "using System.Text.Json.Serialization;");
            Line(sb, "using OBSWebsocketSharp.Extensions;");
            Line(sb);
            using (var _i1 = new BlockIndent(sb, $"namespace {DefaultNS}"))
            {
                using (var _i2 = new BlockIndent(sb, "public partial class OBSRequests"))
                {
                    foreach (var request_obj in requests.EnumerateArray())
                    {
                        string initVersion = request_obj.ReadString("initialVersion") ?? "0.0.0";
                        string requestName = request_obj.ReadString("requestType") ?? throw new NullReferenceException("requestType");

                        var desc = request_obj.ReadString("description") ?? "";
                        var meta = $"[RequestMetadata({request_obj.ReadNumber("complexity")}, {int.Parse(request_obj.ReadRawString("rpcVersion") ?? "0")}, {request_obj.ReadBoolString("deprecated")}, \"{initVersion}\", \"{request_obj.ReadString("category")}\")]";

                        JsonElement requestArgs = request_obj.GetProperty("requestFields");
                        JsonElement responeObjs = request_obj.GetProperty("responseFields");

                        List<requestArgs> parsedArgs = [];
                        List<responseData> parsedResp = [];

                        foreach (var arg in requestArgs.EnumerateArray())
                        {
                            parsedArgs.Add(new()
                            {
                                name = arg.ReadString("valueName") ?? throw new NullReferenceException("valueName"),
                                type = JsonTypeToCS(arg.ReadString("valueType") ?? throw new NullReferenceException("valueType"), true),
                                desc = arg.ReadString("valueDescription") ?? "",
                                restrictions = arg.ReadString("valueRestrictions"),
                                optional = arg.ReadBool("valueOptional") ?? throw new NullReferenceException("valueOptional"),
                                optionalHint = arg.ReadString("valueOptionalBehavior")
                            });
                        }

                        foreach (var res in responeObjs.EnumerateArray())
                        {
                            parsedResp.Add(new()
                            {
                                name = res.ReadString("valueName") ?? throw new NullReferenceException("valueName"),
                                type = JsonTypeToCS(res.ReadString("valueType") ?? throw new NullReferenceException("valueType")),
                                desc = res.ReadString("valueDescription") ?? "",
                            });
                        }

                        parsedArgs.Sort((a, b) => a.optional.CompareTo(b.optional));
                        var parsedArgsFixed = parsedArgs.Where(a => !a.name.Contains('.') && parsedArgs.FindIndex(p => p.name == a.name) != -1).ToArray();

                        bool isHasReturn = parsedResp.Count > 0;
                        string customReturn = parsedResp.Count > 1 ? $"{requestName}Return" : "";
                        string retType = isHasReturn ? (parsedResp.Count > 1 ? customReturn : parsedResp[0].type) : "void";
                        string callArgs = $"{string.Join(", ", parsedArgsFixed
                            .Select(a =>
                            {
                                if (a.optional)
                                    return $"{GetValueType(a.name, a.type)}? {a.name} = null";
                                else
                                    return $"{GetValueType(a.name, a.type)} {a.name}";
                            }))}";

                        if (!string.IsNullOrWhiteSpace(customReturn))
                        {
                            using (var _i2_1 = new BlockIndent(sb, $"public class {customReturn}"))
                            {
                                foreach (var res in parsedResp)
                                {
                                    Summary(sb, comment: res.desc);
                                    Line(sb, $"public {res.type} {res.name} = default!;");
                                }
                            }
                            Line(sb);
                        }

                        Summary(sb,
                            comment: desc,
                            // Use the full list (parsedArgs) here to help the user
                            paramsStrs: parsedArgs.Select(a => (a.name, $"{a.desc}{(a.optional && a.optionalHint != null ? $"\n<code>If omitted: {a.optionalHint.Replace("<", "&lt;").Replace(">", "&gt;")}</code>" : "")}{(a.restrictions != null ? $"\n<code>Restrictions: {a.restrictions.Replace("<", "&lt;").Replace(">", "&gt;")}</code>" : "")}")),
                            returnStr: string.IsNullOrWhiteSpace(customReturn) ? (isHasReturn ? parsedResp[0].desc : "") : $"<see cref=\"{customReturn}\"/>"
                            );

                        using (var _i3 = new BlockIndent(sb, $"public async {(retType == "void" ? "Task" : $"Task<{retType}>")} {requestName}({callArgs})"))
                        {
                            // Restrictions
                            foreach (var a in parsedArgsFixed)
                            {
                                if (!string.IsNullOrWhiteSpace(a.restrictions))
                                {
                                    using (var _i4 = new BlockIndent(sb, $"if ({string.Join(" || ", a.restrictions.Split(',').Select(r => $"!({a.name} {r.Trim()})"))})"))
                                    {
                                        Line(sb, $"throw new ArgumentOutOfRangeException(nameof({a.name}), $\"{{nameof({a.name})}} outside of \\\"{a.restrictions}\\\"\");");
                                    }
                                }
                            }

                            // Required
                            if (parsedArgsFixed.Where(a => !a.optional).Any())
                            {
                                using (var _i4 = new BlockIndent(sb, "var data = new JsonObject()", true))
                                {
                                    foreach (var arg in parsedArgsFixed)
                                    {
                                        if (!arg.optional)
                                        {
                                            Line(sb, $"{{nameof({arg.name}), {GetValueString(arg.name, arg.type)}}},");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Line(sb, "var data = new JsonObject();");
                            }

                            // Optional
                            if (parsedArgsFixed.Where(a => a.optional).Any())
                            {
                                foreach (var arg in parsedArgsFixed)
                                {
                                    if (arg.optional)
                                    {
                                        using (var _i5 = new BlockIndent(sb, $"if ({arg.name} != null)"))
                                        {
                                            Line(sb, $"data.Add(nameof({arg.name}), {GetValueString(arg.name, arg.type)});");
                                        }
                                    }
                                }
                            }

                            // Return
                            // TODO fix getting root of jsonDoc
                            if (isHasReturn)
                            {
                                Line(sb, "var response = await Request(data);");
                                if (string.IsNullOrWhiteSpace(customReturn))
                                {
                                    var converter = GetJsonConvertMethodFromCSType(parsedResp[0].type);
                                    if (string.IsNullOrWhiteSpace(converter))
                                    {
                                        Line(sb, $"return response;");
                                    }
                                    else
                                    {
                                        Line(sb, $"return response.{converter}(\"{parsedResp[0].name}\") ?? throw new NullReferenceException(\"{parsedResp[0].name}\");");
                                    }
                                }
                                else
                                {
                                    Line(sb, $"return JsonSerializer.Deserialize<{requestName}Return>(response, serializerOptions) ?? throw new NullReferenceException(nameof({requestName}Return));");
                                }
                            }
                            else
                            {
                                Line(sb, "await Request(data);");
                            }
                        }

                        Line(sb);
                    }
                }
            }

            File.WriteAllText(Path.Combine(RootDir, "Requests.cs"), sb.ToString());
        }
    }
}
