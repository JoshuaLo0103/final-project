using System.Collections;
using BladeFrenzy.Gameplay.Core;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    public class SwordComboGlowController : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Tier Colors")]
        [SerializeField] private Color baseTint = Color.white;
        [SerializeField] private Color tierTwoColor = new(0.25f, 0.9f, 1f, 1f);
        [SerializeField] private Color tierThreeColor = new(0.45f, 1f, 0.35f, 1f);
        [SerializeField] private Color tierFourColor = new(1f, 0.38f, 0.95f, 1f);

        [Header("Alternate Style")]
        [SerializeField] private Color alternateBaseTint = new(1f, 0.52f, 0.14f, 1f);
        [SerializeField] private Color alternateTierTwoColor = new(1f, 0.82f, 0.18f, 1f);
        [SerializeField] private Color alternateTierThreeColor = new(1f, 0.35f, 0.16f, 1f);
        [SerializeField] private Color alternateTierFourColor = new(0.85f, 0.28f, 1f, 1f);


        [Header("Glow")]
        [SerializeField] private float baseEmissionIntensity = 0.35f;
        [SerializeField] private float tierTwoEmissionIntensity = 1.4f;
        [SerializeField] private float tierThreeEmissionIntensity = 2.4f;
        [SerializeField] private float tierFourEmissionIntensity = 3.4f;
        [SerializeField] private float alternateBaseEmissionIntensity = 1.65f;

        [SerializeField] private float pulseBoost = 1.35f;
        [SerializeField] private float pulseDuration = 0.16f;
        [SerializeField] private float settleDuration = 0.28f;

        [Header("Glow Light")]
        [SerializeField] private bool useGlowLight = true;
        [SerializeField] private Vector3 glowLightLocalPosition = new(0f, 0f, 1.35f);
        [SerializeField] private float glowLightRange = 1.2f;
        [SerializeField] private float glowLightIntensityScale = 0.55f;


        private Material[][] _materialInstances;
        private Coroutine _glowRoutine;
        private Color _currentTint;
        private Color _currentEmission;
        private int _currentMultiplier = 1;
        private bool _alternateStyle;
        private Light _glowLight;



        private void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);

            CreateMaterialInstances();
            EnsureGlowLight();

            SetGlow(baseTint, baseTint * baseEmissionIntensity);
        }

        private void OnEnable()
        {
            GameEvents.OnComboTierChanged += HandleComboTierChanged;
            GameEvents.OnRunStarted += HandleRunStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnComboTierChanged -= HandleComboTierChanged;
            GameEvents.OnRunStarted -= HandleRunStarted;

            if (_glowRoutine != null)
            {
                StopCoroutine(_glowRoutine);
                _glowRoutine = null;
            }
        }

private void OnDestroy()
        {
            if (_materialInstances == null)
                return;

            foreach (Material[] rendererMaterials in _materialInstances)
            {
                if (rendererMaterials == null)
                    continue;

                foreach (Material material in rendererMaterials)
                {
                    if (material == null)
                        continue;

                    if (Application.isPlaying)
                        Destroy(material);
                    else
                        DestroyImmediate(material);
                }
            }
        }

private void HandleRunStarted()
        {
            _currentMultiplier = 1;
            _alternateStyle = false;
            StartGlowAnimation(ResolveCurrentTint(), ResolveCurrentEmission());
        }

private void HandleComboTierChanged(ComboTierChangedEventArgs eventArgs)
        {
            _currentMultiplier = eventArgs.Multiplier;
            StartGlowAnimation(ResolveCurrentTint(), ResolveCurrentEmission());
        }

        private void StartGlowAnimation(Color targetTint, Color targetEmission)
        {
            if (_glowRoutine != null)
                StopCoroutine(_glowRoutine);

            _glowRoutine = StartCoroutine(AnimateGlow(targetTint, targetEmission));
        }

        private IEnumerator AnimateGlow(Color targetTint, Color targetEmission)
        {
            Color startTint = _currentTint;
            Color startEmission = _currentEmission;
            Color pulseEmission = targetEmission * pulseBoost;

            yield return LerpGlow(startTint, startEmission, targetTint, pulseEmission, pulseDuration);
            yield return LerpGlow(targetTint, pulseEmission, targetTint, targetEmission, settleDuration);

            _glowRoutine = null;
        }

        private IEnumerator LerpGlow(Color fromTint, Color fromEmission, Color toTint, Color toEmission, float duration)
        {
            if (duration <= 0f)
            {
                SetGlow(toTint, toEmission);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                SetGlow(Color.Lerp(fromTint, toTint, t), Color.Lerp(fromEmission, toEmission, t));
                yield return null;
            }

            SetGlow(toTint, toEmission);
        }

        private void CreateMaterialInstances()
        {
            _materialInstances = new Material[targetRenderers.Length][];
            for (int rendererIndex = 0; rendererIndex < targetRenderers.Length; rendererIndex++)
            {
                Renderer targetRenderer = targetRenderers[rendererIndex];
                if (targetRenderer == null)
                    continue;

                Material[] sharedMaterials = targetRenderer.sharedMaterials;
                Material[] instances = new Material[sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                {
                    Material source = sharedMaterials[materialIndex];
                    if (source == null)
                        continue;

                    Material instance = new(source)
                    {
                        name = $"{source.name}_ComboGlow_Runtime"
                    };
                    instance.EnableKeyword("_EMISSION");
                    instances[materialIndex] = instance;
                }

                targetRenderer.sharedMaterials = instances;
                _materialInstances[rendererIndex] = instances;
            }
        }

private void SetGlow(Color tint, Color emission)
        {
            _currentTint = tint;
            _currentEmission = emission;

            if (_materialInstances != null)
            {
                foreach (Material[] rendererMaterials in _materialInstances)
                {
                    if (rendererMaterials == null)
                        continue;

                    foreach (Material material in rendererMaterials)
                    {
                        if (material == null)
                            continue;

                        SetColorIfPresent(material, "_BaseColor", tint);
                        SetColorIfPresent(material, "_Color", tint);
                        SetColorIfPresent(material, "_EmissionColor", emission);
                    }
                }
            }

            if (_glowLight != null)
            {
                float emissionPeak = Mathf.Max(emission.r, Mathf.Max(emission.g, emission.b));
                _glowLight.color = tint;
                _glowLight.range = glowLightRange;
                _glowLight.intensity = emissionPeak * glowLightIntensityScale;
            }
        }

private Color ResolveTierColor(int multiplier)
        {
            if (_alternateStyle)
            {
                return multiplier switch
                {
                    >= 4 => alternateTierFourColor,
                    3 => alternateTierThreeColor,
                    2 => alternateTierTwoColor,
                    _ => alternateBaseTint
                };
            }

            return multiplier switch
            {
                >= 4 => tierFourColor,
                3 => tierThreeColor,
                2 => tierTwoColor,
                _ => baseTint
            };
        }

private float ResolveEmissionIntensity(int multiplier)
        {
            if (_alternateStyle && multiplier <= 1)
                return alternateBaseEmissionIntensity;

            return multiplier switch
            {
                >= 4 => tierFourEmissionIntensity,
                3 => tierThreeEmissionIntensity,
                2 => tierTwoEmissionIntensity,
                _ => baseEmissionIntensity
            };
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, color);
        }
    

public void ToggleColorStyle()
        {
            _alternateStyle = !_alternateStyle;
            StartGlowAnimation(ResolveCurrentTint(), ResolveCurrentEmission());
        }

        public void SetColorStyle(bool useAlternateStyle)
        {
            if (_alternateStyle == useAlternateStyle)
                return;

            _alternateStyle = useAlternateStyle;
            StartGlowAnimation(ResolveCurrentTint(), ResolveCurrentEmission());
        }

        private Color ResolveCurrentTint()
        {
            return ResolveTierColor(_currentMultiplier);
        }

        private Color ResolveCurrentEmission()
        {
            Color tint = ResolveCurrentTint();
            return tint * ResolveEmissionIntensity(_currentMultiplier);
        }


private void EnsureGlowLight()
        {
            if (!useGlowLight)
                return;

            Transform existingLight = transform.Find("ComboGlowLight");
            if (existingLight != null && existingLight.TryGetComponent(out _glowLight))
                return;

            GameObject lightObject = new("ComboGlowLight");
            Transform lightTransform = lightObject.transform;
            lightTransform.SetParent(transform, false);
            lightTransform.localPosition = glowLightLocalPosition;
            lightTransform.localRotation = Quaternion.identity;
            lightTransform.localScale = Vector3.one;

            _glowLight = lightObject.AddComponent<Light>();
            _glowLight.type = LightType.Point;
            _glowLight.range = glowLightRange;
            _glowLight.shadows = LightShadows.None;
            _glowLight.intensity = 0f;
        }
}
}
