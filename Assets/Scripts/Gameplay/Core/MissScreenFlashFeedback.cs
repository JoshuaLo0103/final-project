using System.Collections;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class MissScreenFlashFeedback : MonoBehaviour
    {
        [SerializeField] private Color flashColor = new(1f, 0.04f, 0.02f, 1f);
        [SerializeField] private float maxAlpha = 0.42f;
        [SerializeField] private float flashDuration = 0.48f;
        [SerializeField] private float panelDistance = 0.55f;
        [SerializeField] private float edgeThickness = 0.18f;

        private readonly Renderer[] _panelRenderers = new Renderer[4];
        private readonly Transform[] _panelTransforms = new Transform[4];
        private Material _flashMaterial;
        private Transform _viewer;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            CreateFlashMaterial();
            TryCreatePanels();
            SetAlpha(0f);
        }

        private void OnEnable()
        {
            GameEvents.OnFruitMissed += HandleFruitMissed;
        }

        private void OnDisable()
        {
            GameEvents.OnFruitMissed -= HandleFruitMissed;

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }

            SetAlpha(0f);
        }

        private void HandleFruitMissed(FruitMissedEventArgs _)
        {
            if (!TryCreatePanels())
                return;

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private bool TryCreatePanels()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return false;

            if (_viewer == mainCamera.transform && _panelRenderers[0] != null)
            {
                LayoutPanels(mainCamera);
                return true;
            }

            _viewer = mainCamera.transform;
            for (int index = 0; index < _panelTransforms.Length; index++)
            {
                if (_panelTransforms[index] != null)
                    Destroy(_panelTransforms[index].gameObject);
            }

            CreatePanel(0, "MissFlashTop");
            CreatePanel(1, "MissFlashBottom");
            CreatePanel(2, "MissFlashLeft");
            CreatePanel(3, "MissFlashRight");
            LayoutPanels(mainCamera);
            return true;
        }

        private void CreatePanel(int index, string panelName)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = panelName;
            panel.transform.SetParent(_viewer, false);

            Collider panelCollider = panel.GetComponent<Collider>();
            if (panelCollider != null)
                Destroy(panelCollider);

            Renderer panelRenderer = panel.GetComponent<Renderer>();
            panelRenderer.sharedMaterial = _flashMaterial;
            panelRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            panelRenderer.receiveShadows = false;
            panelRenderer.enabled = false;

            _panelTransforms[index] = panel.transform;
            _panelRenderers[index] = panelRenderer;
        }

        private void LayoutPanels(Camera targetCamera)
        {
            float distance = Mathf.Max(targetCamera.nearClipPlane + 0.05f, panelDistance);
            float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            float width = height * Mathf.Max(0.1f, targetCamera.aspect);
            float thickness = Mathf.Clamp01(edgeThickness);
            float stripHeight = height * thickness;
            float stripWidth = width * thickness;

            SetPanel(0, new Vector3(0f, (height - stripHeight) * 0.5f, distance), new Vector3(width, stripHeight, 1f));
            SetPanel(1, new Vector3(0f, -(height - stripHeight) * 0.5f, distance), new Vector3(width, stripHeight, 1f));
            SetPanel(2, new Vector3(-(width - stripWidth) * 0.5f, 0f, distance), new Vector3(stripWidth, height, 1f));
            SetPanel(3, new Vector3((width - stripWidth) * 0.5f, 0f, distance), new Vector3(stripWidth, height, 1f));
        }

        private void SetPanel(int index, Vector3 localPosition, Vector3 localScale)
        {
            if (_panelTransforms[index] == null)
                return;

            _panelTransforms[index].localPosition = localPosition;
            _panelTransforms[index].localRotation = Quaternion.identity;
            _panelTransforms[index].localScale = localScale;
        }

        private IEnumerator FlashRoutine()
        {
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, flashDuration));
                float pulse = normalized < 0.5f
                    ? EaseOutCubic(normalized / 0.5f)
                    : 1f - EaseInCubic((normalized - 0.5f) / 0.5f);

                SetAlpha(maxAlpha * pulse);
                yield return null;
            }

            SetAlpha(0f);
            _flashRoutine = null;
        }

        private void CreateFlashMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Standard");

            _flashMaterial = new Material(shader);
            _flashMaterial.SetOverrideTag("RenderType", "Transparent");
            _flashMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _flashMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _flashMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            if (_flashMaterial.HasProperty("_Surface"))
                _flashMaterial.SetFloat("_Surface", 1f);
            if (_flashMaterial.HasProperty("_Cull"))
                _flashMaterial.SetFloat("_Cull", 0f);
            if (_flashMaterial.HasProperty("_ZWrite"))
                _flashMaterial.SetFloat("_ZWrite", 0f);

            SetAlpha(0f);
        }

        private void SetAlpha(float alpha)
        {
            if (_flashMaterial == null)
                return;

            Color color = flashColor;
            color.a = Mathf.Clamp01(alpha);
            _flashMaterial.color = color;

            if (_flashMaterial.HasProperty("_BaseColor"))
                _flashMaterial.SetColor("_BaseColor", color);
            if (_flashMaterial.HasProperty("_Color"))
                _flashMaterial.SetColor("_Color", color);

            bool shouldShow = color.a > 0.001f;
            foreach (Renderer panelRenderer in _panelRenderers)
            {
                if (panelRenderer != null)
                    panelRenderer.enabled = shouldShow;
            }
        }

        private static float EaseOutCubic(float value)
        {
            value = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - value, 3f);
        }

        private static float EaseInCubic(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * value;
        }
    }
}
