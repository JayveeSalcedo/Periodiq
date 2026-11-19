using UnityEngine;

namespace Periodiq.Reactions
{
    /// <summary>A formed molecule: pops in, floats and slowly turns. Feature 4 shows its info card.</summary>
    public class Molecule : MonoBehaviour
    {
        public CompoundData Compound { get; private set; }

        float _t;
        Vector3 _basePos;
        bool _leaving;

        public void Init(CompoundData c)
        {
            Compound = c;
            _basePos = transform.position;
            transform.localScale = Vector3.zero;
        }

        /// <summary>Shrink away and destroy (used when a newer molecule replaces this one).</summary>
        public void Dismiss() => _leaving = true;

        void Update()
        {
            _t += Time.deltaTime;

            if (_leaving)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 4f);
                if (transform.localScale.x <= 0.001f) Destroy(gameObject);
                return;
            }

            float pop = Mathf.Clamp01(_t / 0.45f);
            transform.localScale = Vector3.one * BackOut(pop);
            transform.position = _basePos + Vector3.up * (Mathf.Sin(_t * 1.4f) * 0.005f);
            transform.Rotate(Vector3.up, 25f * Time.deltaTime, Space.World);
        }

        static float BackOut(float t)
        {
            const float s = 1.70158f;
            t -= 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }
    }
}
