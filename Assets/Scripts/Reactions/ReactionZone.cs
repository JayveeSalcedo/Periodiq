using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Periodiq.Atoms;
using Periodiq.Data;
using UnityEngine;

namespace Periodiq.Reactions
{
    /// <summary>
    /// A glowing ring in front of the table. Drop atoms into it; once they've stopped
    /// changing for a moment, it checks the recipe list:
    ///   match    → atoms pull together and become the molecule
    ///   no match → ring flashes red and <see cref="ReactionFailed"/> fires (Feature 4 explains why)
    /// If the atoms so far are part of a bigger recipe (H+H could become H₂O) it waits longer.
    /// Put this on a child of the PeriodicTable so it's placed with the table.
    /// </summary>
    public class ReactionZone : MonoBehaviour
    {
        [SerializeField] MoleculeFactory moleculeFactory;
        [Tooltip("Atoms within this distance (metres) of the centre count as inside.")]
        [SerializeField] float radius = 0.12f;
        [SerializeField] float settleSeconds = 1.2f;
        [Tooltip("Used instead when the current atoms could still become a bigger compound.")]
        [SerializeField] float settleSecondsIfMoreLikely = 3f;
        [SerializeField] float pullSeconds = 0.5f;
        [SerializeField] float moleculeHeight = 0.08f;

        [Header("Ring visual")]
        [SerializeField] LineRenderer ring;
        [SerializeField] Color idleColor = new(0.3f, 0.8f, 1f, 0.6f);
        [SerializeField] Color readyColor = new(0.3f, 1f, 0.5f, 0.9f);
        [SerializeField] Color failColor = new(1f, 0.3f, 0.3f, 0.9f);

        /// <summary>Fired after a molecule forms.</summary>
        public event Action<CompoundData, Molecule> CompoundFormed;
        /// <summary>Fired when 2+ atoms settle in the zone but match no recipe.</summary>
        public event Action<IReadOnlyList<ElementData>> ReactionFailed;

        readonly List<Atom> _inside = new();
        string _signature = "";
        float _stableFor;
        bool _evaluated;
        bool _busy;
        float _flash;
        Molecule _current;

        public IReadOnlyList<Atom> AtomsInside => _inside;

        void Awake()
        {
            if (ring) BuildRing();
        }

        void Update()
        {
            if (_busy) return;

            CollectAtoms();
            var sig = string.Join(",", _inside.Select(a => a.Element.symbol).OrderBy(s => s));
            if (sig != _signature)
            {
                _signature = sig;
                _stableFor = 0f;
                _evaluated = false;
            }
            else _stableFor += Time.deltaTime;

            UpdateRingColor();

            if (_evaluated || _inside.Count == 0) return;

            var counts = CompoundDatabase.Count(_inside.Select(a => a.Element.symbol));
            var match = CompoundDatabase.FindExact(counts);
            bool moreLikely = CompoundDatabase.IsPartOfLarger(counts);
            float wait = moreLikely ? settleSecondsIfMoreLikely : settleSeconds;
            if (_stableFor < wait) return;

            _evaluated = true;
            if (match != null)
            {
                StartCoroutine(React(match, _inside.ToList()));
            }
            else if (_inside.Count >= 2)
            {
                // "Almost there" cases (H + O) get a hint but no red flash.
                if (!moreLikely) _flash = 1f;
                ReactionFailed?.Invoke(_inside.Select(a => a.Element).ToList());
            }
        }

        void CollectAtoms()
        {
            _inside.Clear();
            var centre = transform.position;
            foreach (var a in FindObjectsByType<Atom>(FindObjectsSortMode.None))
            {
                if (a.IsHeld || a.Locked || a.Element == null) continue;
                if ((a.transform.position - centre).sqrMagnitude <= radius * radius) _inside.Add(a);
            }
        }

        IEnumerator React(CompoundData compound, List<Atom> atoms)
        {
            _busy = true;
            foreach (var a in atoms) a.Locked = true;

            var target = transform.position + Vector3.up * moleculeHeight;
            var starts = atoms.Select(a => a.transform.position).ToArray();
            var scales = atoms.Select(a => a.transform.localScale).ToArray();
            for (float t = 0; t < 1f; t += Time.deltaTime / pullSeconds)
            {
                float e = t * t * (3f - 2f * t);
                for (int i = 0; i < atoms.Count; i++)
                {
                    atoms[i].transform.position = Vector3.Lerp(starts[i], target, e);
                    atoms[i].transform.localScale = scales[i] * (1f - e * 0.8f);
                }
                yield return null;
            }
            foreach (var a in atoms) Destroy(a.gameObject);

            if (_current) _current.Dismiss();
            var cam = Camera.main.transform;
            var toCam = cam.position - target; toCam.y = 0f;
            var rot = toCam.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(-toCam) : Quaternion.identity;
            _current = moleculeFactory.Create(compound, target, rot);

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            CompoundFormed?.Invoke(compound, _current);

            _signature = "";
            _busy = false;
        }

        // ---------- Visual ----------

        void BuildRing()
        {
            const int segments = 72;
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = segments;
            ring.widthMultiplier = 0.006f;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
        }

        void UpdateRingColor()
        {
            if (!ring) return;
            Color c;
            if (_flash > 0f)
            {
                _flash -= Time.deltaTime * 1.2f;
                c = Color.Lerp(idleColor, failColor, Mathf.PingPong(_flash * 6f, 1f));
            }
            else if (_inside.Count > 0)
            {
                // Pulse faster as the zone gets ready to check.
                float p = 0.5f + 0.5f * Mathf.Sin(Time.time * (4f + _stableFor * 6f));
                c = Color.Lerp(idleColor, readyColor, p);
            }
            else c = idleColor;
            ring.startColor = ring.endColor = c;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
