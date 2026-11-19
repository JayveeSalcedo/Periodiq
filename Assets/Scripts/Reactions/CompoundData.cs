using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Periodiq.Reactions
{
    [Serializable]
    public class AtomSpec
    {
        public string symbol;
        public Vector3 position; // ångströms, molecule centred on origin
    }

    [Serializable]
    public class BondSpec
    {
        public int a;
        public int b;
        public int order; // 1 single, 2 double, 3 triple, 0 ionic (no stick)
    }

    [Serializable]
    public class CompoundData
    {
        public string id;
        public string name;
        public string formula;
        public string bondType; // "covalent" | "ionic"
        public string description;
        public AtomSpec[] atoms;
        public BondSpec[] bonds;

        public bool IsIonic => bondType == "ionic";

        Dictionary<string, int> _composition;
        public IReadOnlyDictionary<string, int> Composition
        {
            get
            {
                if (_composition != null) return _composition;
                _composition = new Dictionary<string, int>();
                foreach (var a in atoms)
                    _composition[a.symbol] = _composition.TryGetValue(a.symbol, out var c) ? c + 1 : 1;
                return _composition;
            }
        }

        public int AtomCount => atoms?.Length ?? 0;
    }

    [Serializable]
    class CompoundList
    {
        public CompoundData[] compounds;
    }

    /// <summary>Loads Resources/compounds.json and matches a bag of element symbols to a compound.</summary>
    public static class CompoundDatabase
    {
        static CompoundData[] _all;

        public static IReadOnlyList<CompoundData> All
        {
            get { EnsureLoaded(); return _all; }
        }

        /// <summary>Compound whose composition exactly equals the given counts, or null.</summary>
        public static CompoundData FindExact(IReadOnlyDictionary<string, int> counts)
        {
            EnsureLoaded();
            foreach (var c in _all)
                if (SameCounts(c.Composition, counts)) return c;
            return null;
        }

        /// <summary>True if some compound has strictly more atoms and contains all of these
        /// (e.g. H+H is part of H₂O). Used to wait a bit longer before reacting.</summary>
        public static bool IsPartOfLarger(IReadOnlyDictionary<string, int> counts)
        {
            EnsureLoaded();
            int total = counts.Values.Sum();
            foreach (var c in _all)
            {
                if (c.AtomCount <= total) continue;
                bool contains = true;
                foreach (var kv in counts)
                {
                    if (!c.Composition.TryGetValue(kv.Key, out var n) || n < kv.Value) { contains = false; break; }
                }
                if (contains) return true;
            }
            return false;
        }

        public static Dictionary<string, int> Count(IEnumerable<string> symbols)
        {
            var d = new Dictionary<string, int>();
            foreach (var s in symbols) d[s] = d.TryGetValue(s, out var c) ? c + 1 : 1;
            return d;
        }

        static bool SameCounts(IReadOnlyDictionary<string, int> a, IReadOnlyDictionary<string, int> b)
        {
            if (a.Count != b.Count) return false;
            foreach (var kv in a)
                if (!b.TryGetValue(kv.Key, out var n) || n != kv.Value) return false;
            return true;
        }

        static void EnsureLoaded()
        {
            if (_all != null) return;
            var asset = Resources.Load<TextAsset>("compounds");
            if (asset == null)
            {
                Debug.LogError("compounds.json not found in a Resources folder.");
                _all = Array.Empty<CompoundData>();
                return;
            }
            _all = JsonUtility.FromJson<CompoundList>(asset.text).compounds;
        }
    }
}
