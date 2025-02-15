using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Subnautica_Echelon
{
    public static class Helper
    {
        public static IEnumerable<Transform> Children(Transform t)
        {
            if (t == null)
                yield break;
            for (int i = 0; i < t.childCount; i++)
                yield return t.GetChild(i);
        }


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
                    yield return obj.name;
        }
        public static string NamesS(IEnumerable<UnityEngine.Component> source)
            => S(Names(source));
        public static IEnumerable<string> Names(IEnumerable<UnityEngine.Component> source)
        {
            foreach (var obj in source)
            {
                if (obj == null)
                    yield return "<null>";
                else
                    yield return obj.name;
            }
        }
        public static IEnumerable<string> Names(IEnumerable<FieldInfo> source)
        {
            foreach (var obj in source)
                yield return obj.Name;
        }

        public static string S(IEnumerable<string> source)
            => string.Join(", ", source);

        public static Component FindComponentInChildren(Transform t, string componentTypeName)
        {
            var c = t.GetComponent(componentTypeName);
            if (c != null)
                return c;
            for (int i = 0; i < t.childCount; i++)
            {
                c = FindComponentInChildren(t.GetChild(i), componentTypeName);
                if (c != null)
                    return c;
            }
            return null;
        }
    }
}
