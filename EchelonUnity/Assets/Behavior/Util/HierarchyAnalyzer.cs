using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal abstract class JsonNode
{
    public void SaveTo(string filename)
    {
        using (var fs = new FileStream(filename, FileMode.Create))
        {
            using (var w = new StreamWriter(fs))
            {
                WriteTo(default, w);
            }
        }
    }

    internal abstract void WriteTo(Indent indent, StreamWriter w);
}

internal class JsonValue : JsonNode
{
    public string Value { get; }

    public static JsonValue Null { get; } = new JsonValue(null, new OverflowGuard());

    public JsonValue(string value, OverflowGuard guard)
    {
        guard.SignalNewValue();
        Value = EscapeString(value);
    }

    public static string EscapeString(string value)
         => value == null
            ? "null"
            : '"' + value.Replace("\\", "\\\\").Replace("\"", "\\\\") + '"';

    public JsonValue(object value, OverflowGuard guard)
    {
        guard.SignalNewValue();
        switch (value)
        {
            case float f:
                if (f == float.PositiveInfinity)
                    f = float.MaxValue;
                else if (f == float.NegativeInfinity)
                    f = float.MinValue;
                Value = f.ToString(CultureInfo.InvariantCulture);
                break;
            case double f:
                if (f == double.PositiveInfinity)
                    f = double.MaxValue;
                else if (f == double.NegativeInfinity)
                    f = double.MinValue;
                Value = f.ToString(CultureInfo.InvariantCulture);
                break;
            case bool b:
                Value = b.ToString(CultureInfo.InvariantCulture).ToLower();
                break;
            case int i:
                Value = i.ToString(CultureInfo.InvariantCulture);
                break;
            case long l:
                Value = l.ToString(CultureInfo.InvariantCulture);
                break;
            case short s:
                Value = s.ToString(CultureInfo.InvariantCulture);
                break;
            case null:
                Value = "null";
                break;
            default:
                Value = EscapeString(value.ToString());
                break;
        }
    }

    internal override void WriteTo(Indent indent, StreamWriter w)
    {
        //w.Write(indent);
        w.Write(Value);
    }
}

internal class SoftNull : JsonValue
{
    public Type Type { get; }

    public SoftNull(Type type, OverflowGuard guard):base($"SoftNull<{type.Name}>", guard)
    {
        Type = type;
    }
}

internal class JsonValue<T> : JsonValue
{
    public JsonValue(T value, OverflowGuard guard) : base(value, guard)
    { }
}

internal class JsonReference : JsonValue
{
    public JsonReference(Component t, OverflowGuard guard) : base(C2S(t), guard)
    { }

    public JsonReference(UnityEngine.Object o, OverflowGuard guard) : base(O2S(o), guard)
    { }

    internal static string O2S(UnityEngine.Object o)
    {
        return $"{o.GetType()} '{o.name}' id={o.GetInstanceID()}";

    }

    internal static string C2S(Component c)
    {
        return $"{c.GetType().Name} '{c.name}' ids=[{c.GetInstanceID()},{c.gameObject.GetInstanceID()}]";
    }

}

internal class JsonArray : JsonNode
{
    public bool IsEmpty => Data.Count == 0;
    private List<JsonNode> Data { get; } = new List<JsonNode>();
    public OverflowGuard Guard { get; }

    public JsonArray(string name, OverflowGuard guard)
    {
        Guard = guard;
        guard.SignalNewArray(name);
    }

    public void Add(JsonNode node)
    {
        Guard.SignalNewArrayElement();
        Data.Add(node);
    }

    public void AddAsValues<T>(IEnumerable<T> values)
    {
        foreach (var item in values)
            Add(new JsonValue(item, Guard));
    }

    internal override void WriteTo(Indent indent, StreamWriter w)
    {
        w.WriteLine("[");
        var inner = indent.Inc();
        for (int i = 0; i < Data.Count; i++)
        {
            w.Write(inner);
            Data[i].WriteTo(inner, w);
            if (i + 1 < Data.Count)
                w.WriteLine(',');
            else
                w.WriteLine();
        }
        w.Write(indent); w.Write("]");

    }
}

internal class JsonObject : JsonNode
{
    public JsonObject(string name, OverflowGuard guard)
    {
        Guard = guard;
        guard.SignalNewObject(name);
    }


    private List<KeyValuePair<string, JsonNode>> Properties { get; } = new List<KeyValuePair<string, JsonNode>>();
    public int MaxRecursionDepth { get; }
    public OverflowGuard Guard { get; }

    public void Add(string key, JsonNode value)
    {
        Guard.SignalNewProperty(key);
        Properties.Add(new KeyValuePair<string, JsonNode>(key, value));
    }

    public void AddValue(string key, object value)
        => Add(key, new JsonValue(value, Guard));

    public JsonObject AddObject(string key)
    {
        Guard.SignalNewProperty(key);
        JsonObject child = new JsonObject(key, Guard);
        Add(key, child);
        return child;
    }
    public JsonArray AddArray(string key)
    {
        Guard.SignalNewProperty(key);
        JsonArray child = new JsonArray(key, Guard);
        Add(key, child);
        return child;
    }

    internal void SetComponentProperties(Component c)
    {
        SetObjectProperties(c);
        Add("GameObject.InstanceId", new JsonValue(c.gameObject.GetInstanceID(), Guard));
    }
    internal void SetObjectProperties(UnityEngine.Object o)
    {
        Add("Class", new JsonValue(o.GetType().Name, Guard));
        Add("Name", new JsonValue(o.name, Guard));
        Add("InstanceId", new JsonValue(o.GetInstanceID(), Guard));
    }

    internal override void WriteTo(Indent indent, StreamWriter w)
    {
        w.WriteLine("{");
        var inner = indent.Inc();
        for (int i = 0; i < Properties.Count; i++)
        {
            w.Write(inner); w.Write(JsonValue.EscapeString(Properties[i].Key));
            w.Write(": ");
            Properties[i].Value.WriteTo(inner, w);
            if (i + 1 < Properties.Count)
                w.WriteLine(',');
            else
                w.WriteLine();
        }
        w.Write(indent); w.Write("}");
    }

    internal void RemoveEmptyObjects()
    {
        for (int i = 0; i < Properties.Count; i++)
        {
            if (Properties[i].Value is JsonObject o && o.Properties.Count == 0)
            {
                Properties.RemoveAt(i--);
            }
            else if (Properties[i].Value is JsonArray a && a.IsEmpty)
            {
                Properties.RemoveAt(i--);
            }
        }
    }
}


internal class JsonException : JsonArray
{
    public JsonException(Exception source)
        :base($"Exception", new OverflowGuard())
    {
        if (source is AggregateException agg)
        {
            foreach (var inner in agg.InnerExceptions)
                Add(new JsonException(inner));
        }
        else
        {
            while (source != null)
            {
                var g = new OverflowGuard();
                var inner = new JsonObject($"Exception",g);
                inner.AddValue("ExceptionType", source.GetType().Name);
                inner.AddArray("StackTrace").AddAsValues(source.StackTrace.Split('\n').Select(x => x.Trim()));
                inner.AddValue($"Message", source.Message);
                Add(inner);
                source = source.InnerException;
            }
        }
    }

}

internal class Counter
{
    public Counter(string name, int max)
    {
        Max = max;
        Name = name;
    }

    public int Max { get; }
    public string Name { get; }
    public int Current { get; set; }
    public void Inc()
    {
        if (Current++ >= Max)
            throw new InvalidOperationException($"{Name} limit reached ({Max}). Further allocation prohibited");
    }
}

internal class OverflowGuard
{
    public readonly Counter objects = new Counter($"Object",10000);
    //public readonly Counter properties = new Counter($"Property",10000);
    public readonly Counter values = new Counter($"Value",100000);
    public readonly Counter arrays = new Counter($"Array",10000);
    public readonly Counter arrayElements = new Counter($"ArrayElement",10000);


    //private static void WriteLine(string line)
    //{
    //    File.AppendAllText(@"C:\Temp\Logs\json.log", $"{line}\r\n");
    //}


    internal void SignalNewArray(string name)
    {
        //WriteLine($"+Array[{name}]");
        //arrays.Inc();
    }

    internal void SignalNewArrayElement()
    {
        //WriteLine($"+ArrayElement");
        //arrayElements.Inc();
    }

    internal void SignalNewObject(string name)
    {
        //WriteLine($"+Object[{name}]");
        //objects.Inc();
    }

    internal void SignalNewProperty(string name)
    {
        //WriteLine($"+Property[{name}]");
    }

    internal void SignalNewValue()
    {
        values.Inc();
    }

    internal void SignalPropertyAccess(PropertyInfo f)
    {
        //WriteLine($"+PAccess[{f.Name}] type {f.PropertyType.Name}");
    }
}


public class HierarchyAnalyzer
{
    private OverflowGuard Guard { get; } = new OverflowGuard();

    private static void Log(Indent indent, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;
        ConsoleControl.Write(indent + msg);
    }
    private static void LogMultiLine(Indent indent, string firstLine, IEnumerable<string> nextLines)
    {
        ConsoleControl.Write(indent + firstLine);
        indent = indent.Inc();
        foreach (var line in nextLines)
            ConsoleControl.Write(indent + line);

    }

    private JsonNode MaterialToJsonNode(Material m)
    {
        try
        {
            if (ReferenceEquals(m, null))
                return JsonValue.Null;
            if (m == null)
                return new SoftNull(m.GetType(), Guard);
            if (VisitedBefore(m))
                return new JsonReference(m, Guard);

            JsonObject rs = new JsonObject($"Material "+m.name,Guard);
            rs.SetObjectProperties(m);




            int numProperties = m.shader.GetPropertyCount();
            var properties = rs.AddObject($"MaterialProperties");
            for (int i = 0; i < numProperties; i++)
            {
                var type = m.shader.GetPropertyType(i);
                var name = m.shader.GetPropertyName(i);
                try
                {
                    switch (type)
                    {
                        case UnityEngine.Rendering.ShaderPropertyType.Color:
                            properties.Add($"#{i} {name}({type})", ToJsonNode(m.GetColor(name), m));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                            properties.Add($"#{i} {name}({type})", ToJsonNode(m.GetVector(name), m));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Float:
                            properties.Add($"#{i} {name}({type})", ToJsonNode(m.GetFloat(name), m));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Range:
                            var range = m.shader.GetPropertyRangeLimits(i);
                            properties.Add($"#{i} {name}({type} in [{range.x},{range.y}])", ToJsonNode(m.GetFloat(name), m));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                            {
                                var t = m.GetTexture(name);
                                properties.Add($"#{i} {name}({type})", ObjectToJson(t, true));
                            }
                            break;
                        default:
                            properties.AddValue($"#{i} {name}({type})", "<unsupported type>");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    properties.Add(name, new JsonException(ex));
                }
            }
            AddFieldsAndProperties(rs, m, true);

            rs.RemoveEmptyObjects();
            return rs;
        }
        catch (Exception ex)
        {
            return new JsonException(ex);
        }
    }


    private JsonNode ToJsonNode(object v, object owner)
    {
        try
        {
            switch (v)
            {
                case double _:
                case float _:
                case int _:
                case bool _:
                case long _:
                case short _:
                    return new JsonValue(v, Guard);
                case Vector2 _:
                case Vector3 _:
                case Vector4 _:
                    return new JsonValue(v, Guard);
                case Matrix4x4 mat:
                    {
                        JsonObject rs = new JsonObject(owner.ToString(), Guard);
                        rs.Add("row0",ToJsonNode(mat.GetRow(0),v));
                        rs.Add("row1",ToJsonNode(mat.GetRow(1),v));
                        rs.Add("row2",ToJsonNode(mat.GetRow(2),v));
                        rs.Add("row3",ToJsonNode(mat.GetRow(3),v));
                        return rs;
                    }
                case Transform t:
                    return ToJson(t,true);
                case Texture t:
                    return ObjectToJson(t, !typeof(Texture).IsAssignableFrom(owner.GetType()));

                case Material[] ms:
                    {
                        JsonArray ar = new JsonArray(owner.ToString(),Guard);
                        foreach (var m in ms)
                        {
                            ar.Add(MaterialToJsonNode(m));
                        }
                        return ar;
                    }
                case Material m:
                    return MaterialToJsonNode(m);
                case Component c:
                    return ComponentToJson(c);
                case Shader c:
                    return ObjectToJson(c, true);
                case UnityEngine.Object o:
                    return ObjectToJson(o, false);
                case null:
                    return JsonValue.Null;
                case IDirectionSource t:
                    return ComplexToJson(t);

                default:
                    return new JsonValue(v, Guard);
            }
        }
        catch (Exception ex)
        {
            return new JsonException(ex);

        }




    }

    public void LogToJson(Component c, string filename)
    {
        ComponentToJson(c).SaveTo(filename);
    }
    public void LogToJson(Transform t, string filename)
    {
        ToJson(t,false).SaveTo(filename);
    }
    private JsonNode ToJson(Transform t, bool nameOnly)
    {
        try
        {
            if (ReferenceEquals(t, null))
                return JsonValue.Null;
            if (t == null)
                return new SoftNull(t.GetType(), Guard);

            if (nameOnly || VisitedBefore(t))
                return new JsonReference(t, Guard);


            JsonObject o = new JsonObject("Transform "+t.name, Guard);
            o.SetComponentProperties(t);

            AddFieldsAndProperties(o, t, true);

            var components = o.AddArray($"Components");

            foreach (var c in t.GetComponents<Component>())
            {
                components.Add(ComponentToJson(c));
            }
            if (t.childCount > 0)
            {
                var children = o.AddArray("Children");
                for (int i = 0; i < t.childCount; i++)
                    children.Add(ToJson(t.GetChild(i), false));
            }
            return o;
        }
        catch (Exception ex)
        {
            return new JsonException(ex);
        }
    }

    private JsonNode ComplexToJson(object any)
    {
        try
        {
            if (ReferenceEquals(any, null))
                return JsonValue.Null;
            if (any == null)
                return new SoftNull(any.GetType(), Guard);
            if (ComplexVisited(any))
                return new JsonValue($"[{any.GetType()}] {any}", Guard);

            JsonObject o = new JsonObject("Complex "+any,Guard);
            o.AddValue("Class", any.GetType().Name);
            AddFieldsAndProperties(o, any);

            return o;
        }
        catch (Exception ex)
        {
            return new JsonException(ex);
        }
    }

    private JsonNode ObjectToJson(UnityEngine.Object c, bool asObject)
    {
        try
        {

            if (ReferenceEquals(c, null))
                return JsonValue.Null;
            if (c == null)
                return new SoftNull(c.GetType(), Guard);
            if (!asObject || VisitedBefore(c))
                return new JsonReference(c, Guard);
            JsonObject o = new JsonObject("Object "+c.name,  Guard);
            o.SetObjectProperties(c);
            AddFieldsAndProperties(o, c, true);

            return o;
        }
        catch (Exception ex)
        {
            return new JsonException(ex);
        }
    }

    private JsonNode ComponentToJson(Component c)
    {
        try
        {
            if (ReferenceEquals(c, null))
                return JsonValue.Null;
            if (c == null)
                return new SoftNull(c.GetType(), Guard);
            if (VisitedBefore(c))
                return new JsonReference(c, Guard);
            JsonObject o = new JsonObject("Component "+c.name, Guard);
            o.SetComponentProperties(c);

            AddFieldsAndProperties(o, c, true);

            return o;
        }
        catch (Exception ex)
        {
            return new JsonException(ex);
        }
    }

    private HashSet<int> Visited { get; } = new HashSet<int>();
    private HashSet<object> VisitedComplex {get; } = new HashSet<object>();
    private bool VisitedBefore(UnityEngine.Object o)
    {
        return !Visited.Add(o.GetInstanceID());
    }
    private bool ComplexVisited(object o)
        => !VisitedComplex.Add(o);

    private void AddFieldsAndProperties(JsonObject target, object c, bool ignoreVisited = false)
    {
        if (c != null)
        {
            if (c is UnityEngine.Object uo)
            {
                if (uo == null || (!ignoreVisited && VisitedBefore(uo)))
                    return;
            }
            var type = c.GetType();
            var fields = target.AddObject($"Fields");
            foreach (var f in type.GetFields())
            {
                try
                {
                    if (f.IsDefined(typeof(ObsoleteAttribute), true))
                        continue;

                    var v = f.GetValue(c);
                    fields.Add(f.Name, ToJsonNode(v, c));
                }
                catch (Exception ex)
                {
                    fields.Add(f.Name, new JsonException(ex));
                }
            }
            var properties = target.AddObject($"Properties");
            foreach (var f in type.GetProperties())
            {
                try
                {
                    if (f.IsDefined(typeof(ObsoleteAttribute), true))
                        continue;
                    if (f.Name == $"renderingDisplaySize")
                    {
                        properties.AddValue(f.Name, "Excluded from export due to stability concerns");
                        continue;
                    }
                    Guard.SignalPropertyAccess(f);
                    var v = f.GetValue(c);
                    properties.Add(f.Name, ToJsonNode(v, c));
                }
                catch (Exception ex)
                {
                    properties.Add(f.Name, new JsonException(ex));
                }
            }
            target.RemoveEmptyObjects();
        }
    }


}
