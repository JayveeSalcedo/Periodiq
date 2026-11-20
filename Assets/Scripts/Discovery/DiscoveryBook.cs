using System.Collections;
using System.Collections.Generic;
using Periodiq.Interaction;
using Periodiq.Reactions;
using Periodiq.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Periodiq.Discovery
{
    /// <summary>
    /// A 3D book standing beside the periodic table.
    /// Closed: a book you can tap (shows progress on the cover).
    /// Open: a two-page spread (world-space UI) with every compound — found ones in colour,
    /// the rest greyed out — plus a Badges tab. Tap a found compound to put its molecule
    /// in the reaction zone. Everything is built in code; put this on an empty child of
    /// the PeriodicTable (e.g. local position (0.42, 0, 0)).
    /// </summary>
    public class DiscoveryBook : MonoBehaviour, ITappable
    {
        [SerializeField] DiscoveryStore store;
        [SerializeField] ReactionZone zone;
        [Tooltip("Any URP/Lit material; tinted in code.")]
        [SerializeField] Material coverMaterial;
        [SerializeField] Color coverColor = new(0.16f, 0.22f, 0.45f);

        [Header("Open book placement (relative to this object)")]
        [SerializeField] Vector3 pagesOffset = new(0f, 0.14f, -0.06f);
        [SerializeField] float pagesYaw = -18f;      // turn toward the table centre
        [SerializeField] float pagesTilt = 12f;      // lean back for easier reading
        [SerializeField] float pagesWidthMetres = 0.38f;

        const float CanvasW = 1400f, CanvasH = 900f;
        const int PerSide = 6, PerPage = PerSide * 2;

        // Theme
        static readonly Color Paper = new(0.97f, 0.95f, 0.9f);
        static readonly Color PaperRight = new(0.95f, 0.93f, 0.87f);
        static readonly Color Ink = new(0.15f, 0.16f, 0.2f);
        static readonly Color Faded = new(0.55f, 0.56f, 0.6f);
        static readonly Color Locked = new(0.86f, 0.86f, 0.86f);
        static readonly Color Covalent = new(0.62f, 0.86f, 1f);
        static readonly Color Ionic = new(0.82f, 0.72f, 1f);
        static readonly Color Gold = new(1f, 0.85f, 0.35f);
        static readonly Color Accent = new(0.16f, 0.22f, 0.45f);

        enum Tab { Compounds, Badges }

        GameObject _cover;
        TMP_Text _coverProgress;
        RectTransform _pages;
        Transform _leftGrid, _rightGrid;
        TMP_Text _progressText, _pageLabel;
        RectTransform _progressFill;
        Image _tabCompounds, _tabBadges;
        Tab _tab = Tab.Compounds;
        int _page;
        bool _open;
        Coroutine _anim;

        void Awake()
        {
            BuildCover();
            BuildPages();
        }

        void OnEnable() { if (store) store.Changed += Refresh; }
        void OnDisable() { if (store) store.Changed -= Refresh; }
        void Start() => Refresh();

        // Closed book tapped.
        public void OnTap()
        {
            if (!_open) SetOpen(true);
        }

        public void SetOpen(bool open)
        {
            _open = open;
            if (open) Refresh();
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(Animate(open));
        }

        IEnumerator Animate(bool open)
        {
            float full = pagesWidthMetres / CanvasW;
            _pages.gameObject.SetActive(true);
            _cover.SetActive(true);
            float from = _pages.localScale.x, to = open ? full : 0f;
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.3f)
            {
                float e = 1f - Mathf.Pow(1f - t, 3f);
                _pages.localScale = Vector3.one * Mathf.Lerp(from, to, e);
                _cover.transform.localScale = Vector3.one * Mathf.Lerp(open ? 1f : 0f, open ? 0f : 1f, e);
                yield return null;
            }
            _pages.localScale = Vector3.one * to;
            _pages.gameObject.SetActive(open);
            _cover.SetActive(!open);
            _cover.transform.localScale = Vector3.one;
        }

        // ---------- Refresh ----------

        void Refresh()
        {
            if (!store) return;
            int found = store.FoundCount, total = store.Total;
            _coverProgress.text = $"{found} / {total}";
            _progressText.text = $"{found} / {total} found";
            _progressFill.anchorMax = new Vector2(total == 0 ? 0f : found / (float)total, 1f);

            _tabCompounds.color = _tab == Tab.Compounds ? Accent : Faded;
            _tabBadges.color = _tab == Tab.Badges ? Accent : Faded;

            Clear(_leftGrid);
            Clear(_rightGrid);

            int count = _tab == Tab.Compounds ? CompoundDatabase.All.Count : Badges.All.Count;
            int pages = Mathf.Max(1, Mathf.CeilToInt(count / (float)PerPage));
            _page = Mathf.Clamp(_page, 0, pages - 1);
            _pageLabel.text = $"Page {_page + 1} of {pages}";

            for (int i = _page * PerPage; i < Mathf.Min(count, (_page + 1) * PerPage); i++)
            {
                var parent = (i - _page * PerPage) < PerSide ? _leftGrid : _rightGrid;
                if (_tab == Tab.Compounds) CompoundCell(CompoundDatabase.All[i], parent);
                else BadgeCell(Badges.All[i], parent);
            }
        }

        void CompoundCell(CompoundData c, Transform parent)
        {
            bool found = store.IsFound(c.id);
            var bg = found ? (c.IsIonic ? Ionic : Covalent) : Locked;
            var btn = UIBuild.Button(c.id, parent, bg, () => OnCompoundTapped(c));
            btn.interactable = found;
            var colors = btn.colors; colors.disabledColor = Color.white; btn.colors = colors; // keep our grey

            var ink = found ? Ink : Faded;
            UIBuild.Anchor(UIBuild.Text("Formula", btn.transform, ChemistryExplainer.FormatFormula(c.formula), 54, ink, FontStyles.Bold),
                0.05f, 0.45f, 0.95f, 0.95f);
            UIBuild.Anchor(UIBuild.Text("Name", btn.transform, c.name, 28, ink), 0.05f, 0.22f, 0.95f, 0.47f);
            UIBuild.Anchor(UIBuild.Text("Status", btn.transform, found ? "Tap to view" : "Not made yet", 22, found ? Accent : Faded,
                FontStyles.Italic), 0.05f, 0.04f, 0.95f, 0.24f);
        }

        void BadgeCell(Badge b, Transform parent)
        {
            bool earned = store.HasBadge(b.id);
            var bg = UIBuild.Image(b.id, parent, earned ? Gold : Locked);
            var ink = earned ? Ink : Faded;
            UIBuild.Anchor(UIBuild.Text("Title", bg.transform, b.title, 34, ink, FontStyles.Bold),
                0.06f, 0.55f, 0.94f, 0.95f);
            UIBuild.Anchor(UIBuild.Text("Desc", bg.transform, b.description, 24, ink), 0.06f, 0.06f, 0.94f, 0.55f);
        }

        void OnCompoundTapped(CompoundData c)
        {
            if (!store.IsFound(c.id)) return;
            zone.ShowMolecule(c);
            SetOpen(false); // get out of the way so they can see it
        }

        static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
        }

        // ---------- Build: closed cover ----------

        void BuildCover()
        {
            _cover = new GameObject("Cover");
            _cover.transform.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube); // keeps its BoxCollider for tapping
            body.name = "Body";
            body.transform.SetParent(_cover.transform, false);
            body.transform.localScale = new Vector3(0.12f, 0.16f, 0.03f);
            body.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            var mr = body.GetComponent<MeshRenderer>();
            if (coverMaterial) mr.sharedMaterial = coverMaterial;
            var mpb = new MaterialPropertyBlock();
            mpb.SetColor("_BaseColor", coverColor);
            mpb.SetColor("_Color", coverColor);
            mr.SetPropertyBlock(mpb);

            // Text on the front face (-Z faces the user).
            CoverText("Title", "Discovery\nBook", 0.14f, new Vector3(0f, 0.11f, -0.016f), FontStyles.Bold);
            _coverProgress = CoverText("Progress", "0 / 0", 0.1f, new Vector3(0f, 0.05f, -0.016f), FontStyles.Normal);
            CoverText("Hint", "tap to open", 0.06f, new Vector3(0f, 0.025f, -0.016f), FontStyles.Italic);
        }

        TMP_Text CoverText(string name, string text, float size, Vector3 pos, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_cover.transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = TextAlignmentOptions.Center;
            t.color = new Color(1f, 0.92f, 0.7f);
            t.rectTransform.sizeDelta = new Vector2(0.11f, 0.06f);
            return t;
        }

        // ---------- Build: open pages ----------

        void BuildPages()
        {
            var go = new GameObject("Pages", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            _pages = (RectTransform)go.transform;
            _pages.sizeDelta = new Vector2(CanvasW, CanvasH);
            _pages.localPosition = pagesOffset;
            _pages.localRotation = Quaternion.Euler(-pagesTilt, pagesYaw, 0f);
            _pages.localScale = Vector3.zero;
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            go.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 4f;

            // Pages + spine
            UIBuild.Anchor(UIBuild.Image("LeftPage", _pages, Paper).rectTransform, 0f, 0f, 0.5f, 1f);
            UIBuild.Anchor(UIBuild.Image("RightPage", _pages, PaperRight).rectTransform, 0.5f, 0f, 1f, 1f);
            var spine = UIBuild.Image("Spine", _pages, new Color(0f, 0f, 0f, 0.18f)).rectTransform;
            spine.anchorMin = new Vector2(0.5f, 0f); spine.anchorMax = new Vector2(0.5f, 1f);
            spine.sizeDelta = new Vector2(8f, 0f);

            // Left header: title + progress
            UIBuild.Anchor(UIBuild.Text("Title", _pages, "Discovery Book", 56, Ink, FontStyles.Bold, TextAlignmentOptions.Left),
                0.03f, 0.89f, 0.47f, 0.98f);
            var barBg = UIBuild.Image("ProgressBg", _pages, new Color(0f, 0f, 0f, 0.1f)).rectTransform;
            UIBuild.Anchor(barBg, 0.03f, 0.845f, 0.32f, 0.875f);
            _progressFill = UIBuild.Image("Fill", barBg, Accent).rectTransform;
            _progressFill.anchorMin = Vector2.zero; _progressFill.anchorMax = new Vector2(0f, 1f);
            _progressFill.offsetMin = _progressFill.offsetMax = Vector2.zero;
            _progressText = UIBuild.Text("ProgressText", _pages, "", 28, Faded, FontStyles.Normal, TextAlignmentOptions.Left);
            UIBuild.Anchor(_progressText.rectTransform, 0.335f, 0.83f, 0.49f, 0.89f);

            // Right header: tabs + close
            _tabCompounds = UIBuild.Button("TabCompounds", _pages, Accent, () => { _tab = Tab.Compounds; _page = 0; Refresh(); }).image;
            UIBuild.Anchor(_tabCompounds.rectTransform, 0.53f, 0.86f, 0.71f, 0.96f);
            UIBuild.Stretch(UIBuild.Text("Label", _tabCompounds.transform, "Compounds", 30, Color.white, FontStyles.Bold).rectTransform);

            _tabBadges = UIBuild.Button("TabBadges", _pages, Faded, () => { _tab = Tab.Badges; _page = 0; Refresh(); }).image;
            UIBuild.Anchor(_tabBadges.rectTransform, 0.72f, 0.86f, 0.86f, 0.96f);
            UIBuild.Stretch(UIBuild.Text("Label", _tabBadges.transform, "Badges", 30, Color.white, FontStyles.Bold).rectTransform);

            var close = UIBuild.Button("Close", _pages, new Color(0f, 0f, 0f, 0.08f), () => SetOpen(false)).image;
            UIBuild.Anchor(close.rectTransform, 0.91f, 0.86f, 0.97f, 0.96f);
            UIBuild.Stretch(UIBuild.Text("X", close.transform, "X", 36, Ink, FontStyles.Bold).rectTransform);

            // Grids
            _leftGrid = MakeGrid("LeftGrid", 0.03f, 0.47f);
            _rightGrid = MakeGrid("RightGrid", 0.53f, 0.97f);

            // Footer: prev / page / next
            var prev = UIBuild.Button("Prev", _pages, new Color(0f, 0f, 0f, 0.06f), () => { _page--; Refresh(); }).image;
            UIBuild.Anchor(prev.rectTransform, 0.03f, 0.02f, 0.16f, 0.09f);
            UIBuild.Stretch(UIBuild.Text("L", prev.transform, "< Prev", 28, Ink, FontStyles.Bold).rectTransform);

            _pageLabel = UIBuild.Text("PageLabel", _pages, "", 26, Faded);
            UIBuild.Anchor(_pageLabel.rectTransform, 0.30f, 0.02f, 0.47f, 0.09f); // left of the spine

            var next = UIBuild.Button("Next", _pages, new Color(0f, 0f, 0f, 0.06f), () => { _page++; Refresh(); }).image;
            UIBuild.Anchor(next.rectTransform, 0.84f, 0.02f, 0.97f, 0.09f);
            UIBuild.Stretch(UIBuild.Text("R", next.transform, "Next >", 28, Ink, FontStyles.Bold).rectTransform);

            go.SetActive(false);
        }

        Transform MakeGrid(string name, float xMin, float xMax)
        {
            var rt = UIBuild.Rect(name, _pages);
            UIBuild.Anchor(rt, xMin, 0.11f, xMax, 0.81f);
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            // 2 columns × 3 rows inside ~616 × 630 px.
            g.cellSize = new Vector2(298f, 196f);
            g.spacing = new Vector2(20f, 16f);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 2;
            g.childAlignment = TextAnchor.UpperCenter;
            return rt;
        }
    }
}
