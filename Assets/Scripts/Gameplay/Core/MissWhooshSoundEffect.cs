using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class MissWhooshSoundEffect : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] private string resourcesClipPath = "Audio/MissWhoosh";

        [Header("Spatial Audio")]
        [SerializeField] private float volume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float maxDistance = 7f;
        [SerializeField] private float spread = 45f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

        private AudioClip _whooshClip;

        private void Awake()
        {
            _whooshClip = Resources.Load<AudioClip>(resourcesClipPath);
            if (_whooshClip == null)
                _whooshClip = CreateProceduralWhoosh();
        }

        private void OnEnable()
        {
            GameEvents.OnFruitMissed += HandleFruitMissed;
        }

        private void OnDisable()
        {
            GameEvents.OnFruitMissed -= HandleFruitMissed;
        }

        private void HandleFruitMissed(FruitMissedEventArgs eventArgs)
        {
            if (_whooshClip == null)
                return;

            GameObject soundObject = new GameObject("MissWhooshSound");
            soundObject.transform.position = eventArgs.WorldPosition;

            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = _whooshClip;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = Mathf.Max(0f, volume);
            source.spatialBlend = Mathf.Clamp01(spatialBlend);
            source.minDistance = Mathf.Max(0.01f, minDistance);
            source.maxDistance = Mathf.Max(source.minDistance, maxDistance);
            source.spread = spread;
            source.rolloffMode = rolloffMode;
            source.dopplerLevel = 0f;
            source.pitch = Random.Range(0.92f, 1.08f);
            source.Play();

            Destroy(soundObject, _whooshClip.length + 0.25f);
        }

        private static AudioClip CreateProceduralWhoosh()
        {
            const int sampleRate = 44100;
            const float duration = 0.45f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int index = 0; index < sampleCount; index++)
            {
                float t = index / (float)sampleCount;
                float envelope = Mathf.Sin(t * Mathf.PI);
                float frequency = Mathf.Lerp(520f, 120f, t);
                phase += frequency / sampleRate;

                float airyTone = Mathf.Sin(phase * Mathf.PI * 2f);
                float breath = Random.Range(-1f, 1f) * 0.35f;
                samples[index] = (airyTone * 0.65f + breath) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create("ProceduralMissWhoosh", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
