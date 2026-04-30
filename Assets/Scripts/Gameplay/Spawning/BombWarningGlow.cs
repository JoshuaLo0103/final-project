using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class BombWarningGlow : MonoBehaviour
    {
        [SerializeField] private Color warningColor = new(1f, 0.08f, 0.02f, 1f);
        [SerializeField] private float baseTintStrength = 0.45f;
        [SerializeField] private float pulseTintStrength = 1f;
        [SerializeField] private float baseEmissionStrength = 1.5f;
        [SerializeField] private float pulseEmissionStrength = 7f;
        [SerializeField] private float pulseSpeed = 7f;
        [SerializeField] private float sizePulseStrength = 0.14f;
        [SerializeField] private float baseLightIntensity = 1.2f;
        [SerializeField] private float pulseLightIntensity = 4.5f;
        [SerializeField] private float warningLightRange = 2.3f;

        private readonly System.Collections.Generic.Dictionary<Renderer, Color> _baseColors = new();
        private readonly System.Collections.Generic.Dictionary<Renderer, Color> _baseEmissionColors = new();
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;
        private Light _warningLight;
        private Vector3 _baseScale;
        private bool _isGlowing;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
            CacheRenderers();
        }

        private void Update()
        {
            if (!_isGlowing)
                return;

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float tintStrength = Mathf.Lerp(baseTintStrength, pulseTintStrength, pulse);
            float emissionStrength = Mathf.Lerp(baseEmissionStrength, pulseEmissionStrength, pulse);
            float lightIntensity = Mathf.Lerp(baseLightIntensity, pulseLightIntensity, pulse);

            ApplyGlow(tintStrength, emissionStrength);
            if (_warningLight != null)
                _warningLight.intensity = lightIntensity;

            transform.localScale = _baseScale * (1f + pulse * sizePulseStrength);
        }

        public void BeginGlow()
        {
            if (_renderers == null || _renderers.Length == 0)
                CacheRenderers();

            _baseScale = transform.localScale;
            EnsureWarningLight();
            if (_warningLight != null)
                _warningLight.enabled = true;

            _isGlowing = true;
        }

        public void EndGlow()
        {
            if (!_isGlowing)
                return;

            _isGlowing = false;
            transform.localScale = _baseScale;
            if (_warningLight != null)
                _warningLight.enabled = false;

            RestoreRendererState();
        }

        private void EnsureWarningLight()
        {
            if (_warningLight != null)
                return;

            Transform lightTransform = transform.Find("BombWarningLight");
            GameObject lightObject;
            if (lightTransform != null)
            {
                lightObject = lightTransform.gameObject;
            }
            else
            {
                lightObject = new GameObject("BombWarningLight");
                lightObject.transform.SetParent(transform, false);
                lightObject.transform.localPosition = Vector3.zero;
            }

            _warningLight = lightObject.GetComponent<Light>();
            if (_warningLight == null)
                _warningLight = lightObject.AddComponent<Light>();

            _warningLight.type = LightType.Point;
            _warningLight.color = warningColor;
            _warningLight.range = warningLightRange;
            _warningLight.intensity = baseLightIntensity;
            _warningLight.shadows = LightShadows.None;
            _warningLight.enabled = false;
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null || targetRenderer.sharedMaterial == null)
                    continue;

                Material material = targetRenderer.sharedMaterial;
                if (!_baseColors.ContainsKey(targetRenderer))
                {
                    _baseColors[targetRenderer] = material.HasProperty("_BaseColor")
                        ? material.GetColor("_BaseColor")
                        : material.color;
                }

                if (!_baseEmissionColors.ContainsKey(targetRenderer))
                {
                    _baseEmissionColors[targetRenderer] = material.HasProperty("_EmissionColor")
                        ? material.GetColor("_EmissionColor")
                        : Color.black;
                }
            }
        }

        private void ApplyGlow(float tintStrength, float emissionStrength)
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(_propertyBlock);

                Color baseColor = _baseColors.TryGetValue(targetRenderer, out Color cachedBaseColor)
                    ? cachedBaseColor
                    : Color.white;
                Color tintedColor = Color.Lerp(baseColor, warningColor, tintStrength);
                _propertyBlock.SetColor("_BaseColor", tintedColor);
                _propertyBlock.SetColor("_Color", tintedColor);

                Color baseEmission = _baseEmissionColors.TryGetValue(targetRenderer, out Color cachedEmission)
                    ? cachedEmission
                    : Color.black;
                _propertyBlock.SetColor("_EmissionColor", baseEmission + warningColor * emissionStrength);

                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void RestoreRendererState()
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(_propertyBlock);

                if (_baseColors.TryGetValue(targetRenderer, out Color baseColor))
                {
                    _propertyBlock.SetColor("_BaseColor", baseColor);
                    _propertyBlock.SetColor("_Color", baseColor);
                }

                if (_baseEmissionColors.TryGetValue(targetRenderer, out Color baseEmission))
                    _propertyBlock.SetColor("_EmissionColor", baseEmission);

                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
