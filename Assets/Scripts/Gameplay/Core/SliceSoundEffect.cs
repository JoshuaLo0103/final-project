using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class SliceSoundEffect : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] private string resourcesClipPath = "Audio/22_Slash_04";
        [SerializeField] private AudioClip fruitSliceClip;

        [SerializeField] private string bombResourcesClipPath = "Audio/Explosion 1";

        [Header("Spatial Audio")]
        [SerializeField] private float volume = 0.95f;
        [SerializeField] private float bombVolume = 1f;
        [SerializeField] private float spatialBlend = 0f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 1f;
        [SerializeField] private float spread = 35f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

        private AudioClip _slashClip;
        private AudioClip _bombClip;
        private AudioSource _audioSource;

        private void Awake()
        {
            _slashClip = ResolveClip(resourcesClipPath, "slash");
            _bombClip = ResolveClip(bombResourcesClipPath, "explosion");

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
                Debug.LogWarning($"SliceSoundEffect could not load a fruit slice clip. Assign one directly or add a clip at Resources/{resourcesClipPath}.");
            if (_bombClip == null)
                Debug.LogWarning($"SliceSoundEffect could not load bomb clip at Resources/{bombResourcesClipPath}.");
        }

        private static AudioClip ResolveClip(string preferredPath, string fallbackNameHint)
        {
            AudioClip preferred = Resources.Load<AudioClip>(preferredPath);
            if (preferred != null)
                return preferred;

            AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (AudioClip clip in clips)
            {
                if (clip == null)
                    continue;

                if (clip.name.IndexOf(fallbackNameHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return clip;
            }

            return null;
        }

        private void OnEnable()
        {
            GameEvents.OnFruitSliced += HandleFruitSliced;
            GameEvents.OnBombHit += HandleBombHit;
        }

        private void OnDisable()
        {
            GameEvents.OnFruitSliced -= HandleFruitSliced;
            GameEvents.OnBombHit -= HandleBombHit;
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

        private void HandleBombHit(BombHitEventArgs eventArgs)
        {
            if (_bombClip == null || _audioSource == null)
                return;

            if (spatialBlend > 0f)
                transform.position = eventArgs.WorldPosition;

            _audioSource.pitch = Random.Range(0.94f, 1.04f);
            _audioSource.PlayOneShot(_bombClip, bombVolume);
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
