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

        private void Awake()
        {
            _slashClip = Resources.Load<AudioClip>(resourcesClipPath);

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
            if (_slashClip == null || eventArgs.FruitData == null)
                return;

            float pitch = ResolvePitch(eventArgs.FruitData.FruitType);
            PlayAtPoint(eventArgs.WorldPosition, pitch);
        }

        private void PlayAtPoint(Vector3 worldPosition, float pitch)
        {
            GameObject soundObject = new GameObject("SliceSoundEffect");
            soundObject.transform.position = worldPosition;

            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = _slashClip;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
            audioSource.volume = Mathf.Max(0f, volume);
            audioSource.pitch = pitch;
            audioSource.minDistance = Mathf.Max(0.01f, minDistance);
            audioSource.maxDistance = Mathf.Max(audioSource.minDistance, maxDistance);
            audioSource.spread = spread;
            audioSource.rolloffMode = rolloffMode;
            audioSource.dopplerLevel = 0f;

            audioSource.Play();

            float clipDuration = _slashClip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
            Destroy(soundObject, clipDuration + 0.1f);
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
