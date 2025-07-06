using System.Collections.Generic;
using UnityEngine;
using Logger = VehicleFramework.Logger;
using Object = UnityEngine.Object;

namespace Subnautica_Echelon.Util
{
    /// <summary>
    /// Various utility extensions and methods for querying or manipulating GameObjects and Components.
    /// </summary>
    public static class GameObjectHelper
    {
        /// <summary>
        /// Duplicates a source component onto another object, copying all its fields in the process.
        /// </summary>
        /// <typeparam name="T">Type being copied</typeparam>
        /// <param name="original">Original component</param>
        /// <param name="destination">Destination owner</param>
        /// <returns>Duplicated component</returns>
        public static T CopyComponentWithFieldsTo<T>(this T original, GameObject destination) where T : Component
        {
            if (!original)
            {
                Logger.Error($"Original component of type {typeof(T).Name} is null, cannot copy.");
                return null;
            }
            System.Type type = original.GetType();
            T copy = (T)destination.EnsureComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        /// <summary>
        /// Returns the first non-null object from the two provided.
        /// </summary>
        /// <typeparam name="T">Type being compared</typeparam>
        /// <param name="a">First object to return if not null</param>
        /// <param name="b">Second objec to return if <paramref name="a"/> is null</param>
        /// <returns><paramref name="a"/> if not null, <paramref name="b"/> if <paramref name="a"/> is null,
        /// null if both are null</returns>
        public static T Or<T>(this T a, T b) where T : Object
        {
            if (a)
                return a;
            return b;
        }

        /// <summary>
        /// Changes the active state of a GameObject and logs the action, including any exceptions that occur.
        /// </summary>
        /// <remarks>
        /// Does nothing if the object already matches the new state
        /// </remarks>
        /// <param name="gameObject">Game object being manipulated</param>
        /// <param name="toEnabled">New enabled state</param>
        public static void LoggedSetActive(this GameObject gameObject, bool toEnabled)
        {
            if (gameObject == null)
            {
                Logger.Error("GameObject is null, cannot set active state.");
                return;
            }
            try
            {
                if (gameObject.activeSelf != toEnabled)
                {
                    Logger.DebugLog($"Setting active state of {gameObject.NiceName()} to {toEnabled}");
                    gameObject.SetActive(toEnabled);
                    Logger.DebugLog($"Set active state of {gameObject.NiceName()} to {toEnabled}");
                }
            }
            catch (System.Exception e)
            {
                Logger.Error($"Failed to set active state of {gameObject.NiceName()}: {e.Message}");
                Logger.Error(e.StackTrace);
            }
        }

        /// <summary>
        /// Selectively returns the transform of a GameObject.
        /// Returns null if the GameObject is null.
        /// </summary>
        public static Transform GetTransform(this GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            return gameObject.transform;
        }

        /// <summary>
        /// Selectively returns the transform of a Component.
        /// Returns null if the Component is null.
        /// </summary>
        public static Transform GetTransform(this Component component)
        {
            if (component == null)
                return null;
            return component.transform;
        }
        /// <summary>
        /// Selectively returns the GameObject of a Component.
        /// Returns null if the Component is null.
        /// </summary>
        public static GameObject GetGameObject(this Component component)
        {
            if (component == null)
                return null;
            return component.gameObject;
        }

        /// <summary>
        /// Selectively returns the Texture2D of a Sprite.
        /// Returns null if the Sprite is null.
        /// </summary>
        public static Texture2D GetTexture2D(this Sprite sprite)
        {
            if (sprite == null)
                return null;
            return sprite.texture;
        }

        /// <summary>
        /// Queries a nicer representation of an Object for logging purposes.
        /// Includes the object's name, type, and instance ID.
        /// Returns "&lt;null&gt;" if the object is null.
        /// </summary>
        public static string NiceName(this Object o)
        {
            if (!o)
            {
                return "<null>";
            }

            string text = o.name;
            int num = text.IndexOf('(');
            if (num >= 0)
            {
                text = text.Substring(0, num);
            }

            return $"<{o.GetType().Name}> '{text}' [{o.GetInstanceID()}]";
        }

        /// <summary>
        /// Produces the full hierarchy path of a Transform as a single string using / as separator.
        /// Returns "&lt;null&gt;" if the Transform is null.
        /// </summary>
        public static string PathToString(this Transform t)
        {
            if (!t)
            {
                return "<null>";
            }

            List<string> list = new List<string>();
            try
            {
                while ((bool)t)
                {
                    list.Add($"{t.name}[{t.GetInstanceID()}]");
                    t = t.parent;
                }
            }
            catch (UnityException)
            {
            }

            list.Reverse();
            return string.Join("/", list);
        }

        /// <summary>
        /// Queries all children of a Transform as an <see cref="IEnumerable{T}" /> of Transforms.
        /// Returns an empty enumerable if the Transform is null or has no children.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>

        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            if (!transform)
            {
                yield break;
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }

        /// <summary>
        /// Gets the GameObject associated with a Collider.
        /// Favors the attached Rigidbody if available, otherwise uses the Collider's GameObject.
        /// Returns null if the Collider is null.
        /// </summary>
        public static GameObject GetGameObjectOf(Collider collider)
        {
            if (!collider)
                return null;
            if (collider.attachedRigidbody)
            {
                return collider.attachedRigidbody.gameObject;
            }

            return collider.gameObject;
        }

        /// <summary>
        /// Changes the active state of a MonoBehaviour and its parent hierarchy if necessary,
        /// such that the MonoBehaviour ends up active and enabled.
        /// Logs changes and errors as errors.
        /// </summary>
        /// <param name="c">Behavior to change the state of</param>
        /// <param name="rootTransform">Hierarchy root which will not be altered. If encountered, the loop stops</param>
        public static void RequireActive(this MonoBehaviour c, Transform rootTransform)
        {
            if (c.isActiveAndEnabled)
            {
                return;
            }

            if (!c.enabled)
            {
                Logger.Error($"{c} has been disabled. Re-enabling");
                c.enabled = true;
            }

            if (c.isActiveAndEnabled)
            {
                return;
            }

            Transform transform = c.transform;
            while (transform && transform != rootTransform)
            {
                if (!transform.gameObject.activeSelf)
                {
                    Logger.Error($"{transform.gameObject} has been deactivate. Re-activating");
                    transform.gameObject.SetActive(value: false);
                    if (c.isActiveAndEnabled)
                    {
                        return;
                    }
                }

                transform = transform.parent;
            }

            if (!rootTransform.gameObject.activeSelf)
            {
                Logger.Error($"{rootTransform.gameObject} has been deactivate. Re-activating");
                rootTransform.gameObject.SetActive(value: false);
            }
        }
    }

}
