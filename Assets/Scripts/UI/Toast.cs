using System.Collections;
using System.Collections.Generic;
using Periodiq.Discovery;
using Periodiq.Reactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Periodiq.UI
{
    /// <summary>
    /// Small banners that drop in from the top: "New discovery!" and "Badge unlocked!".
    /// Queued so several in a row don't overlap. Built in code.
    /// </summary>
    public class Toast : MonoBehaviour
    {
        [SerializeField] DiscoveryStore store;
        [SerializeField] float showSeconds = 2.4f;

        readonly Queue<(string title, string body, Color color)> _queue = new();
        RectTransform _panel;
        Image _bg;
        TMP_Text _title, _body;
        bool _running;

        void Awake() => Build();

        void OnEnable()
        {
            if (!store) return;
            store.NewDiscovery += OnDiscovery;
            store.BadgeEarned += OnBadge;
        }

        void OnDisable()
        {
            if (!store) return;
            store.NewDiscovery -= OnDiscovery;
            store.BadgeEarned -= OnBadge;
        }

        void OnDiscovery(CompoundData c) =>
            Show("New discovery!", $"{c.name} added to your Discovery Book", new Color(0.3f, 0.75f, 1f));

        void OnBadge(Badge b) =>
            Show($"Badge unlocked: {b.title}", b.description, new Color(1f, 0.82f, 0.3f));

        public void Show(string title, string body, Color color)
        {
            _queue.Enqueue((title, body, color));
            if (!_running) StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            _running = true;
            while (_queue.Count > 0)
            {
                var (title, body, color) = _queue.Dequeue();
                _title.text = title;
                _body.text = body;
                _bg.color = color;
                _panel.gameObject.SetActive(true);
                yield return Slide(200f, -60f);
                yield return new WaitForSeconds(showSeconds);
                yield return Slide(-60f, 200f);
                _panel.gameObject.SetActive(false);
            }
            _running = false;
        }

        IEnumerator Slide(float from, float to)
        {
            for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / 0.25f)
            {
                float e = 1f - Mathf.Pow(1f - t, 3f);
                _panel.anchoredPosition = new Vector2(0f, Mathf.Lerp(from, to, e));
                yield return null;
            }
            _panel.anchoredPosition = new Vector2(0f, to);
        }

        void Build()
        {
            var canvasGo = new GameObject("ToastCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            _panel = UIBuild.Rect("Toast", canvasGo.transform);
            _panel.anchorMin = new Vector2(0.06f, 1f);
            _panel.anchorMax = new Vector2(0.94f, 1f);
            _panel.pivot = new Vector2(0.5f, 1f);
            _panel.sizeDelta = new Vector2(0f, 170f);
            _panel.anchoredPosition = new Vector2(0f, 200f);
            _bg = _panel.gameObject.AddComponent<Image>();

            var dark = new Color(0.08f, 0.09f, 0.1f);
            _title = UIBuild.Text("Title", _panel, "", 44, dark, FontStyles.Bold, TextAlignmentOptions.Left);
            UIBuild.Anchor(_title.rectTransform, 0.05f, 0.5f, 0.95f, 0.92f);
            _body = UIBuild.Text("Body", _panel, "", 32, dark, FontStyles.Normal, TextAlignmentOptions.Left);
            UIBuild.Anchor(_body.rectTransform, 0.05f, 0.08f, 0.95f, 0.5f);

            // Never block touches.
            _bg.raycastTarget = false;
            _panel.gameObject.SetActive(false);
        }
    }
}
