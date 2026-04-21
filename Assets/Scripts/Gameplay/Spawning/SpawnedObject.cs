using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpawnedObject : MonoBehaviour
    {
        [SerializeField] private float missedYThreshold = 42f;

        private SpawnManager _owner;
        private Rigidbody _rigidbody;
        private bool _isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_isActive)
                return;

            if (transform.position.y < missedYThreshold)
                ReturnToPool();
        }

        public void Launch(
            SpawnManager owner,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            Vector3 angularVelocity)
        {
            _owner = owner;
            _isActive = true;

            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;
        }

        public void ReturnToPool()
        {
            TryReturnToPool();
        }

        public bool TryReturnToPool()
        {
            if (!_isActive)
                return false;

            _isActive = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            if (_owner != null)
                _owner.Release(this);
            else
                gameObject.SetActive(false);

            return true;
        }
    }
}
