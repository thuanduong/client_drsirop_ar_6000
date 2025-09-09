using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ImageHelper
{
	public static Sprite CreateSprite(Texture2D tex)
	{
		if (tex == null)
		{
			tex = new Texture2D(1, 1);
		}
		return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
	}

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255;

        if (hex.Length == 6)
        {
            // RRGGBB
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            // RRGGBBAA
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else
        {
            Debug.LogError("Invalid hex string. Must be 6 or 8 characters long.");
            return Color.black;
        }

        return new Color32(r, g, b, a);
    }
}
