using System.Collections.Generic;
using Periodiq.Data;
using UnityEngine;

namespace Periodiq.Atoms
{
    /// <summary>
    /// Builds a Bohr-model atom procedurally from element data:
    /// a packed nucleus of protons + neutrons, one ring per shell, electrons orbiting each ring.
    /// Everything is built in a unit space (outer shell radius = 1) and the root is scaled.
    /// </summary>
    public class AtomFactory : MonoBehaviour
    {
        [Header("Materials (any URP/Lit or Unlit materials)")]
        [SerializeField] Material protonMaterial;    // red
        [SerializeField] Material neutronMaterial;   // grey
        [SerializeField] Material electronMaterial;  // cyan, emissive looks great
        [SerializeField] Material shellMaterial;     // for LineRenderer, e.g. URP/Unlit, semi-transparent white

        [Header("Size")]
        [Tooltip("World radius of the whole atom in metres.")]
        [SerializeField] float atomRadius = 0.06f;
        [Tooltip("Cap on nucleons drawn so heavy atoms stay fast on low-end phones.")]
        [SerializeField] int maxVisibleNucleons = 40;

        static Mesh _sphereMesh;

        public Atom Create(ElementData e, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject($"Atom_{e.symbol}");
            root.transform.SetPositionAndRotation(position, rotation);

            BuildNucleus(e, root.transform);
            BuildShells(e, root.transform);

            var atom = root.AddComponent<Atom>(); // adds SphereCollider via RequireComponent
            atom.Init(e, atomRadius);
            return atom;
        }

        // ---------- Nucleus ----------

        void BuildNucleus(ElementData e, Transform parent)
        {
            var nucleus = new GameObject("Nucleus").transform;
            nucleus.SetParent(parent, false);

            int protons = e.number;
            int neutrons = Mathf.Max(0, e.neutrons);
            int total = protons + neutrons;
            int visible = Mathf.Min(total, maxVisibleNucleons);

            // Keep the proton:neutron ratio when capping.
            int visibleProtons = Mathf.Clamp(Mathf.RoundToInt(visible * (protons / (float)total)), 1, visible);

            // Nucleus grows slowly with mass; stays well inside the first shell.
            float nucleusRadius = Mathf.Lerp(0.07f, 0.2f, Mathf.InverseLerp(1, 60, visible));
            float ballRadius = nucleusRadius * (visible == 1 ? 1f : 0.55f / Mathf.Pow(visible, 1f / 3f) * 1.6f);

            // Shuffle so protons and neutrons mix.
            var kinds = new List<bool>(visible);
            for (int i = 0; i < visible; i++) kinds.Add(i < visibleProtons);
            var rng = new System.Random(e.number);
            for (int i = kinds.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (kinds[i], kinds[j]) = (kinds[j], kinds[i]);
            }

            for (int i = 0; i < visible; i++)
            {
                Vector3 p = visible == 1 ? Vector3.zero : FibonacciPoint(i, visible) * nucleusRadius * Mathf.Pow((i + 1f) / visible, 1f / 3f);
                var ball = MakeSphere(kinds[i] ? protonMaterial : neutronMaterial, nucleus);
                ball.localPosition = p;
                ball.localScale = Vector3.one * ballRadius * 2f;
            }

            nucleus.gameObject.AddComponent<Spinner>().Init(Vector3.up, 20f);
        }

        // ---------- Shells + electrons ----------

        void BuildShells(ElementData e, Transform parent)
        {
            var shells = e.shells;
            if (shells == null || shells.Length == 0) return;

            int n = shells.Length;
            for (int s = 0; s < n; s++)
            {
                float r = Mathf.Lerp(0.35f, 1f, n == 1 ? 1f : s / (float)(n - 1));

                // Each shell gets its own tilt so the atom reads as 3D, not a flat target.
                var shell = new GameObject($"Shell{s + 1}").transform;
                shell.SetParent(parent, false);
                shell.localRotation = Quaternion.Euler(70f + s * 17f, s * 47f, 0f);

                DrawRing(shell, r);

                int electrons = shells[s];
                for (int k = 0; k < electrons; k++)
                {
                    float a = k * Mathf.PI * 2f / electrons;
                    var el = MakeSphere(electronMaterial, shell);
                    el.localPosition = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
                    el.localScale = Vector3.one * 0.06f;
                }

                // Inner shells orbit faster.
                shell.gameObject.AddComponent<Spinner>().Init(Vector3.forward, 90f / (s + 1));
            }
        }

        void DrawRing(Transform parent, float radius)
        {
            const int segments = 64;
            var lr = new GameObject("Ring").AddComponent<LineRenderer>();
            lr.transform.SetParent(parent, false);
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = segments;
            lr.widthMultiplier = 0.004f; // world units; stays thin regardless of atom scale
            lr.sharedMaterial = shellMaterial;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            }
        }

        // ---------- Helpers ----------

        static Transform MakeSphere(Material mat, Transform parent)
        {
            if (_sphereMesh == null)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _sphereMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(tmp);
            }
            var go = new GameObject("Ball");
            go.AddComponent<MeshFilter>().sharedMesh = _sphereMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static Vector3 FibonacciPoint(int i, int count)
        {
            float y = 1f - (i + 0.5f) / count * 2f;
            float r = Mathf.Sqrt(1f - y * y);
            float phi = i * Mathf.PI * (3f - Mathf.Sqrt(5f));
            return new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
        }
    }

    /// <summary>Constant local rotation; used for nucleus wobble and electron orbits.</summary>
    public class Spinner : MonoBehaviour
    {
        Vector3 _axis;
        float _speed;

        public void Init(Vector3 axis, float degreesPerSecond)
        {
            _axis = axis;
            _speed = degreesPerSecond;
        }

        void Update() => transform.Rotate(_axis, _speed * Time.deltaTime, Space.Self);
    }
}
