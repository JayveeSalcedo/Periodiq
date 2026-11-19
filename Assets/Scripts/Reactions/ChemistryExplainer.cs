using System.Collections.Generic;
using System.Linq;
using System.Text;
using Periodiq.Data;

namespace Periodiq.Reactions
{
    /// <summary>
    /// Turns element data into short, student-friendly explanations.
    /// Output uses TextMeshPro rich text (&lt;sub&gt;, &lt;sup&gt;, &lt;b&gt;).
    /// </summary>
    public static class ChemistryExplainer
    {
        // ---------- Element helpers ----------

        public static bool IsNobleGas(ElementData e) => e.category == "noble gas";

        public static bool IsMetal(ElementData e) =>
            e.category != null && e.category.Contains("metal") && !e.category.Contains("nonmetal") && e.category != "metalloid";

        public static int Valence(ElementData e) => e.ValenceElectrons;

        /// <summary>Electrons needed for a full outer shell (2 for H/He, 8 otherwise).</summary>
        public static int Needs(ElementData e)
        {
            int full = e.number <= 2 ? 2 : 8;
            return System.Math.Max(0, full - Valence(e));
        }

        // ---------- Success ----------

        public static string ExplainCompound(CompoundData c)
        {
            var elements = c.Composition.Keys.Select(ElementDatabase.BySymbol).Where(e => e != null).ToList();
            var sb = new StringBuilder();

            if (c.IsIonic)
            {
                var metal = elements.FirstOrDefault(IsMetal);
                var nonmetal = elements.FirstOrDefault(e => !IsMetal(e));
                if (metal != null && nonmetal != null)
                {
                    int give = Valence(metal);
                    int take = Needs(nonmetal);
                    int nMetal = c.Composition[metal.symbol];
                    int nNon = c.Composition[nonmetal.symbol];
                    sb.Append($"<b>Ionic bond:</b> {metal.name} gives away its {Plural(give, "outer electron")}");
                    sb.Append($" and {nonmetal.name} takes {Plural(take, "electron")} to fill its outer shell.\n");
                    sb.Append($"{metal.symbol} becomes {Ion(metal.symbol, give, true)} and {nonmetal.symbol} becomes {Ion(nonmetal.symbol, take, false)}. ");
                    sb.Append("Opposite charges attract and hold them together.");
                    if (nMetal != nNon)
                        sb.Append($"\nThat's why it takes {nMetal} {metal.symbol} for every {nNon} {nonmetal.symbol}: {nMetal}×{give} = {nNon}×{take} electrons.");
                    return sb.ToString();
                }
            }

            sb.Append("<b>Covalent bond:</b> these nonmetals <b>share</b> pairs of electrons so every atom ends up with a full outer shell.\n");
            foreach (var e in elements)
            {
                int need = Needs(e);
                sb.Append($"• {e.symbol} has {Plural(Valence(e), "outer electron")}, so it shares {need} {(need == 1 ? "pair" : "pairs")}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }

        // ---------- Failure ----------

        public static string ExplainNoReaction(IReadOnlyList<ElementData> atoms)
        {
            var counts = CompoundDatabase.Count(atoms.Select(a => a.symbol));
            var kinds = counts.Keys.Select(ElementDatabase.BySymbol).Where(e => e != null).ToList();

            // 1. Noble gases don't bond.
            var noble = kinds.FirstOrDefault(IsNobleGas);
            if (noble != null)
                return $"<b>{noble.name}</b> is a noble gas. Its outer shell is already full ({Valence(noble)} electrons), so it has no reason to share, give or take electrons. Noble gases almost never bond.";

            // 2. Close to a real recipe? Tell them what to add/remove.
            var hint = NearestRecipeHint(counts);
            if (hint != null) return hint;

            // 3. Only metals.
            if (kinds.All(IsMetal))
                return "These are all <b>metals</b>. Metals want to <b>give away</b> electrons, but nothing here wants to take them. Add a nonmetal like Cl or O.";

            // 4. One metal + one nonmetal with the wrong ratio.
            var metals = kinds.Where(IsMetal).ToList();
            var nonmetals = kinds.Where(k => !IsMetal(k)).ToList();
            if (metals.Count == 1 && nonmetals.Count == 1)
            {
                var m = metals[0]; var n = nonmetals[0];
                int give = Valence(m), take = Needs(n);
                if (give > 0 && take > 0)
                {
                    int l = Lcm(give, take);
                    int needM = l / give, needN = l / take;
                    return $"{m.name} gives {Plural(give, "electron")} but {n.name} needs {take}. " +
                           $"The electrons must balance: try <b>{needM} {m.symbol} + {needN} {n.symbol}</b>.";
                }
            }

            // 5. Generic: show who still needs electrons.
            var sb = new StringBuilder("These atoms don't make a stable compound here.\n");
            foreach (var e in kinds)
                sb.Append($"• {e.symbol}: {Plural(Valence(e), "outer electron")}, needs {Needs(e)} more\n");
            sb.Append("Try a combination where every atom can fill its outer shell.");
            return sb.ToString();
        }

        static string NearestRecipeHint(IReadOnlyDictionary<string, int> counts)
        {
            CompoundData best = null;
            int bestDist = int.MaxValue;

            // Prefer "just add more" hints (C + H → add 3 H for methane) over swaps.
            int have = counts.Values.Sum();
            foreach (var c in CompoundDatabase.All)
            {
                bool superset = counts.All(kv => c.Composition.TryGetValue(kv.Key, out var n) && n >= kv.Value);
                int extra = c.AtomCount - have;
                if (superset && extra > 0 && extra <= 3 && extra < bestDist) { bestDist = extra; best = c; }
            }
            if (best != null) return BuildHint(best, counts);

            foreach (var c in CompoundDatabase.All)
            {
                int d = 0;
                foreach (var s in c.Composition.Keys.Union(counts.Keys))
                {
                    c.Composition.TryGetValue(s, out var a);
                    counts.TryGetValue(s, out var b);
                    d += System.Math.Abs(a - b);
                }
                if (d < bestDist) { bestDist = d; best = c; }
            }
            if (best == null || bestDist > 2) return null;
            return BuildHint(best, counts);
        }

        static string BuildHint(CompoundData best, IReadOnlyDictionary<string, int> counts)
        {
            var add = new List<string>();
            var remove = new List<string>();
            foreach (var s in best.Composition.Keys.Union(counts.Keys))
            {
                best.Composition.TryGetValue(s, out var want);
                counts.TryGetValue(s, out var have);
                if (want > have) add.Add($"{want - have} {s}");
                if (have > want) remove.Add($"{have - want} {s}");
            }

            string target = $"<b>{best.name}</b> ({FormatFormula(best.formula)})";
            if (remove.Count == 0) return $"Almost! Add <b>{string.Join(" + ", add)}</b> to make {target}.";
            if (add.Count == 0) return $"Too many atoms. Remove <b>{string.Join(" + ", remove)}</b> to make {target}.";
            return $"Close! Swap <b>{string.Join(" + ", remove)}</b> for <b>{string.Join(" + ", add)}</b> to make {target}.";
        }

        // ---------- Formatting ----------

        /// <summary>Converts unicode subscript digits (H₂O) to TMP rich text (H&lt;sub&gt;2&lt;/sub&gt;O),
        /// which works with any font.</summary>
        public static string FormatFormula(string formula)
        {
            var sb = new StringBuilder();
            foreach (char ch in formula)
            {
                if (ch >= '₀' && ch <= '₉') sb.Append("<sub>").Append((char)('0' + (ch - '₀'))).Append("</sub>");
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        static string Ion(string symbol, int charge, bool positive)
        {
            string sign = positive ? "+" : "-";
            return $"{symbol}<sup>{(charge > 1 ? charge.ToString() : "")}{sign}</sup>";
        }

        static string Plural(int n, string word) => n == 1 ? $"1 {word}" : $"{n} {word}s";

        static int Lcm(int a, int b) => a / Gcd(a, b) * b;
        static int Gcd(int a, int b) { while (b != 0) (a, b) = (b, a % b); return a; }
    }
}
