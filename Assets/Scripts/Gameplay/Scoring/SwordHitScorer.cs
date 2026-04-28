using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    [RequireComponent(typeof(Rigidbody))]
    public class SwordHitScorer : MonoBehaviour
    {
        [Header("Slice Detection")]
        [SerializeField] private float minimumSliceSpeed = 2.75f;
        [SerializeField] private Vector3 localBladeAxis = Vector3.forward;
        [SerializeField] private float minimumPlaneNormalMagnitude = 0.2f;

        private Rigidbody _rigidbody;
        private Vector3 _previousPosition;
        private Vector3 _currentVelocity;
        private bool _hasPreviousPosition;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _previousPosition = transform.position;
            _hasPreviousPosition = true;
        }

        private void FixedUpdate()
        {
            Vector3 currentPosition = transform.position;
            if (_hasPreviousPosition)
                _currentVelocity = (currentPosition - _previousPosition) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);

            _previousPosition = currentPosition;
            _hasPreviousPosition = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null)
                return;

            Vector3 contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : collision.collider.ClosestPoint(transform.position);

            TrySliceHit(collision.collider, contactPoint);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            TrySliceHit(other, other.ClosestPoint(transform.position));
        }

        private void TrySliceHit(Collider other, Vector3 hitPoint)
        {
            if (other == null)
                return;

            SpawnedObject spawnedObject = ResolveComponent<SpawnedObject>(other);
            if (spawnedObject == null)
                return;

            Vector3 bladeVelocity = ResolveBladeVelocity();
            if (bladeVelocity.magnitude < minimumSliceSpeed)
                return;

            Vector3 bladeDirection = transform.TransformDirection(localBladeAxis).normalized;
            Vector3 slicePlaneNormal = Vector3.Cross(bladeVelocity.normalized, bladeDirection);
            if (slicePlaneNormal.sqrMagnitude < minimumPlaneNormalMagnitude * minimumPlaneNormalMagnitude)
                return;

            spawnedObject.TrySlice(hitPoint, slicePlaneNormal.normalized, bladeVelocity.normalized);
        }

        private Vector3 ResolveBladeVelocity()
        {
            if (_rigidbody != null && _rigidbody.linearVelocity.sqrMagnitude > _currentVelocity.sqrMagnitude)
                return _rigidbody.linearVelocity;

            return _currentVelocity;
        }

        private static T ResolveComponent<T>(Collider other) where T : Component
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out T attachedComponent))
                return attachedComponent;

            if (other.TryGetComponent(out T colliderComponent))
                return colliderComponent;

            return other.GetComponentInParent<T>();
        }
    }
}
