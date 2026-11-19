using System;
using System.Collections.Generic;
using UnityEngine;

namespace Periodiq.Data
{
    [Serializable]
    public class ElementData
    {
        public int number;
        public string symbol;
        public string name;
        public float atomicMass;
        public int neutrons;
        public string category;
        public string phase;
        public int period;
        public int group;
        public int xpos;          // column on the standard table (1-18)
        public int ypos;          // row on the standard table (1-7, 9-10 for f-block)
        public int[] shells;      // electrons per shell, e.g. [2, 8, 1]
        public string electronConfiguration;
        public float electronegativity;

        public int ValenceElectrons => shells != null && shells.Length > 0 ? shells[shells.Length - 1] : 0;
    }

    [Serializable]
    class ElementList
    {
        public ElementData[] elements;
    }

    /// <summary>Loads Resources/elements.json once and gives lookup by number or symbol.</summary>
    public static class ElementDatabase
    {
        static ElementData[] _all;
        static Dictionary<string, ElementData> _bySymbol;

        public static IReadOnlyList<ElementData> All
        {
            get { EnsureLoaded(); return _all; }
        }

        public static ElementData BySymbol(string symbol)
        {
            EnsureLoaded();
            return _bySymbol.TryGetValue(symbol, out var e) ? e : null;
        }

        public static ElementData ByNumber(int number)
        {
            EnsureLoaded();
            return number >= 1 && number <= _all.Length ? _all[number - 1] : null;
        }

        static void EnsureLoaded()
        {
            if (_all != null) return;
            var asset = Resources.Load<TextAsset>("elements");
            if (asset == null)
            {
                Debug.LogError("elements.json not found in a Resources folder.");
                _all = Array.Empty<ElementData>();
                _bySymbol = new Dictionary<string, ElementData>();
                return;
            }
            _all = JsonUtility.FromJson<ElementList>(asset.text).elements;
            Array.Sort(_all, (a, b) => a.number.CompareTo(b.number));
            _bySymbol = new Dictionary<string, ElementData>();
            foreach (var e in _all) _bySymbol[e.symbol] = e;
        }
    }
}
