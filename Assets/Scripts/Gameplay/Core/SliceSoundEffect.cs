using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class SliceSoundEffect : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] private string resourcesClipPath = "Audio/22_Slash_04";

        [Header("Spatial Audio")]
        [SerializeField] private float volume = 0.95f;
        [SerializeField] private float spatialBlend = 0f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 1f;
        [SerializeField] private float spread = 35f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

        private AudioClip _slashClip;
        private AudioSource _audioSource;

        private void Awake()
        {
            _slashClip = Resources.Load<AudioClip>(resourcesClipPath);

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
            _audioSource.volume = Mathf.Max(0f, volume);
            _audioSource.minDistance = Mathf.Max(0.01f, minDistance);
            _audioSource.maxDistance = Mathf.Max(_audioSource.minDistance, maxDistance);
            _audioSource.spread = spread;
            _audioSource.rolloffMode = rolloffMode;
            _audioSource.dopplerLevel = 0f;

            if (_slashClip == null)
                Debug.LogWarning($"SliceSoundEffect could not load clip at Resources/{resourcesClipPath}.");
        }

        private void OnEnable()
        {
            GameEvents.OnFruitSliced += HandleFruitSliced;
        }

        private void OnDisable()
        {
            GameEvents.OnFruitSliced -= HandleFruitSliced;
        }

        private void HandleFruitSliced(FruitSliceEventArgs eventArgs)
        {
            if (_slashClip == null || _audioSource == null)
                return;

            if (spatialBlend > 0f)
                transform.position = eventArgs.WorldPosition;

            FruitType fruitType = eventArgs.FruitData != null ? eventArgs.FruitData.FruitType : default;
            _audioSource.pitch = ResolvePitch(fruitType);
            _audioSource.PlayOneShot(_slashClip, volume);
        }

        private static float ResolvePitch(FruitType fruitType)
        {
            Vector2 range = fruitType switch
            {
                FruitType.Apple => new Vector2(0.97f, 1.03f),
                FruitType.Banana => new Vector2(1.08f, 1.15f),
                FruitType.Orange => new Vector2(1.0f, 1.07f),
                FruitType.Watermelon => new Vector2(0.88f, 0.96f),
                _ => new Vector2(1f, 1f)
            };

            return Random.Range(range.x, range.y);
        }
    }
}
