using UnityEngine;

namespace BladeFrenzy.VR
{
    /// <summary>
    /// Keeps the tracked headset inside a small standing play area by
    /// nudging the XR origin back when the player physically drifts too far.
    /// </summary>
    public class StationaryPlayAreaLimiter : MonoBehaviour
    {
        [SerializeField] private Transform trackedCamera;
        [SerializeField] private Vector2 halfExtents = new Vector2(0.6f, 0.4f);
        [SerializeField] private bool lockVerticalPosition = true;

        private Vector3 _allowedCenter;
        private float _originY;

        private void Awake()
        {
            CacheOriginState();
        }

        private void OnEnable()
        {
            CacheOriginState();
        }

        private void LateUpdate()
        {
            if (trackedCamera == null)
                return;

            Vector3 cameraPosition = trackedCamera.position;
            Vector2 offset = new Vector2(
                cameraPosition.x - _allowedCenter.x,
                cameraPosition.z - _allowedCenter.z
            );

            Vector2 clampedOffset = new Vector2(
                Mathf.Clamp(offset.x, -halfExtents.x, halfExtents.x),
                Mathf.Clamp(offset.y, -halfExtents.y, halfExtents.y)
            );

            Vector2 correction = clampedOffset - offset;
            if (correction.sqrMagnitude <= 0.000001f)
                return;

            Vector3 adjustedPosition = transform.position + new Vector3(correction.x, 0f, correction.y);
            if (lockVerticalPosition)
                adjustedPosition.y = _originY;

            transform.position = adjustedPosition;
        }

        public void RecenterPlayArea()
        {
            CacheOriginState();
        }

        private void CacheOriginState()
        {
            _originY = transform.position.y;
            _allowedCenter = trackedCamera != null ? trackedCamera.position : transform.position;
        }
    }
}
