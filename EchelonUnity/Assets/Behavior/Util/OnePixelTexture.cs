using System.Collections.Generic;
using UnityEngine;

public static class OnePixelTexture
{
    private static Dictionary<Color, Texture2D> _colorTextureCache = new Dictionary<Color, Texture2D>();


    public static Texture2D Get(Color color)
    {
        if (!_colorTextureCache.TryGetValue(color, out var texture))
        {
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            _colorTextureCache[color] = texture;
        }
        return texture;
    }

    public static Texture2D Get(Color32 color)
    {
        return Get(new Color(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f));
    }

    public static Texture2D GetGray(float gray)
    {
        return Get(new Color(gray, gray, gray, 1f));
    }
}