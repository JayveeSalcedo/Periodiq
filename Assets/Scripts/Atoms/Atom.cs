using Periodiq.Data;
using UnityEngine;

namespace Periodiq.Atoms
{
    /// <summary>
    /// A grabbed atom floating in AR. Root has a SphereCollider so it can be picked up again.
    /// Feature 3 (reaction zone) reads <see cref="Element"/> from this.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Atom : MonoBehaviour
    {
        public ElementData Element { get; private set; }
        bool _isHeld;
        public bool IsHeld
        {
            get => _isHeld;
            set { _isHeld = value; if (value) _resting = false; }
        }

        Vector3 _restPosition;
        bool _resting;
        float _bobPhase;
        float _popT;
        Vector3 _targetScale;

        const float PopDuration = 0.35f;
        const float BobHeight = 0.004f;
        const float BobSpeed = 1.6f;

        public void Init(ElementData element, float radius)
        {
            Element = element;
            name = $"Atom_{element.symbol}";
            GetComponent<SphereCollider>().radius = 1f;
            _targetScale = Vector3.one * radius;
            transform.localScale = Vector3.zero;
            _popT = 0f;
            _bobPhase = Random.value * Mathf.PI * 2f;
        }

        public void Release()
        {
            IsHeld = false;
            _restPosition = transform.position;
            _resting = true;
        }

        void Update()
        {
            // Pop-out: overshoot ease (back-out) from 0 to full size.
            if (_popT < 1f)
            {
                _popT = Mathf.Min(1f, _popT + Time.deltaTime / PopDuration);
                transform.localScale = _targetScale * BackOut(_popT);
            }

            // Gentle hover when resting.
            if (!IsHeld && _resting)
            {
                _bobPhase += Time.deltaTime * BobSpeed;
                transform.position = _restPosition + Vector3.up * Mathf.Sin(_bobPhase) * BobHeight;
            }
        }

        static float BackOut(float t)
        {
            const float s = 1.70158f;
            t -= 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }
    }
}
