using BladeFrenzy.Gameplay.Core;
using BladeFrenzy.Gameplay.Scoring;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CollectibleCoin : MonoBehaviour
    {
        [SerializeField] private float lifetime = 9f;
        [SerializeField] private float missedYThreshold = 42f;
        [SerializeField] private float customGravity = 2.6f;
        [SerializeField] private float spinDegreesPerSecond = 540f;
        [SerializeField] private float hoverAmplitude = 0.03f;
        [SerializeField] private float hoverFrequency = 3f;
        [SerializeField] private float collectDelay = 0.35f;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Color glowColor = new(1f, 0.78f, 0.18f, 1f);
        [SerializeField] private float glowIntensity = 2.2f;
        [SerializeField] private float glowRange = 3.5f;

        private Rigidbody _rigidbody;
        private CoinManager _coinManager;
        private Light _glowLight;
        private float _spawnTime;
        private Vector3 _visualBaseLocalPosition;
        private bool _collected;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _coinManager = FindFirstObjectByType<CoinManager>();
            ResolveVisualRoot();
            EnsureGlowLight();
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            ResolveVisualRoot();
            EnsureGlowLight();
            _visualBaseLocalPosition = visualRoot != null ? visualRoot.localPosition : Vector3.zero;
            _collected = false;
        }

        private void Update()
        {
            if (_collected)
                return;

            if (visualRoot != null)
            {
                visualRoot.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.Self);
                Vector3 localPosition = _visualBaseLocalPosition;
                localPosition.y += Mathf.Sin((Time.time - _spawnTime) * hoverFrequency) * hoverAmplitude;
                visualRoot.localPosition = localPosition;
            }

            if (Time.time - _spawnTime >= lifetime || transform.position.y < missedYThreshold)
                Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (_collected || _rigidbody == null)
                return;

            _rigidbody.AddForce(Vector3.down * Mathf.Max(0f, customGravity), ForceMode.Acceleration);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryCollect(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            TryCollect(collision.collider);
        }

        public void Launch(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            transform.SetPositionAndRotation(position, rotation);
            _spawnTime = Time.time;
            ResolveVisualRoot();
            _visualBaseLocalPosition = visualRoot != null ? visualRoot.localPosition : Vector3.zero;
            _collected = false;

            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.useGravity = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;
        }

        private void TryCollect(Collider other)
        {
            if (_collected || Time.time - _spawnTime < collectDelay || other == null || ResolveSword(other) == null)
                return;

            _collected = true;

            if (_coinManager == null)
                _coinManager = FindFirstObjectByType<CoinManager>();

            _coinManager?.CollectCoin(transform.position);
            Destroy(gameObject);
        }

        private static SwordHitScorer ResolveSword(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out SwordHitScorer attachedSword))
                return attachedSword;

            if (other.TryGetComponent(out SwordHitScorer sword))
                return sword;

            return other.GetComponentInParent<SwordHitScorer>();
        }

        private void EnsureGlowLight()
        {
            if (_glowLight == null)
            {
                _glowLight = GetComponent<Light>();
                if (_glowLight == null)
                    _glowLight = gameObject.AddComponent<Light>();
            }

            _glowLight.type = LightType.Point;
            _glowLight.color = glowColor;
            _glowLight.intensity = Mathf.Max(0f, glowIntensity);
            _glowLight.range = Mathf.Max(0.1f, glowRange);
            _glowLight.shadows = LightShadows.None;
        }

        private void ResolveVisualRoot()
        {
            if (visualRoot != null)
                return;

            visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
        }
    }
}
