using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class BombFuseSizzleSound : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] private string resourcesClipPath = "Audio/BombFuseSizzle";

        [Header("Spatial Audio")]
        [SerializeField] private float volume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
        [SerializeField] private float minDistance = 0.35f;
        [SerializeField] private float maxDistance = 6f;
        [SerializeField] private float spread = 25f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

        private AudioSource _audioSource;
        private AudioClip _sizzleClip;

        private void Awake()
        {
            _sizzleClip = Resources.Load<AudioClip>(resourcesClipPath);
            if (_sizzleClip == null)
                _sizzleClip = CreateProceduralSizzle();

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.clip = _sizzleClip;
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.volume = Mathf.Max(0f, volume);
            _audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
            _audioSource.minDistance = Mathf.Max(0.01f, minDistance);
            _audioSource.maxDistance = Mathf.Max(_audioSource.minDistance, maxDistance);
            _audioSource.spread = spread;
            _audioSource.rolloffMode = rolloffMode;
            _audioSource.dopplerLevel = 0f;
        }

        public void Play()
        {
            if (_audioSource == null || _sizzleClip == null || _audioSource.isPlaying)
                return;

            _audioSource.pitch = Random.Range(0.92f, 1.08f);
            _audioSource.Play();
        }

        public void Stop()
        {
            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.Stop();
        }

        private static AudioClip CreateProceduralSizzle()
        {
            const int sampleRate = 44100;
            const float duration = 0.65f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float cracklePhase = 0f;

            for (int index = 0; index < sampleCount; index++)
            {
                float t = index / (float)sampleCount;
                float loopEnvelope = Mathf.Sin(t * Mathf.PI);
                float hiss = Random.Range(-1f, 1f) * 0.22f;

                cracklePhase += Random.Range(850f, 1350f) / sampleRate;
                float crackle = Mathf.Sin(cracklePhase * Mathf.PI * 2f) * Random.Range(0.05f, 0.2f);
                if (Random.value > 0.965f)
                    crackle += Random.Range(0.25f, 0.45f);

                samples[index] = (hiss + crackle) * Mathf.Lerp(0.75f, 1f, loopEnvelope);
            }

            AudioClip clip = AudioClip.Create("ProceduralBombFuseSizzle", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
