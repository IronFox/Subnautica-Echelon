public static class ObjectUtil
{
    public static T Or<T>(this T obj, T fallbackObject) where T : UnityEngine.Object
    {
        return obj ? obj : fallbackObject;
    }

}