using System.Collections.Generic;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class HealingFruitEffect : MonoBehaviour
    {
        [SerializeField] private Color healingColor = new(0.1f, 1f, 0.25f, 1f);
        [SerializeField] private float tintStrength = 0.65f;
        [SerializeField] private float emissionStrength = 3f;
        [SerializeField] private float pulseSpeed = 4.5f;
        [SerializeField] private float pulseScaleStrength = 0.08f;
        [SerializeField] private float lightIntensity = 1.8f;
        [SerializeField] private float lightRange = 1.8f;

        private readonly Dictionary<Renderer, Color> _baseColors = new();
        private readonly Dictionary<Renderer, Color> _baseEmissionColors = new();
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;
        private Light _healingLight;
        private Vector3 _baseScale;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
            CacheRenderers();
            EnsureHealingLight();
        }

        private void OnEnable()
        {
            _baseScale = transform.localScale;
            if (_healingLight != null)
                _healingLight.enabled = true;
        }

        private void OnDisable()
        {
            if (_healingLight != null)
                _healingLight.enabled = false;
            RestoreRendererState();
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            transform.localScale = _baseScale * (1f + pulse * pulseScaleStrength);
            ApplyHealingColor(pulse);

            if (_healingLight != null)
                _healingLight.intensity = Mathf.Lerp(lightIntensity * 0.45f, lightIntensity, pulse);
        }

        private void EnsureHealingLight()
        {
            if (_healingLight != null)
                return;

            Transform lightTransform = transform.Find("HealingFruitLight");
            GameObject lightObject = lightTransform != null
                ? lightTransform.gameObject
                : new GameObject("HealingFruitLight");

            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = Vector3.zero;

            _healingLight = lightObject.GetComponent<Light>();
            if (_healingLight == null)
                _healingLight = lightObject.AddComponent<Light>();

            _healingLight.type = LightType.Point;
            _healingLight.color = healingColor;
            _healingLight.range = lightRange;
            _healingLight.intensity = lightIntensity;
            _healingLight.shadows = LightShadows.None;
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

        private void ApplyHealingColor(float pulse)
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(_propertyBlock);

                Color baseColor = _baseColors.TryGetValue(targetRenderer, out Color cachedBaseColor)
                    ? cachedBaseColor
                    : Color.white;
                Color tintedColor = Color.Lerp(baseColor, healingColor, tintStrength);
                _propertyBlock.SetColor("_BaseColor", tintedColor);
                _propertyBlock.SetColor("_Color", tintedColor);

                Color baseEmission = _baseEmissionColors.TryGetValue(targetRenderer, out Color cachedEmission)
                    ? cachedEmission
                    : Color.black;
                _propertyBlock.SetColor("_EmissionColor", baseEmission + healingColor * (emissionStrength * pulse));

                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void RestoreRendererState()
        {
            if (_renderers == null)
                return;

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
