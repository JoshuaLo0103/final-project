using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class SpawnPointFlashFeedback : MonoBehaviour
    {
        [SerializeField] private float flashDuration = 0.5f;
        [SerializeField] private float scaleMultiplier = 2.6f;
        [SerializeField] private Color flashColor = new(1f, 0.96f, 0.3f, 1f);
        [SerializeField] private float emissionBoost = 6f;
        [SerializeField] private bool autoCreateMarkerIfMissing = true;
        [SerializeField] private float markerSize = 0.5f;
        [SerializeField] private float markerVerticalOffset = 1.4f;
        [SerializeField] private float colorShiftPerTrigger = 0.12f;

        private readonly Dictionary<Transform, Coroutine> _activeFlashes = new();
        private readonly Dictionary<Transform, Vector3> _baseScales = new();
        private readonly Dictionary<Transform, Transform> _markerRoots = new();
        private readonly Dictionary<Renderer, Color> _baseColors = new();
        private readonly Dictionary<Renderer, Color> _baseEmissionColors = new();
        private MaterialPropertyBlock _propertyBlock;
        private float _currentHue = 0.58f;

        private void Awake()
        {
            _propertyBlock ??= new MaterialPropertyBlock();
        }

        public void Trigger(Transform spawnPoint)
        {
            if (spawnPoint == null)
                return;
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            AdvanceFlashColor();
            EnsureMarker(spawnPoint);

            Transform flashTarget = _markerRoots.TryGetValue(spawnPoint, out Transform markerRoot) && markerRoot != null
                ? markerRoot
                : spawnPoint;

            if (!_baseScales.ContainsKey(flashTarget))
                _baseScales[flashTarget] = flashTarget.localScale;

            if (_activeFlashes.TryGetValue(spawnPoint, out Coroutine existing))
                StopCoroutine(existing);

            _activeFlashes[spawnPoint] = StartCoroutine(FlashRoutine(spawnPoint, flashTarget));
        }

        public void EnsureMarkersFor(Transform[] spawnPoints)
        {
            if (spawnPoints == null)
                return;

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                    continue;

                EnsureMarker(spawnPoint);
            }
        }

        private void EnsureMarker(Transform spawnPoint)
        {
            if (!autoCreateMarkerIfMissing)
                return;

            markerVerticalOffset = Mathf.Max(markerVerticalOffset, 1.25f);

            Transform markerTransform = spawnPoint.Find("SpawnFlashMarker");
            GameObject marker;
            if (markerTransform != null)
            {
                marker = markerTransform.gameObject;
            }
            else
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "SpawnFlashMarker";
                marker.transform.SetParent(spawnPoint, false);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localRotation = Quaternion.identity;
                marker.layer = spawnPoint.gameObject.layer;

            }

            marker.transform.localScale = Vector3.one * markerSize;
            marker.transform.localPosition = Vector3.up * markerVerticalOffset;
            _markerRoots[spawnPoint] = marker.transform;

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                if (Application.isPlaying)
                    Destroy(markerCollider);
                else
                    DestroyImmediate(markerCollider);
            }

            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer == null)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            if (shader != null)
            {
                Material markerMaterial = new Material(shader);
                markerMaterial.color = new Color(0.2f, 0.6f, 1f, 1f);
                if (markerMaterial.HasProperty("_BaseColor"))
                    markerMaterial.SetColor("_BaseColor", new Color(0.2f, 0.6f, 1f, 1f));
                if (markerMaterial.HasProperty("_EmissionColor"))
                    markerMaterial.SetColor("_EmissionColor", new Color(0.25f, 0.45f, 1f, 1f));
                markerRenderer.sharedMaterial = markerMaterial;
            }
        }

        private void AdvanceFlashColor()
        {
            _currentHue += colorShiftPerTrigger;
            if (_currentHue > 1f)
                _currentHue -= 1f;

            Color shifted = Color.HSVToRGB(_currentHue, 0.9f, 1f);
            shifted.a = 1f;
            flashColor = shifted;
        }

        private IEnumerator FlashRoutine(Transform spawnPoint, Transform flashTarget)
        {
            Vector3 baseScale = _baseScales[flashTarget];
            Vector3 peakScale = baseScale * scaleMultiplier;

            Renderer[] renderers = spawnPoint.GetComponentsInChildren<Renderer>(true);
            CacheRendererState(renderers);

            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flashDuration);

                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                float pulse = Mathf.Sin(easeOut * Mathf.PI);
                flashTarget.localScale = Vector3.Lerp(baseScale, peakScale, pulse);

                ApplyRendererFlash(renderers, pulse);
                yield return null;
            }

            flashTarget.localScale = baseScale;
            RestoreRendererState(renderers);
            _activeFlashes.Remove(spawnPoint);
        }

        private void CacheRendererState(Renderer[] renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                Material sharedMaterial = renderer.sharedMaterial;
                if (sharedMaterial == null)
                    continue;

                if (!_baseColors.ContainsKey(renderer))
                    _baseColors[renderer] = sharedMaterial.HasProperty("_BaseColor")
                        ? sharedMaterial.GetColor("_BaseColor")
                        : sharedMaterial.color;

                if (!_baseEmissionColors.ContainsKey(renderer))
                    _baseEmissionColors[renderer] = sharedMaterial.HasProperty("_EmissionColor")
                        ? sharedMaterial.GetColor("_EmissionColor")
                        : Color.black;
            }
        }

        private void ApplyRendererFlash(Renderer[] renderers, float pulse)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(_propertyBlock);

                if (_baseColors.TryGetValue(renderer, out Color baseColor))
                {
                    Color tinted = Color.Lerp(baseColor, flashColor, pulse * 0.5f);
                    _propertyBlock.SetColor("_BaseColor", tinted);
                    _propertyBlock.SetColor("_Color", tinted);
                }

                if (_baseEmissionColors.TryGetValue(renderer, out Color baseEmission))
                {
                    Color emission = baseEmission + flashColor * (pulse * emissionBoost);
                    _propertyBlock.SetColor("_EmissionColor", emission);
                }

                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void RestoreRendererState(Renderer[] renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(_propertyBlock);

                if (_baseColors.TryGetValue(renderer, out Color baseColor))
                {
                    _propertyBlock.SetColor("_BaseColor", baseColor);
                    _propertyBlock.SetColor("_Color", baseColor);
                }

                if (_baseEmissionColors.TryGetValue(renderer, out Color baseEmission))
                    _propertyBlock.SetColor("_EmissionColor", baseEmission);

                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
