using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace BladeFrenzy.Gameplay.Core
{
    public class ScoreboardUI : MonoBehaviour
    {
        private const float MinimumGameOverDistance = 2.5f;
        private const float MinimumGameOverButtonActivationDelay = 3f;

        [Header("Scene References")]
        [SerializeField] private Canvas scoreboardCanvas;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text finalComboText;
        [SerializeField] private TMP_Text statusText;

        [Header("Bootstrap")]
        [SerializeField] private bool buildRuntimeHudIfMissing = true;

        [Header("Placement")]
        [SerializeField] private Vector3 offsetFromViewer = new(0f, 1.5f, 6.5f);
        [SerializeField] private Vector3 canvasScale = new(0.005f, 0.005f, 0.005f);
        [SerializeField] private float followLerpSpeed = 5f;
        [SerializeField] private bool followViewer = false;
        [SerializeField] private bool placeFromViewerOnStart = false;

        [Header("Game Over Placement")]
        [SerializeField] private bool placeGameOverInReach = true;
        [SerializeField] private Vector3 gameOverOffsetFromViewer = new(0f, -0.05f, 2.5f);
        [SerializeField] private Vector3 gameOverCanvasScale = new(0.005f, 0.005f, 0.005f);
        [SerializeField] private float gameOverButtonActivationDelay = 3f;

        [Header("Juicy Feedback")]
        [SerializeField] private float scorePunchScale = 1.28f;
        [SerializeField] private float scorePunchDuration = 0.22f;

        private GameManager _gameManager;
        private ScoreManager _scoreManager;
        private DifficultyManager _difficultyManager;
        private LivesManager _livesManager;

        private Transform _viewer;
        private float _statusMessageTimer;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _placementInitialized;
        private Vector3 _originalCanvasPosition;
        private Quaternion _originalCanvasRotation;
        private Vector3 _originalCanvasScale;
        private bool _hasOriginalCanvasPlacement;
        private bool _gameOverPlacementActive;
        private Coroutine _scorePunchRoutine;
        private Vector3 _scoreTextBaseScale = Vector3.one;
        private bool _hasScoreTextBaseScale;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _scoreManager = GetComponent<ScoreManager>();
            _difficultyManager = GetComponent<DifficultyManager>();
            _livesManager = GetComponent<LivesManager>();

            ApplySerializedReferences();

            if (scoreboardCanvas == null && buildRuntimeHudIfMissing)
                BuildRuntimeHud();
            else
                EnsureLivesDisplay();

            EnsureWorldSpaceUiInput();
            EnsureFeedbackElements();
            BindRuntimeButtons();
            CacheCanvasPlacement();

            FindViewer();
            if (placeFromViewerOnStart)
                InitializePlacement();
            RefreshHud();
            SetGameOverVisible(false, default, string.Empty);
        }

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunEnded += HandleRunEnded;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnComboTierChanged += HandleComboTierChanged;
            GameEvents.OnHighScoreBeaten += HandleHighScoreBeaten;
            GameEvents.OnFruitMissed += HandleFruitMissed;
            GameEvents.OnLivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunEnded -= HandleRunEnded;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnComboTierChanged -= HandleComboTierChanged;
            GameEvents.OnHighScoreBeaten -= HandleHighScoreBeaten;
            GameEvents.OnFruitMissed -= HandleFruitMissed;
            GameEvents.OnLivesChanged -= HandleLivesChanged;

            StopFeedbackAnimations();
        }

        private void Start()
        {
            EnsureWorldSpaceUiInput();
            EnsureFeedbackElements();
            BindRuntimeButtons();
            CacheCanvasPlacement();
        }

        private void LateUpdate()
        {
            if (scoreboardCanvas == null)
                return;

            if (_viewer == null)
                FindViewer();

            if (placeFromViewerOnStart && !_placementInitialized)
                InitializePlacement();

            if (_viewer != null && followViewer)
            {
                Transform canvasTransform = scoreboardCanvas.transform;
                _targetPosition = _viewer.position
                    + _viewer.forward * offsetFromViewer.z
                    + Vector3.up * offsetFromViewer.y
                    + _viewer.right * offsetFromViewer.x;

                Vector3 lookDirection = _targetPosition - _viewer.position;
                if (lookDirection.sqrMagnitude > 0.0001f)
                    _targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

                float t = 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime);
                canvasTransform.position = Vector3.Lerp(canvasTransform.position, _targetPosition, t);
                canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, _targetRotation, t);
            }

            if (_statusMessageTimer > 0f)
                _statusMessageTimer = Mathf.Max(0f, _statusMessageTimer - Time.deltaTime);
            else if (_gameManager != null && _gameManager.IsRunActive && statusText != null)
                statusText.text = "Slice clean. Avoid bombs.";

            RefreshHud();
        }

        private void BuildRuntimeHud()
        {
            EnsureEventSystem();

            GameObject canvasObject = new("ScoreboardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            scoreboardCanvas = canvasObject.GetComponent<Canvas>();
            scoreboardCanvas.renderMode = RenderMode.WorldSpace;
            scoreboardCanvas.transform.localScale = canvasScale;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(920f, 540f);

            Image panel = CreatePanel("Panel", canvasObject.transform, new Color(0.03f, 0.05f, 0.08f, 0.78f));
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(18f, 18f);
            panelRect.offsetMax = new Vector2(-18f, -18f);

            CreateText("TitleText", panel.transform, "BLADE FRENZY", 42, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(820f, 54f), out _, new Color(0.97f, 0.98f, 1f));
            CreateText("StatusText", panel.transform, "Slice to survive.", 22, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(820f, 36f), out statusText, new Color(0.7f, 0.84f, 0.93f));

            CreateMetricCard("ScoreCard", panel.transform, new Vector2(235f, 86f), new Vector2(-210f, -152f), "SCORE", out scoreText, Color.white);
            CreateMetricCard("ComboCard", panel.transform, new Vector2(235f, 86f), new Vector2(0f, -152f), "COMBO", out comboText, new Color(1f, 0.84f, 0.28f));
            CreateMetricCard("MultiplierCard", panel.transform, new Vector2(235f, 86f), new Vector2(210f, -152f), "MULTIPLIER", out multiplierText, new Color(1f, 0.5f, 0.25f));

            CreateMetricCard("HighScoreCard", panel.transform, new Vector2(235f, 86f), new Vector2(-210f, -258f), "HIGH SCORE", out highScoreText, new Color(0.76f, 0.98f, 0.86f));
            CreateMetricCard("TimerCard", panel.transform, new Vector2(235f, 86f), new Vector2(0f, -258f), "TIME", out timerText, new Color(0.8f, 0.91f, 1f));
            CreateMetricCard("DifficultyCard", panel.transform, new Vector2(235f, 86f), new Vector2(210f, -258f), "DIFFICULTY", out difficultyText, new Color(1f, 0.67f, 0.34f));

            CreateText(
                "LivesText",
                panel.transform,
                string.Empty,
                48,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -372f),
                new Vector2(820f, 64f),
                out livesText,
                new Color(1f, 0.45f, 0.45f));

            BuildGameOverPanel(panel.transform);
            ApplySerializedReferences();
            EnsureWorldSpaceUiInput();
            EnsureFeedbackElements();
        }

        private void EnsureLivesDisplay()
        {
            if (livesText != null || scoreboardCanvas == null)
                return;

            Transform panel = scoreboardCanvas.transform.Find("Panel");
            if (panel == null)
                panel = scoreboardCanvas.transform;

            RectTransform canvasRect = scoreboardCanvas.GetComponent<RectTransform>();
            if (canvasRect != null && canvasRect.sizeDelta.y < 540f)
                canvasRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x, 540f);

            CreateText(
                "LivesText",
                panel,
                string.Empty,
                48,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -372f),
                new Vector2(820f, 64f),
                out livesText,
                new Color(1f, 0.45f, 0.45f));
        }

        private void BuildGameOverPanel(Transform parent)
        {
            gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(parent, false);

            RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(680f, 210f);
            panelRect.anchoredPosition = new Vector2(0f, 120f);

            Image panelImage = gameOverPanel.GetComponent<Image>();
            panelImage.color = new Color(0.11f, 0.04f, 0.04f, 0.92f);

            CreateText("GameOverTitle", gameOverPanel.transform, "RUN OVER", 38, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(560f, 48f), out _);
            CreateText("FinalScoreText", gameOverPanel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(540f, 34f), out finalScoreText);
            CreateText("FinalComboText", gameOverPanel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(540f, 34f), out finalComboText);

            CreateButton("RestartButton", gameOverPanel.transform, "Restart", new Vector2(-110f, -162f), () => _gameManager?.RestartRun());
            CreateButton("QuitButton", gameOverPanel.transform, "Quit", new Vector2(110f, -162f), () => _gameManager?.QuitGame());
        }

        private void RefreshHud()
        {
            if (_scoreManager == null || _gameManager == null || _difficultyManager == null)
                return;

            if (scoreText != null)
                scoreText.text = _scoreManager.Score.ToString();
            if (comboText != null)
                comboText.text = _scoreManager.ComboCount.ToString();
            if (multiplierText != null)
                multiplierText.text = $"{_scoreManager.Multiplier}x";
            if (highScoreText != null)
                highScoreText.text = _scoreManager.HighScore.ToString();
            if (livesText != null && _livesManager != null)
                livesText.text = $"Lives  {BuildLivesString(_livesManager.CurrentLives, _livesManager.MaxLives)}";
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(_gameManager.RemainingTime).ToString();
            if (difficultyText != null)
                difficultyText.text = _difficultyManager.CurrentTierLabel;
        }

        private void SetGameOverVisible(bool visible, ScoreSnapshot snapshot, string reason)
        {
            if (gameOverPanel == null)
                return;

            gameOverPanel.SetActive(visible);
            if (!visible)
                return;

            if (statusText != null)
                statusText.text = reason;
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {snapshot.Score}    High Score: {snapshot.HighScore}";
            if (finalComboText != null)
                finalComboText.text = $"Max Combo: {snapshot.MaxCombo}    Final Multiplier: {snapshot.Multiplier}x";
        }

        private void HandleRunStarted()
        {
            RestoreCanvasPlacement();
            SetStatus("Slice clean. Avoid bombs.", 0f);
            SetGameOverVisible(false, default, string.Empty);
        }

        private void HandleRunEnded(GameRunEndedEventArgs eventArgs)
        {
            PlaceGameOverCanvasInReach();
            SetGameOverVisible(true, eventArgs.Snapshot, eventArgs.EndReason);
            SuppressSwordButtonActivation(Mathf.Max(MinimumGameOverButtonActivationDelay, gameOverButtonActivationDelay));
        }

        private void HandleScoreChanged(ScoreChangedEventArgs eventArgs)
        {
            if (scoreText != null)
                scoreText.text = eventArgs.Score.ToString();

            if (eventArgs.PointsAdded > 0)
                PlayScorePunch();
        }

        private void HandleComboTierChanged(ComboTierChangedEventArgs eventArgs)
        {
            if (eventArgs.Multiplier <= 1)
                return;

            SetStatus($"Combo tier up: {eventArgs.Multiplier}x", 1.5f);
        }

        private void HandleHighScoreBeaten(HighScoreBeatenEventArgs eventArgs)
        {
            SetStatus($"New high score: {eventArgs.NewHighScore}", 1.5f);
        }

        private void HandleFruitMissed(FruitMissedEventArgs _)
        {
            SetStatus("Combo dropped on miss.", 1.2f);
        }

        private void HandleLivesChanged(LivesChangedEventArgs eventArgs)
        {
            if (livesText != null)
                livesText.text = $"Lives  {BuildLivesString(eventArgs.CurrentLives, eventArgs.MaxLives)}";

            if (eventArgs.CurrentLives > 0)
                SetStatus($"{eventArgs.CurrentLives} lives remaining.", 1.1f);
        }

        private void FindViewer()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _viewer = mainCamera.transform;
                return;
            }

            _viewer = FindFirstObjectByType<Camera>()?.transform;
        }

        private void InitializePlacement()
        {
            if (scoreboardCanvas == null || _viewer == null)
                return;

            Transform canvasTransform = scoreboardCanvas.transform;
            _targetPosition = _viewer.position
                + _viewer.forward * offsetFromViewer.z
                + Vector3.up * offsetFromViewer.y
                + _viewer.right * offsetFromViewer.x;

            Vector3 flatForward = Vector3.ProjectOnPlane(_viewer.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude <= 0.0001f)
                flatForward = Vector3.forward;

            _targetRotation = Quaternion.LookRotation(flatForward, Vector3.up);
            canvasTransform.position = _targetPosition;
            canvasTransform.rotation = _targetRotation;
            _placementInitialized = true;
        }

        private void CacheCanvasPlacement()
        {
            if (_hasOriginalCanvasPlacement || scoreboardCanvas == null)
                return;

            Transform canvasTransform = scoreboardCanvas.transform;
            _originalCanvasPosition = canvasTransform.position;
            _originalCanvasRotation = canvasTransform.rotation;
            _originalCanvasScale = canvasTransform.localScale;
            _hasOriginalCanvasPlacement = true;
        }

        private void RestoreCanvasPlacement()
        {
            if (!_gameOverPlacementActive || !_hasOriginalCanvasPlacement || scoreboardCanvas == null)
                return;

            Transform canvasTransform = scoreboardCanvas.transform;
            canvasTransform.position = _originalCanvasPosition;
            canvasTransform.rotation = _originalCanvasRotation;
            canvasTransform.localScale = _originalCanvasScale;
            _gameOverPlacementActive = false;
        }

        private void PlaceGameOverCanvasInReach()
        {
            if (!placeGameOverInReach || scoreboardCanvas == null)
                return;

            if (_viewer == null)
                FindViewer();

            if (_viewer == null)
                return;

            CacheCanvasPlacement();

            Vector3 flatForward = Vector3.ProjectOnPlane(_viewer.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude <= 0.0001f)
                flatForward = _viewer.forward.sqrMagnitude > 0.0001f ? _viewer.forward.normalized : Vector3.forward;

            Vector3 flatRight = Vector3.ProjectOnPlane(_viewer.right, Vector3.up).normalized;
            if (flatRight.sqrMagnitude <= 0.0001f)
                flatRight = Vector3.right;

            Transform canvasTransform = scoreboardCanvas.transform;
            float forwardDistance = Mathf.Max(MinimumGameOverDistance, Mathf.Abs(gameOverOffsetFromViewer.z));
            canvasTransform.position = _viewer.position
                + flatRight * gameOverOffsetFromViewer.x
                + Vector3.up * gameOverOffsetFromViewer.y
                + flatForward * forwardDistance;
            canvasTransform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
            canvasTransform.localScale = gameOverCanvasScale;
            _gameOverPlacementActive = true;
        }

        private static void EnsureEventSystem()
        {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            EventSystem eventSystem = null;
            foreach (EventSystem candidate in eventSystems)
            {
                if (candidate.GetComponent<XRUIInputModule>() != null)
                {
                    eventSystem = candidate;
                    break;
                }
            }

            if (eventSystem == null && eventSystems.Length > 0)
                eventSystem = eventSystems[0];

            if (eventSystem == null)
            {
                GameObject eventSystemObject = new("EventSystem", typeof(EventSystem));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            eventSystem.gameObject.SetActive(true);
            eventSystem.enabled = true;

            XRUIInputModule xrInputModule = eventSystem.GetComponent<XRUIInputModule>();
            if (xrInputModule == null)
                xrInputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();

            xrInputModule.enableXRInput = true;
            xrInputModule.enableMouseInput = true;
            xrInputModule.enableTouchInput = true;
            EventSystem.current = eventSystem;

            foreach (BaseInputModule inputModule in eventSystem.GetComponents<BaseInputModule>())
            {
                if (inputModule != xrInputModule)
                    inputModule.enabled = false;
            }

            foreach (EventSystem duplicate in eventSystems)
            {
                if (duplicate == eventSystem)
                    continue;

                foreach (BaseInputModule inputModule in duplicate.GetComponents<BaseInputModule>())
                    inputModule.enabled = false;

                duplicate.enabled = false;
                duplicate.gameObject.SetActive(false);
            }
        }

        private void SetStatus(string message, float duration)
        {
            if (statusText == null)
                return;

            statusText.text = message;
            _statusMessageTimer = duration;
        }

        private void EnsureFeedbackElements()
        {
            CaptureScoreTextBaseScale();
        }

        private void CaptureScoreTextBaseScale()
        {
            if (_hasScoreTextBaseScale || scoreText == null)
                return;

            _scoreTextBaseScale = scoreText.transform.localScale;
            _hasScoreTextBaseScale = true;
        }

        private void PlayScorePunch()
        {
            if (scoreText == null)
                return;

            CaptureScoreTextBaseScale();

            if (_scorePunchRoutine != null)
                StopCoroutine(_scorePunchRoutine);

            _scorePunchRoutine = StartCoroutine(AnimateScorePunch());
        }

        private IEnumerator AnimateScorePunch()
        {
            Transform scoreTransform = scoreText.transform;
            float duration = Mathf.Max(0.05f, scorePunchDuration);
            float halfDuration = duration * 0.5f;
            float elapsed = 0f;
            Vector3 peakScale = _scoreTextBaseScale * Mathf.Max(1f, scorePunchScale);

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / halfDuration));
                scoreTransform.localScale = Vector3.LerpUnclamped(_scoreTextBaseScale, peakScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / halfDuration));
                scoreTransform.localScale = Vector3.LerpUnclamped(peakScale, _scoreTextBaseScale, t);
                yield return null;
            }

            scoreTransform.localScale = _scoreTextBaseScale;
            _scorePunchRoutine = null;
        }

        private void StopFeedbackAnimations()
        {
            if (_scorePunchRoutine != null)
            {
                StopCoroutine(_scorePunchRoutine);
                _scorePunchRoutine = null;
            }

            if (scoreText != null && _hasScoreTextBaseScale)
                scoreText.transform.localScale = _scoreTextBaseScale;
        }

        private static float EaseOutCubic(float value)
        {
            float inverted = 1f - value;
            return 1f - inverted * inverted * inverted;
        }

        private void ApplySerializedReferences()
        {
            // When this component is authored as a scene object/prefab, use its serialized UI references.
            // The runtime-generated path still assigns the same fields programmatically.
        }

        private static Image CreatePanel(string objectName, Transform parent, Color color)
        {
            GameObject panelObject = new(objectName, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void CreateMetricCard(
            string objectName,
            Transform parent,
            Vector2 size,
            Vector2 anchoredPosition,
            string label,
            out TMP_Text valueText,
            Color accentColor)
        {
            Image card = CreatePanel(objectName, parent, new Color(0.09f, 0.12f, 0.16f, 0.9f));
            RectTransform cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 1f);
            cardRect.anchorMax = new Vector2(0.5f, 1f);
            cardRect.sizeDelta = size;
            cardRect.anchoredPosition = anchoredPosition;

            CreateText($"{objectName}_Label", card.transform, label, 18, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(size.x - 20f, 24f), out _, new Color(0.63f, 0.72f, 0.82f));
            CreateText($"{objectName}_Value", card.transform, "0", 34, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(size.x - 24f, 40f), out valueText, accentColor);
        }

        private static void CreateText(
            string objectName,
            Transform parent,
            string text,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            out TMP_Text label,
            Color? color = null)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI textLabel = textObject.GetComponent<TextMeshProUGUI>();
            textLabel.text = text;
            textLabel.fontSize = fontSize;
            textLabel.fontStyle = fontStyle;
            textLabel.alignment = alignment;
            textLabel.color = color ?? Color.white;

            label = textLabel;
        }

        private static void CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.sizeDelta = new Vector2(180f, 54f);
            buttonRect.anchoredPosition = anchoredPosition;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.86f, 0.31f, 0.19f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);

            CreateText("Label", buttonObject.transform, label, 28, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 40f), out TMP_Text _);
        }

        private void EnsureWorldSpaceUiInput()
        {
            EnsureEventSystem();

            if (scoreboardCanvas == null)
                return;

            if (scoreboardCanvas.worldCamera == null)
                scoreboardCanvas.worldCamera = Camera.main;

            if (scoreboardCanvas.GetComponent<GraphicRaycaster>() == null)
                scoreboardCanvas.gameObject.AddComponent<GraphicRaycaster>();

            if (scoreboardCanvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                scoreboardCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        private void BindRuntimeButtons()
        {
            BindButton("RestartButton", HandleRestartClicked);
            BindButton("QuitButton", HandleQuitClicked);
        }

        private void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
        {
            Transform buttonTransform = gameOverPanel != null
                ? gameOverPanel.transform.Find(buttonName)
                : transform.Find(buttonName);

            Button button = buttonTransform != null
                ? buttonTransform.GetComponent<Button>()
                : null;

            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
            EnsureSwordButtonActivation(button);
        }

        private static void EnsureSwordButtonActivation(Button button)
        {
            SwordUiButtonActivator activator = button.GetComponent<SwordUiButtonActivator>();
            if (activator == null)
                activator = button.gameObject.AddComponent<SwordUiButtonActivator>();

            activator.Configure(button);
        }

        private void SuppressSwordButtonActivation(float seconds)
        {
            if (gameOverPanel == null)
                return;

            SwordUiButtonActivator[] activators = gameOverPanel.GetComponentsInChildren<SwordUiButtonActivator>(true);
            foreach (SwordUiButtonActivator activator in activators)
                activator.SuppressActivationFor(seconds);
        }

        private void HandleRestartClicked()
        {
            _gameManager?.RestartRun();
        }

        private void HandleQuitClicked()
        {
            _gameManager?.QuitGame();
        }

        private static string BuildLivesString(int currentLives, int maxLives)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int index = 0; index < maxLives; index++)
            {
                if (index > 0)
                    builder.Append(' ');

                builder.Append(index < currentLives ? '\u2665' : '\u2661');
            }

            return builder.ToString();
        }
    }
}
