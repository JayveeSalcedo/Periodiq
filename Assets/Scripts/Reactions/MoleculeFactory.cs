using UnityEngine;

namespace Periodiq.Reactions
{
    /// <summary>
    /// Builds a ball-and-stick molecule from compound data.
    /// Covalent: sticks (1-3 parallel for single/double/triple bonds).
    /// Ionic: no sticks; ions get a soft glow halo to show they're held by charge.
    /// </summary>
    public class MoleculeFactory : MonoBehaviour
    {
        [Tooltip("Base material for atom balls; colour is set per element (CPK colours).")]
        [SerializeField] Material atomMaterial;
        [SerializeField] Material bondMaterial;
        [Tooltip("Transparent material for the ionic halo.")]
        [SerializeField] Material ionHaloMaterial;
        [Tooltip("Metres per ångström.")]
        [SerializeField] float scale = 0.04f;

        static Mesh _sphere, _cylinder;
        static MaterialPropertyBlock _mpb;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        public Molecule Create(CompoundData c, Vector3 position, Quaternion rotation)
        {
            EnsureMeshes();
            var root = new GameObject($"Molecule_{c.id}");
            root.transform.SetPositionAndRotation(position, rotation);
            var body = new GameObject("Body").transform;
            body.SetParent(root.transform, false);

            // Atoms
            for (int i = 0; i < c.atoms.Length; i++)
            {
                var a = c.atoms[i];
                float r = CpkColors.DisplayRadius(a.symbol) * scale;
                var ball = MakeMesh(_sphere, atomMaterial, body, $"{a.symbol}{i}");
                ball.localPosition = a.position * scale;
                ball.localScale = Vector3.one * r * 2f;
                Tint(ball, CpkColors.For(a.symbol));

                if (c.IsIonic && ionHaloMaterial)
                {
                    var halo = MakeMesh(_sphere, ionHaloMaterial, body, "Halo");
                    halo.localPosition = ball.localPosition;
                    halo.localScale = ball.localScale * 1.35f;
                }
            }

            // Bonds
            if (c.bonds != null)
            {
                foreach (var b in c.bonds)
                {
                    if (b.order <= 0) continue; // ionic → no stick
                    var p0 = c.atoms[b.a].position * scale;
                    var p1 = c.atoms[b.b].position * scale;
                    AddBond(body, p0, p1, b.order);
                }
            }

            var mol = root.AddComponent<Molecule>();
            mol.Init(c);
            return mol;
        }

        void AddBond(Transform parent, Vector3 p0, Vector3 p1, int order)
        {
            var dir = p1 - p0;
            float len = dir.magnitude;
            var mid = (p0 + p1) * 0.5f;
            var rot = Quaternion.FromToRotation(Vector3.up, dir.normalized);

            // Offset direction perpendicular to the bond for double/triple sticks.
            var side = Vector3.Cross(dir.normalized, Vector3.forward);
            if (side.sqrMagnitude < 0.01f) side = Vector3.Cross(dir.normalized, Vector3.right);
            side.Normalize();

            float radius = 0.08f * scale;
            float spacing = radius * 2.6f;
            for (int i = 0; i < order; i++)
            {
                float offset = (i - (order - 1) / 2f) * spacing;
                var stick = MakeMesh(_cylinder, bondMaterial, parent, "Bond");
                stick.localPosition = mid + side * offset;
                stick.localRotation = rot;
                // Unity cylinder is 2 units tall, 1 unit wide.
                stick.localScale = new Vector3(radius * 2f, len / 2f, radius * 2f);
            }
        }

        static Transform MakeMesh(Mesh mesh, Material mat, Transform parent, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void Tint(Transform t, Color c)
        {
            _mpb ??= new MaterialPropertyBlock();
            var r = t.GetComponent<MeshRenderer>();
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColor, c);
            _mpb.SetColor(ColorId, c);
            r.SetPropertyBlock(_mpb);
        }

        static void EnsureMeshes()
        {
            if (_sphere != null) return;
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _sphere = s.GetComponent<MeshFilter>().sharedMesh;
            _cylinder = c.GetComponent<MeshFilter>().sharedMesh;
            Destroy(s);
            Destroy(c);
        }
    }
}
