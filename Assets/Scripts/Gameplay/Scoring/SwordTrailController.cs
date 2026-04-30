using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    [RequireComponent(typeof(Rigidbody))]
    public class SwordTrailController : MonoBehaviour
    {
        [Header("Trail Anchor")]
        [SerializeField] private Transform trailAnchor;
        [SerializeField] private Vector3 fallbackLocalAnchorPosition = new(0f, 0f, 2.25f);

        [Header("Velocity Ease")]
        [SerializeField] private float minVisibleSpeed = 1.25f;
        [SerializeField] private float fullTrailSpeed = 7.5f;
        [SerializeField] private float easeResponsiveness = 14f;

        [Header("Trail Shape")]
        [SerializeField] private float minLifetime = 0.035f;
        [SerializeField] private float maxLifetime = 0.22f;
        [SerializeField] private float minWidth = 0.015f;
        [SerializeField] private float maxWidth = 0.12f;
        [SerializeField] private Color trailColor = new(0.25f, 0.9f, 1f, 1f);
        [SerializeField] private Color trailCoreColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.85f;

        private Rigidbody _rigidbody;
        private TrailRenderer _trailRenderer;
        private Material _trailMaterial;
        private Vector3 _previousPosition;
        private Vector3 _trackedVelocity;
        private float _trailIntensity;
        private bool _hasPreviousPosition;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            EnsureTrailAnchor();
            EnsureTrailRenderer();
            _previousPosition = transform.position;
            _hasPreviousPosition = true;
        }

        private void OnEnable()
        {
            if (_trailRenderer != null)
                _trailRenderer.Clear();
        }

        private void OnDestroy()
        {
            if (_trailMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(_trailMaterial);
            else
                DestroyImmediate(_trailMaterial);
        }

        private void LateUpdate()
        {
            float speed = ResolveSpeed();
            float targetIntensity = Mathf.InverseLerp(minVisibleSpeed, fullTrailSpeed, speed);
            targetIntensity = Mathf.SmoothStep(0f, 1f, targetIntensity);

            float ease = 1f - Mathf.Exp(-easeResponsiveness * Time.deltaTime);
            _trailIntensity = Mathf.Lerp(_trailIntensity, targetIntensity, ease);

            ApplyTrailVisuals(_trailIntensity);
        }

        private float ResolveSpeed()
        {
            Vector3 currentPosition = transform.position;
            if (_hasPreviousPosition)
                _trackedVelocity = (currentPosition - _previousPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

            _previousPosition = currentPosition;
            _hasPreviousPosition = true;

            Vector3 rigidbodyVelocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector3.zero;
            return Mathf.Max(rigidbodyVelocity.magnitude, _trackedVelocity.magnitude);
        }

        private void EnsureTrailAnchor()
        {
            if (trailAnchor != null)
                return;

            Transform existingAnchor = transform.Find("BladeTrailAnchor");
            if (existingAnchor != null)
            {
                trailAnchor = existingAnchor;
                return;
            }

            GameObject anchorObject = new("BladeTrailAnchor");
            trailAnchor = anchorObject.transform;
            trailAnchor.SetParent(transform, false);
            trailAnchor.localPosition = fallbackLocalAnchorPosition;
            trailAnchor.localRotation = Quaternion.identity;
            trailAnchor.localScale = Vector3.one;
        }

        private void EnsureTrailRenderer()
        {
            _trailRenderer = trailAnchor.GetComponent<TrailRenderer>();
            if (_trailRenderer == null)
                _trailRenderer = trailAnchor.gameObject.AddComponent<TrailRenderer>();

            _trailRenderer.emitting = true;
            _trailRenderer.autodestruct = false;
            _trailRenderer.numCornerVertices = 6;
            _trailRenderer.numCapVertices = 4;
            _trailRenderer.alignment = LineAlignment.View;
            _trailRenderer.textureMode = LineTextureMode.Stretch;
            _trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _trailRenderer.receiveShadows = false;
            _trailRenderer.minVertexDistance = 0.015f;
            _trailRenderer.widthCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.72f, 0.72f),
                new Keyframe(1f, 0f));

            _trailMaterial = CreateTrailMaterial();
            _trailRenderer.sharedMaterial = _trailMaterial;
            ApplyTrailVisuals(0f);
        }

        private void ApplyTrailVisuals(float intensity)
        {
            if (_trailRenderer == null)
                return;

            float alpha = maxAlpha * intensity;
            _trailRenderer.time = Mathf.Lerp(minLifetime, maxLifetime, intensity);
            _trailRenderer.startWidth = Mathf.Lerp(minWidth, maxWidth, intensity);
            _trailRenderer.endWidth = 0f;
            _trailRenderer.colorGradient = BuildGradient(alpha);
            _trailRenderer.emitting = intensity > 0.02f;

            if (_trailMaterial != null)
            {
                Color materialColor = Color.Lerp(trailColor, trailCoreColor, 0.35f);
                materialColor.a = alpha;
                _trailMaterial.color = materialColor;
            }
        }

        private Gradient BuildGradient(float alpha)
        {
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(trailCoreColor, 0f),
                    new GradientColorKey(trailColor, 0.28f),
                    new GradientColorKey(trailColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(alpha, 0f),
                    new GradientAlphaKey(alpha * 0.78f, 0.38f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Material CreateTrailMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                            Shader.Find("Universal Render Pipeline/Unlit") ??
                            Shader.Find("Sprites/Default");

            Material material = new(shader)
            {
                name = "SwordTrail_Runtime",
                color = Color.white
            };

            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 1f);
            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }
    }
}
