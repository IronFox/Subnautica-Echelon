using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class HierarchyAnalyzer
{
    private static string O2S(UnityEngine.Object o)
    {
        if (ReferenceEquals(o, null))
            return "Object <hard null>";

        if (o == null)
            return $"{o.GetType()} <soft null>";
        return $"{o.GetType()} '{o.name}' ids={ o.GetInstanceID()}";

    }

    private static string C2S(Component c)
    {
        if (ReferenceEquals(c, null))
            return "Component <hard null>";

        if (c == null)
            return $"{c.GetType()} <soft null>";
        return $"{c.GetType().Name} '{c.name}' ids={ c.GetInstanceID()}/{ c.gameObject.GetInstanceID()}";
    }


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

    private void LogMaterial(Indent indent, Material m)
    {

        if (m != null && !VisitedBefore(m))
        {
            LogMembersOf(indent, m);
            int numProperties = m.shader.GetPropertyCount();
            Log(indent, $"Properties ({numProperties})");
            var i3 = indent.Inc();
            for (int i = 0; i < numProperties; i++)
            {
                var type = m.shader.GetPropertyType(i);
                var name = m.shader.GetPropertyName(i);
                try
                {
                    switch (type)
                    {
                        case UnityEngine.Rendering.ShaderPropertyType.Color:
                            Log(i3, $"'{name}' ({type}) := " + m.GetColor(name));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                            Log(i3, $"'{name}' ({type}) := " + m.GetVector(name));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Float:
                        case UnityEngine.Rendering.ShaderPropertyType.Range:
                            Log(i3, $"'{name}' ({type}) := " + m.GetFloat(name));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                            {
                                var t = m.GetTexture(name);
                                Log(i3, $"'{name}' ({type}) := {O2S(t)}");
                                LogMembersOf(i3.Inc(), t);
                            }
                            break;
                        default:
                            Log(i3, $"'{name}' ({type}) := <unsupported type>");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log(i3, $"'{name}' ({type}) := Exception: " + AllMessages(ex));
                }
            }
        }
    }


    private void LogValue(Indent indent, string msg, string fieldPropertName, object v)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;

        try
        {
            switch (v)
            {
                case double d:
                    Log(indent, msg + " := " + d.ToString(CultureInfo.InvariantCulture));
                    break;
                case float d:
                    Log(indent, msg + " := " + d.ToString(CultureInfo.InvariantCulture));
                    break;
                case int _:
                case bool _:
                case long _:
                case short _:
                case Vector2 _:
                case Vector3 _:
                case Vector4 _:
                    Log(indent, msg + " := " + v);
                    break;
                case Matrix4x4 _:
                    LogMultiLine(indent, msg + " :=", v.ToString().Split('\n'));
                    break;
                case Transform t:
                    Log(indent, $"{msg} := Transform {C2S(t)}");
                    break;
                case Texture2D t:
                    Log(indent, $"{msg} := {O2S(t)}");
                    LogMembersOf(indent.Inc(), t);
                    break;

                case Material[] ms:
                    {
                        int at = 0;
                        foreach (var m in ms)
                        {
                            Log(indent, $"{msg}[{at++}] := {O2S(m)}");
                            LogMaterial(indent.Inc(), m);
                        }
                    }
                    break;

                case Material m:
                    {
                        Log(indent, $"{msg} := {O2S(m)}");
                        LogMaterial(indent.Inc(), m);
                    }
                    break;
                case Component c:
                    {
                        Log(indent, $"{msg} := {O2S(c)}");
                        var i2 = indent.Inc();
                        if (c != null)
                            LogMembersOf(i2, c);
                    }
                    break;
                case UnityEngine.Object o:
                    {
                        Log(indent, $"{msg} := {O2S(o)}");
                        //var i2 = indent.Inc();
                        //LogMembersOf(i2, o);
                    }
                    break;
                default:
                    Log(indent, msg + " := " + v);
                    break;

            }
        }
        catch (Exception ex)
        {
            Log(indent, msg + " := Exception: " + AllMessages(ex));

        }




    }

    public void LogTree(Transform t, Indent indent = default)
    {
        if (t == null)
        {
            Log(indent, "Null Transform");
            return;
        }
        Log(indent, $"Transform {C2S(t)}");
        indent = indent.Inc();


        Log(indent, "Components");
        var s1 = indent.Inc();
        foreach (var c in t.GetComponents<Component>())
        {
            Log(indent, C2S(c));
            LogMembersOf(indent.Inc(), c);

        }
        if (t.childCount > 0)
        {

            Log(indent, "Children");

            for (int i = 0; i < t.childCount; i++)
                LogTree(t.GetChild(i), indent.Inc());
        }
    }

    private HashSet<int> Visited { get; } = new HashSet<int>();

    private bool VisitedBefore(UnityEngine.Object o)
    {
        return !Visited.Add(o.GetInstanceID());
    }

    private void LogMembersOf(Indent indent, object c)
    {
        if (c != null)
        {
            if (c is UnityEngine.Object uo)
            {
                if (uo == null || VisitedBefore(uo))
                    return;
            }
            var type = c.GetType();
            foreach (var f in type.GetFields())
            {
                try
                {
                    if (f.IsDefined(typeof(ObsoleteAttribute), true))
                        continue;

                    var v = f.GetValue(c);
                    LogValue(indent, $"Field '{f.Name}' ({f.FieldType.Name})", f.Name, v);
                }
                catch (Exception ex)
                {
                    Log(indent, $"Caught exception while querying {f.Name} ({f.FieldType.Name}): {AllMessages(ex)}");
                }
            }
            foreach (var f in type.GetProperties())
            {
                try
                {
                    if (f.IsDefined(typeof(ObsoleteAttribute), true))
                        continue;
                    var v = f.GetValue(c);
                    LogValue(indent, $"Property '{f.Name}' ({f.PropertyType.Name})", f.Name, v);
                }
                catch (Exception ex)
                {
                    Log(indent, $"Caught exception while querying {f.Name} ({f.PropertyType.Name}): {AllMessages(ex)}");
                }
            }
        }
    }


    private static string AllMessages(Exception ex)
    {
        string rs = ex.Message;
        if (ex.InnerException != null)
            rs += "<-" + AllMessages(ex.InnerException);
        return rs;
    }

}
