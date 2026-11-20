using System;
using System.Collections.Generic;
using System.Linq;
using Periodiq.Reactions;
using UnityEngine;

namespace Periodiq.Discovery
{
    /// <summary>
    /// Remembers which compounds the student has made and which badges they've earned.
    /// Saved on the phone (PlayerPrefs), so progress survives closing the app.
    /// </summary>
    public class DiscoveryStore : MonoBehaviour
    {
        [SerializeField] ReactionZone zone;

        const string SaveKey = "periodiq.discovery.v1";

        readonly HashSet<string> _found = new();
        readonly HashSet<string> _badges = new();

        /// <summary>A compound was made for the first time.</summary>
        public event Action<CompoundData> NewDiscovery;
        public event Action<Badge> BadgeEarned;
        /// <summary>Anything changed (refresh UI).</summary>
        public event Action Changed;

        public int FoundCount => _found.Count;
        public int Total => CompoundDatabase.All.Count;
        public bool IsFound(string compoundId) => _found.Contains(compoundId);
        public bool HasBadge(string badgeId) => _badges.Contains(badgeId);

        [Serializable]
        class SaveData
        {
            public string[] found;
            public string[] badges;
        }

        void Awake() => Load();

        void OnEnable() { if (zone) zone.CompoundFormed += OnFormed; }
        void OnDisable() { if (zone) zone.CompoundFormed -= OnFormed; }

        void OnFormed(CompoundData c, Molecule m)
        {
            if (!_found.Add(c.id)) return;
            NewDiscovery?.Invoke(c);
            CheckBadges();
            Save();
            Changed?.Invoke();
        }

        void CheckBadges()
        {
            foreach (var b in Badges.All)
            {
                if (_badges.Contains(b.id) || !b.isEarned(this)) continue;
                _badges.Add(b.id);
                BadgeEarned?.Invoke(b);
            }
        }

        void Load()
        {
            _found.Clear();
            _badges.Clear();
            var json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                // Ignore ids that no longer exist in compounds.json.
                var valid = new HashSet<string>(CompoundDatabase.All.Select(c => c.id));
                foreach (var id in data.found ?? Array.Empty<string>()) if (valid.Contains(id)) _found.Add(id);
                foreach (var id in data.badges ?? Array.Empty<string>()) _badges.Add(id);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Discovery save unreadable, starting fresh: {e.Message}");
            }
        }

        void Save()
        {
            var data = new SaveData { found = _found.ToArray(), badges = _badges.ToArray() };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        [ContextMenu("Reset progress")]
        public void ResetProgress()
        {
            _found.Clear();
            _badges.Clear();
            PlayerPrefs.DeleteKey(SaveKey);
            Changed?.Invoke();
        }
    }
}
