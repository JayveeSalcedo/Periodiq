using UnityEngine;

namespace Periodiq.Data
{
    /// <summary>Tile colour per element category. Tweak freely for your art direction.</summary>
    public static class CategoryColors
    {
        public static Color For(string category)
        {
            if (string.IsNullOrEmpty(category)) return Hex("#9AA0A6");
            if (category.StartsWith("unknown")) return Hex("#9AA0A6");
            switch (category)
            {
                case "alkali metal":           return Hex("#FF6B6B");
                case "alkaline earth metal":   return Hex("#FFA94D");
                case "transition metal":       return Hex("#FFD43B");
                case "post-transition metal":  return Hex("#A9E34B");
                case "metalloid":              return Hex("#38D9A9");
                case "diatomic nonmetal":
                case "polyatomic nonmetal":    return Hex("#4DABF7");
                case "noble gas":              return Hex("#B197FC");
                case "lanthanide":             return Hex("#F783AC");
                case "actinide":               return Hex("#E599F7");
                default:                       return Hex("#9AA0A6");
            }
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
