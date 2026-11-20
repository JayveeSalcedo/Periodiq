using System;
using System.Collections.Generic;
using System.Linq;
using Periodiq.Reactions;

namespace Periodiq.Discovery
{
    public class Badge
    {
        public string id;
        public string title;
        public string description;
        public Func<DiscoveryStore, bool> isEarned;
    }

    /// <summary>All badges. Add more here; they're checked after every new discovery.</summary>
    public static class Badges
    {
        public static readonly IReadOnlyList<Badge> All = new List<Badge>
        {
            new() { id = "first_bond", title = "First Bond",
                description = "Make your first compound.",
                isEarned = s => s.FoundCount >= 1 },

            new() { id = "first_ionic", title = "Opposites Attract",
                description = "Make your first ionic compound.",
                isEarned = s => Compounds().Any(c => c.IsIonic && s.IsFound(c.id)) },

            new() { id = "organic", title = "Organic Starter",
                description = "Make methane (CH<sub>4</sub>), the simplest organic molecule.",
                isEarned = s => s.IsFound("ch4") },

            new() { id = "triple", title = "Triple Threat",
                description = "Make a molecule with a triple bond.",
                isEarned = s => Compounds().Any(c => s.IsFound(c.id) && c.bonds != null && c.bonds.Any(b => b.order == 3)) },

            new() { id = "air", title = "Air Collector",
                description = "Make all four gases H<sub>2</sub>, O<sub>2</sub>, N<sub>2</sub> and Cl<sub>2</sub>.",
                isEarned = s => new[] { "h2", "o2", "n2", "cl2" }.All(s.IsFound) },

            new() { id = "halfway", title = "Halfway There",
                description = "Discover half of all compounds.",
                isEarned = s => s.FoundCount * 2 >= s.Total },

            new() { id = "salts", title = "Salt Master",
                description = "Make every ionic compound.",
                isEarned = s => Compounds().Where(c => c.IsIonic).All(c => s.IsFound(c.id)) },

            new() { id = "chemist", title = "Master Chemist",
                description = "Discover every compound.",
                isEarned = s => s.FoundCount >= s.Total },
        };

        static IEnumerable<CompoundData> Compounds() => CompoundDatabase.All;
    }
}
