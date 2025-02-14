using UnityEngine;

public static class ColorManager {
    public static readonly ColorSet[] Colors = new ColorSet[] {
        new ColorSet(HexToColor("#FF003C"), HexToColor("#A10020")),
        new ColorSet(HexToColor("#FF0200"), HexToColor("#960100")),
        new ColorSet(HexToColor("#FF3800"), HexToColor("#961F00")),
        new ColorSet(HexToColor("#FF6200"), HexToColor("#CB3600")),
        new ColorSet(HexToColor("#FF8200"), HexToColor("#CB4800")),
        new ColorSet(HexToColor("#FFB100"), HexToColor("#B66200")),
        new ColorSet(HexToColor("#FFD100"), HexToColor("#B68F00")),
        new ColorSet(HexToColor("#FFFF00"), HexToColor("#A19F00")),
        new ColorSet(HexToColor("#F2FF00"), HexToColor("#84A100")),
        new ColorSet(HexToColor("#9FFF00"), HexToColor("#57A100")),
        new ColorSet(HexToColor("#00FF29"), HexToColor("#00A117")),
        new ColorSet(HexToColor("#00FFA7"), HexToColor("#008C5C")),
        new ColorSet(HexToColor("#00FFFF"), HexToColor("#00ABAA")),
        new ColorSet(HexToColor("#00FFFF"), HexToColor("#00A1CB")),
        new ColorSet(HexToColor("#00AEFF"), HexToColor("#005FCB")),
        new ColorSet(HexToColor("#0065FF"), HexToColor("#0037AB")),
        new ColorSet(HexToColor("#0000FF"), HexToColor("#0000A1")),
        new ColorSet(HexToColor("#6B00FF"), HexToColor("#3B00A1")),
        new ColorSet(HexToColor("#9D00FF"), HexToColor("#5600B6")),
        new ColorSet(HexToColor("#CB00FF"), HexToColor("#7000CB")),
        new ColorSet(HexToColor("#FF00FF"), HexToColor("#9500C0")),
        new ColorSet(HexToColor("#FF00FF"), HexToColor("#AB009D")),
        new ColorSet(HexToColor("#FF00E0"), HexToColor("#AB007B")),
        new ColorSet(HexToColor("#FF00A3"), HexToColor("#AB0059"))
    };
    public static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
    public static ColorSet GetColor(int id) {
        if (id >= 0 && id < Colors.Length) {
            return Colors[id];
        } else {
            return Colors[11];
        }
    }
}

public class ColorSet
{
    public readonly Color Primary;
    public readonly Color Secondary;

    // Constructor to initialize colors
    public ColorSet(Color primary, Color secondary)
    {
        Primary = primary;
        Secondary = secondary;
    }
}
