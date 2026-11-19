using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Periodiq.AR
{
    /// <summary>
    /// Shows a reticle on detected planes; first tap places the periodic table there.
    /// Put this on the XR Origin (it needs ARRaycastManager + ARPlaneManager).
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARTablePlacer : MonoBehaviour
    {
        [SerializeField] GameObject tableRoot;   // object with PeriodicTableBuilder, starts disabled
        [SerializeField] Transform reticle;      // small ring sprite/quad, optional
        [SerializeField] bool hidePlanesAfterPlacing = true;

        ARRaycastManager _raycast;
        ARPlaneManager _planes;
        Camera _cam;
        static readonly List<ARRaycastHit> Hits = new();

        public bool IsPlaced { get; private set; }

        void Awake()
        {
            _raycast = GetComponent<ARRaycastManager>();
            _planes = GetComponent<ARPlaneManager>();
            _cam = Camera.main;
            if (tableRoot) tableRoot.SetActive(false);
        }

        void OnEnable() => EnhancedTouchSupport.Enable();
        void OnDisable() => EnhancedTouchSupport.Disable();

        void Update()
        {
            if (IsPlaced) return;

            // Reticle follows the centre of the screen.
            var centre = new Vector2(Screen.width / 2f, Screen.height / 2f);
            bool hasHit = _raycast.Raycast(centre, Hits, TrackableType.PlaneWithinPolygon);
            if (reticle)
            {
                reticle.gameObject.SetActive(hasHit);
                if (hasHit) reticle.SetPositionAndRotation(Hits[0].pose.position, Hits[0].pose.rotation);
            }

            if (Touch.activeTouches.Count == 0) return;
            var t = Touch.activeTouches[0];
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

            if (_raycast.Raycast(t.screenPosition, Hits, TrackableType.PlaneWithinPolygon))
                Place(Hits[0].pose);
        }

        void Place(Pose pose)
        {
            // Face the user, staying upright.
            var toCam = _cam.transform.position - pose.position;
            toCam.y = 0f;
            var rot = toCam.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(-toCam) : pose.rotation;

            tableRoot.transform.SetPositionAndRotation(pose.position, rot);
            tableRoot.SetActive(true);
            IsPlaced = true;
            if (reticle) reticle.gameObject.SetActive(false);

            if (hidePlanesAfterPlacing && _planes)
            {
                _planes.enabled = false;
                foreach (var p in _planes.trackables) p.gameObject.SetActive(false);
            }
        }

        /// <summary>Hook to a "Move table" button.</summary>
        public void ResetPlacement()
        {
            IsPlaced = false;
            tableRoot.SetActive(false);
            if (_planes)
            {
                _planes.enabled = true;
                foreach (var p in _planes.trackables) p.gameObject.SetActive(true);
            }
        }
    }
}
