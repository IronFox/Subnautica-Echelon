﻿using Subnautica_Echelon.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Subnautica_Echelon.Util
{
    public static class Helper
    {


        public static IEnumerable<Component> AllComponents(Transform t)
        {
            if (t == null)
                return Array.Empty<Component>();

            return t.GetComponents<Component>();
        }
        public static IEnumerable<string> Names(IEnumerable<UnityEngine.Object> source)
        {
            foreach (var obj in source)
                if (obj == null)
                    yield return "<null>";
                else
                    yield return obj.NiceName();
        }
        public static string NamesS(IEnumerable<Component> source)
            => S(Names(source));
        public static IEnumerable<string> Names(IEnumerable<Component> source)
        {
            foreach (var obj in source)
            {
                if (obj == null)
                    yield return "<null>";
                else
                    yield return obj.NiceName();
            }
        }
        public static IEnumerable<string> Names(IEnumerable<FieldInfo> source)
        {
            foreach (var obj in source)
                yield return obj.Name;
        }

        public static string S(IEnumerable<string> source)
            => string.Join(", ", source);


        public static T Clone<T>(T obj) where T : new()
        {
            T copy = new T();
            foreach (var f in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                Log.Write($"Duplicating property {f} on {obj} to {copy}");
                f.SetValue(copy, f.GetValue(obj));
            }
            foreach (var p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (p.CanWrite)
                {
                    Log.Write($"Duplicating property {p} on {obj} to {copy}");
                    p.SetValue(copy, p.GetValue(obj));
                }
                else
                    Log.Write($"Cannot duplicate property {p} on {obj} to {copy} (readonly)");


            return copy;
        }
    }
}
