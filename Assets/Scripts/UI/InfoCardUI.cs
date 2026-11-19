using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Periodiq.UI
{
    /// <summary>
    /// Bottom-sheet card built entirely in code (no prefab needed).
    /// Show() slides it up; tapping ✕ or calling Hide() slides it down.
    /// Needs an EventSystem in the scene for the close button.
    /// </summary>
    public class InfoCardUI : MonoBehaviour
    {
        [SerializeField] Color cardColor = new(0.11f, 0.12f, 0.15f, 0.94f);
        [SerializeField] float slideSeconds = 0.25f;

        RectTransform _card;
        Image _accentBar;
        TMP_Text _title, _formula, _chip, _body;
        Image _chipBg;
        Coroutine _anim;
        bool _visible;

        void Awake() => Build();

        public void Show(string title, string formula, string chip, Color accent, string body)
        {
            _title.text = title;
            _formula.text = formula;
            _formula.gameObject.SetActive(!string.IsNullOrEmpty(formula));
            _chip.text = chip;
            _chipBg.gameObject.SetActive(!string.IsNullOrEmpty(chip));
            _chipBg.color = accent;
            _accentBar.color = accent;
            _body.text = body;
            Slide(true);
        }

        public void Hide() => Slide(false);

        // ---------- Animation ----------

        void Slide(bool show)
        {
            if (_anim != null) StopCoroutine(_anim);
            _visible = show;
            _anim = StartCoroutine(SlideRoutine(show));
        }

        IEnumerator SlideRoutine(bool show)
        {
            if (show) _card.gameObject.SetActive(true);
            float hiddenY = -_card.rect.height - 40f;
            float from = _card.anchoredPosition.y;
            float to = show ? 24f : hiddenY;
            for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / slideSeconds)
            {
                float e = 1f - Mathf.Pow(1f - t, 3f);
                _card.anchoredPosition = new Vector2(0f, Mathf.Lerp(from, to, e));
                yield return null;
            }
            _card.anchoredPosition = new Vector2(0f, to);
            if (!show) _card.gameObject.SetActive(false);
        }

        // ---------- Build UI ----------

        void Build()
        {
            var canvasGo = new GameObject("InfoCardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Card: anchored bottom, full width with margins, height fits content.
            _card = NewRect("Card", canvasGo.transform);
            _card.anchorMin = new Vector2(0f, 0f);
            _card.anchorMax = new Vector2(1f, 0f);
            _card.pivot = new Vector2(0.5f, 0f);
            _card.offsetMin = new Vector2(32f, 0f);
            _card.offsetMax = new Vector2(-32f, 0f);
            _card.gameObject.AddComponent<Image>().color = cardColor;
            var layout = _card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 44, 52);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            _card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Accent bar on the left edge (ignores layout).
            var bar = NewRect("Accent", _card);
            bar.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(0f, 1f);
            bar.pivot = new Vector2(0f, 0.5f);
            bar.sizeDelta = new Vector2(12f, 0f);
            bar.anchoredPosition = Vector2.zero;
            _accentBar = bar.gameObject.AddComponent<Image>();

            // Header row: title + chip.
            var header = NewRect("Header", _card);
            var h = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 20f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            _title = NewText("Title", header, 56, FontStyles.Bold, Color.white);
            _title.GetComponent<LayoutElement>().flexibleWidth = 1f;

            _chipBg = NewRect("Chip", header).gameObject.AddComponent<Image>();
            var chipLayout = _chipBg.gameObject.AddComponent<HorizontalLayoutGroup>();
            chipLayout.padding = new RectOffset(22, 22, 8, 8);
            chipLayout.childControlWidth = chipLayout.childControlHeight = true;
            _chip = NewText("ChipText", _chipBg.transform, 30, FontStyles.Bold, new Color(0.08f, 0.09f, 0.1f));

            // Close button.
            var close = NewRect("Close", header);
            close.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
            var closeLe = close.gameObject.AddComponent<LayoutElement>();
            closeLe.preferredWidth = closeLe.preferredHeight = 84f;
            var btn = close.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(Hide);
            var x = NewText("X", close, 40, FontStyles.Bold, Color.white);
            x.text = "X";
            x.alignment = TextAlignmentOptions.Center;
            var xr = x.rectTransform;
            xr.anchorMin = Vector2.zero; xr.anchorMax = Vector2.one; xr.offsetMin = xr.offsetMax = Vector2.zero;
            x.GetComponent<LayoutElement>().ignoreLayout = true;

            _formula = NewText("Formula", _card, 88, FontStyles.Bold, new Color(0.62f, 0.9f, 1f));
            _body = NewText("Body", _card, 36, FontStyles.Normal, new Color(0.88f, 0.9f, 0.93f));
            _body.lineSpacing = 8f;

            _card.gameObject.SetActive(false);
            _card.anchoredPosition = new Vector2(0f, -2000f);
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static TMP_Text NewText(string name, Transform parent, float size, FontStyles style, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.richText = true;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            rt.gameObject.AddComponent<LayoutElement>();
            return t;
        }
    }
}
