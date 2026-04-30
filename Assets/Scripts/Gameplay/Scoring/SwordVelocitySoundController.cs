using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    [RequireComponent(typeof(Rigidbody))]
    public class SwordVelocitySoundController : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] private AudioClip highVelocityClip;
        [SerializeField] private string fallbackResourcesClipPath = "Audio/22_Slash_04";

        [Header("Velocity Trigger")]
        [SerializeField] private float highVelocityThreshold = 6.5f;
        [SerializeField] private float rearmVelocityThreshold = 3.5f;
        [SerializeField] private float cooldown = 0.22f;

        [Header("Audio")]
        [SerializeField] private float volume = 0.75f;
        [SerializeField] private Vector2 pitchRange = new(0.94f, 1.08f);
        [SerializeField] private float spatialBlend = 0.65f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 8f;

        private Rigidbody _rigidbody;
        private AudioSource _audioSource;
        private Vector3 _previousPosition;
        private Vector3 _trackedVelocity;
        private bool _hasPreviousPosition;
        private bool _isArmed = true;
        private float _lastPlayTime = float.NegativeInfinity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _previousPosition = transform.position;
            _hasPreviousPosition = true;

            if (highVelocityClip == null && !string.IsNullOrWhiteSpace(fallbackResourcesClipPath))
                highVelocityClip = Resources.Load<AudioClip>(fallbackResourcesClipPath);

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
            _audioSource.minDistance = Mathf.Max(0.01f, minDistance);
            _audioSource.maxDistance = Mathf.Max(_audioSource.minDistance, maxDistance);
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.dopplerLevel = 0f;

            if (highVelocityClip == null)
                Debug.LogWarning($"{name} could not load high velocity sword clip. Assign a clip or add one at Resources/{fallbackResourcesClipPath}.", this);
        }

        private void OnEnable()
        {
            _isArmed = true;
            _lastPlayTime = float.NegativeInfinity;
            _previousPosition = transform.position;
            _hasPreviousPosition = true;
        }

        private void Update()
        {
            if (highVelocityClip == null || _audioSource == null)
                return;

            float speed = ResolveSpeed();
            if (speed <= rearmVelocityThreshold)
                _isArmed = true;

            if (!_isArmed || speed < highVelocityThreshold || Time.time < _lastPlayTime + cooldown)
                return;

            _isArmed = false;
            _lastPlayTime = Time.time;
            _audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            _audioSource.PlayOneShot(highVelocityClip, volume);
        }

        private float ResolveSpeed()
        {
            Vector3 currentPosition = transform.position;
            if (_hasPreviousPosition)
                _trackedVelocity = (currentPosition - _previousPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

            _previousPosition = currentPosition;
            _hasPreviousPosition = true;

            Vector3 rigidbodyVelocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector3.zero;
            return Mathf.Max(rigidbodyVelocity.magnitude, _trackedVelocity.magnitude);
        }
    }
}
