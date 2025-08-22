using UnityEngine;

public class ColorGet
{
    /// <summary>
    /// RGB を 0 ～ 255 で指定したカラー値を取得
    /// </summary>
    /// <param name="r">赤</param>
    /// <param name="g">緑</param>
    /// <param name="b">青</param>
    public static Color Rgb(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }

    /// <summary>
    /// カラーを #RRGGBB の形で取得
    /// </summary>
    /// <param name="hexrgb">16進数のカラー値 RRGGBB</param>
    public static Color Hex(int hexrgb)
    {
        int r = (hexrgb >> 16) & 0xff;
        int g = (hexrgb >> 8) & 0xff;
        int b = hexrgb & 0xff;

        return Rgb(r, g, b);
    }
}
