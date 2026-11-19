using UnityEngine;

namespace Periodiq.Reactions
{
    /// <summary>Standard CPK-style molecule colours and display sizes (ångströms).</summary>
    public static class CpkColors
    {
        public static Color For(string symbol)
        {
            switch (symbol)
            {
                case "H":  return Hex("#F2F2F2");
                case "C":  return Hex("#404040");
                case "N":  return Hex("#3050F8");
                case "O":  return Hex("#FF2A2A");
                case "F":
                case "Cl": return Hex("#1FF01F");
                case "S":  return Hex("#FFD52A");
                case "P":  return Hex("#FF8000");
                case "Li":
                case "Na":
                case "K":  return Hex("#9B5CF6");
                case "Mg":
                case "Ca": return Hex("#2E9E3A");
                default:   return Hex("#FF8FD0");
            }
        }

        public static float DisplayRadius(string symbol)
        {
            switch (symbol)
            {
                case "H":  return 0.32f;
                case "C":  return 0.50f;
                case "N":  return 0.48f;
                case "O":  return 0.46f;
                case "F":  return 0.42f;
                case "Cl": return 0.62f;
                case "S":  return 0.60f;
                case "Li": return 0.60f;
                case "Na": return 0.75f;
                case "K":  return 0.85f;
                case "Mg": return 0.65f;
                case "Ca": return 0.80f;
                default:   return 0.55f;
            }
        }

        static Color Hex(string h)
        {
            ColorUtility.TryParseHtmlString(h, out var c);
            return c;
        }
    }
}
