using Periodiq.AR;
using Periodiq.Atoms;
using Periodiq.Table;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Periodiq.Interaction
{
    /// <summary>
    /// Touch a tile → an atom pops out and follows your finger.
    /// Touch an existing atom → pick it up again. Lift finger → it stays floating there.
    /// Dragging keeps the atom at the same distance from the camera it was grabbed at;
    /// a second finger pinch pushes it nearer/farther.
    /// </summary>
    public class GrabController : MonoBehaviour
    {
        [SerializeField] ARTablePlacer placer;
        [SerializeField] AtomFactory factory;
        [Tooltip("How far in front of the tile the atom appears (metres).")]
        [SerializeField] float popOutDistance = 0.08f;
        [SerializeField] float followSmoothing = 18f;
        [SerializeField] float minDistance = 0.15f;
        [SerializeField] float maxDistance = 2.0f;
        [SerializeField] int maxAtoms = 12;

        Camera _cam;
        Atom _held;
        float _holdDistance;
        int _fingerId = -1;
        float _lastPinch;

        void Awake() => _cam = Camera.main;
        void OnEnable() => EnhancedTouchSupport.Enable();
        void OnDisable() => EnhancedTouchSupport.Disable();

        void Update()
        {
            if (placer && (!placer.IsPlaced || Time.frameCount <= placer.PlacedFrame + 1)) return;

            var touches = Touch.activeTouches;
            if (touches.Count == 0)
            {
                if (_held) Drop();
                return;
            }

            if (_held == null)
            {
                var t = touches[0];
                if (t.phase == TouchPhase.Began) TryGrab(t);
                return;
            }

            // Find our finger.
            Touch? mine = null;
            foreach (var t in touches)
                if (t.touchId == _fingerId) { mine = t; break; }

            if (mine == null || mine.Value.phase == TouchPhase.Ended || mine.Value.phase == TouchPhase.Canceled)
            {
                Drop();
                return;
            }

            // Two fingers → pinch adjusts depth.
            if (touches.Count >= 2)
            {
                float pinch = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
                if (_lastPinch > 0f)
                    _holdDistance = Mathf.Clamp(_holdDistance * (pinch / _lastPinch), minDistance, maxDistance);
                _lastPinch = pinch;
            }
            else _lastPinch = 0f;

            var ray = _cam.ScreenPointToRay(mine.Value.screenPosition);
            var target = ray.GetPoint(_holdDistance);
            var tr = _held.transform;
            tr.position = Vector3.Lerp(tr.position, target, 1f - Mathf.Exp(-followSmoothing * Time.deltaTime));
            // Face the camera so the shells read nicely.
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(tr.position - _cam.transform.position), 0.1f);
        }

        void TryGrab(Touch t)
        {
            if (IsOverUI(t.screenPosition)) return;

            var ray = _cam.ScreenPointToRay(t.screenPosition);
            if (!Physics.Raycast(ray, out var hit, 10f)) return;

            // Tappable objects (e.g. the closed Discovery Book).
            var tappable = hit.collider.GetComponentInParent<ITappable>();
            if (tappable != null)
            {
                tappable.OnTap();
                return;
            }

            // Existing atom?
            var atom = hit.collider.GetComponentInParent<Atom>();
            if (atom != null)
            {
                if (atom.Locked) return;
                Hold(atom, t, Vector3.Distance(_cam.transform.position, atom.transform.position));
                return;
            }

            // Tile → spawn a new atom just in front of it.
            var tile = hit.collider.GetComponentInParent<ElementTile>();
            if (tile == null || tile.Data == null) return;
            if (FindObjectsByType<Atom>(FindObjectsSortMode.None).Length >= maxAtoms) return;

            var toCam = (_cam.transform.position - tile.transform.position).normalized;
            var spawnPos = tile.transform.position + toCam * popOutDistance;
            var newAtom = factory.Create(tile.Data, spawnPos, Quaternion.LookRotation(-toCam));
            Hold(newAtom, t, Vector3.Distance(_cam.transform.position, spawnPos));

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        static readonly List<RaycastResult> UiHits = new();

        /// <summary>True if the touch is on a UI element (e.g. the info card), so we don't grab through it.</summary>
        static bool IsOverUI(Vector2 screenPos)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            var data = new PointerEventData(es) { position = screenPos };
            UiHits.Clear();
            es.RaycastAll(data, UiHits);
            return UiHits.Count > 0;
        }

        void Hold(Atom atom, Touch t, float distance)
        {
            _held = atom;
            _held.IsHeld = true;
            _fingerId = t.touchId;
            _holdDistance = Mathf.Clamp(distance, minDistance, maxDistance);
            _lastPinch = 0f;
        }

        void Drop()
        {
            _held.Release();
            _held = null;
            _fingerId = -1;
        }
    }
}
