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
        [SerializeField] private float farRingRadius = 0.38f;
        [SerializeField] private float nearRingRadius = 0.7f;
        [SerializeField] private float ringWidth = 0.07f;
        [SerializeField] private float ringPulseWidth = 0.11f;
        [SerializeField] private float ringGrowStartDistance = 6f;
        [SerializeField] private float ringGrowEndDistance = 1.1f;

        private readonly System.Collections.Generic.Dictionary<Renderer, Color> _baseColors = new();
        private readonly System.Collections.Generic.Dictionary<Renderer, Color> _baseEmissionColors = new();
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;
        private Light _warningLight;
        private LineRenderer _warningRing;
        private Material _warningRingMaterial;
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

            UpdateWarningRing(pulse);
            transform.localScale = _baseScale * (1f + pulse * sizePulseStrength);
        }

        public void BeginGlow()
        {
            if (_renderers == null || _renderers.Length == 0)
                CacheRenderers();

            _baseScale = transform.localScale;
            EnsureWarningLight();
            EnsureWarningRing();
            if (_warningLight != null)
                _warningLight.enabled = true;
            if (_warningRing != null)
                _warningRing.gameObject.SetActive(true);

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
            if (_warningRing != null)
                _warningRing.gameObject.SetActive(false);

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

        private void EnsureWarningRing()
        {
            if (_warningRing != null)
                return;

            Transform existingRing = transform.Find("BombWarningRing");
            GameObject ringObject;
            if (existingRing != null)
            {
                ringObject = existingRing.gameObject;
            }
            else
            {
                ringObject = new GameObject("BombWarningRing");
                ringObject.transform.SetParent(transform, false);
                ringObject.transform.localPosition = Vector3.zero;
                ringObject.transform.localRotation = Quaternion.identity;
            }

            _warningRing = ringObject.GetComponent<LineRenderer>();
            if (_warningRing == null)
                _warningRing = ringObject.AddComponent<LineRenderer>();

            _warningRing.loop = true;
            _warningRing.useWorldSpace = false;
            _warningRing.positionCount = 64;
            _warningRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _warningRing.receiveShadows = false;
            _warningRingMaterial = CreateRingMaterial();
            _warningRing.sharedMaterial = _warningRingMaterial;
            WriteRingPositions(farRingRadius);
            ringObject.SetActive(false);
        }

        private void UpdateWarningRing(float pulse)
        {
            if (_warningRing == null)
                return;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                _warningRing.transform.rotation = mainCamera.transform.rotation;

            float closeness = ResolvePlayerCloseness();
            float radius = Mathf.Lerp(farRingRadius, nearRingRadius, closeness);
            float width = Mathf.Lerp(ringWidth, ringPulseWidth, pulse);
            float alpha = Mathf.Lerp(0.12f, 1f, pulse);
            Color ringColor = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);

            WriteRingPositions(radius);
            _warningRing.startWidth = width;
            _warningRing.endWidth = width;
            _warningRing.startColor = ringColor;
            _warningRing.endColor = ringColor;
            if (_warningRingMaterial != null)
            {
                _warningRingMaterial.color = ringColor;
                if (_warningRingMaterial.HasProperty("_BaseColor"))
                    _warningRingMaterial.SetColor("_BaseColor", ringColor);
                if (_warningRingMaterial.HasProperty("_Color"))
                    _warningRingMaterial.SetColor("_Color", ringColor);
            }
        }

        private float ResolvePlayerCloseness()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return 0f;

            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
            return Mathf.InverseLerp(ringGrowStartDistance, ringGrowEndDistance, distance);
        }

        private void WriteRingPositions(float radius)
        {
            for (int index = 0; index < _warningRing.positionCount; index++)
            {
                float angle = index / (float)_warningRing.positionCount * Mathf.PI * 2f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                _warningRing.SetPosition(index, position);
            }
        }

        private Material CreateRingMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            Material material = shader != null
                ? new Material(shader)
                : new Material(Shader.Find("Sprites/Default"));

            Color ringColor = new Color(warningColor.r, warningColor.g, warningColor.b, 1f);
            material.color = ringColor;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", ringColor);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", ringColor);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_AlphaClip"))
                material.SetFloat("_AlphaClip", 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            return material;
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
            EnsurePropertyBlock();

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
            EnsurePropertyBlock();

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

        private void EnsurePropertyBlock()
        {
            _propertyBlock ??= new MaterialPropertyBlock();
        }
    }
}
