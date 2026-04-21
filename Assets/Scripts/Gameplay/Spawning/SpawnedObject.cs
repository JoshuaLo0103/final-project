using UnityEngine;
using BladeFrenzy.Gameplay.Core;

namespace BladeFrenzy.Gameplay.Spawning
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FruitData))]
    public class SpawnedObject : MonoBehaviour
    {
        [SerializeField] private float missedYThreshold = 42f;
        [SerializeField] private SpawnedObject sourcePrefab;

        private SpawnManager _owner;
        private Rigidbody _rigidbody;
        private FruitData _fruitData;
        private bool _isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _fruitData = GetComponent<FruitData>();
        }

        private void Update()
        {
            if (!_isActive)
                return;

            if (transform.position.y < missedYThreshold)
                ReturnToPool(true);
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

        public void HandleSuccessfulSlice()
        {
            if (!_isActive || _fruitData == null)
                return;

            if (_fruitData.IsBomb)
                GameEvents.RaiseBombHit(_fruitData, transform.position);
            else
                GameEvents.RaiseFruitSliced(_fruitData, transform.position);

            ReturnToPool(false);
        }

        public void HandleBombHit()
        {
            if (!_isActive)
                return;

            GameEvents.RaiseBombHit(_fruitData, transform.position);
            ReturnToPool(false);
        }

        public void SetSourcePrefab(SpawnedObject prefab)
        {
            sourcePrefab = prefab;
        }

        public SpawnedObject GetSourcePrefab()
        {
            return sourcePrefab;
        }

        public void ReturnToPool()
        {
            ReturnToPool(true);
        }

        private void ReturnToPool(bool reportMiss)
        {
            if (!_isActive)
                return;

            _isActive = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            if (reportMiss && _fruitData != null && !_fruitData.IsBomb)
                GameEvents.RaiseFruitMissed(_fruitData, transform.position);

            if (_owner != null)
                _owner.Release(this);
            else
                gameObject.SetActive(false);
        }
    }
}
