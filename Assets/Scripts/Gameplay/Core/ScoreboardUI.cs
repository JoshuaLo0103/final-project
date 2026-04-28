using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BladeFrenzy.Gameplay.Core
{
    public class ScoreboardUI : MonoBehaviour
    {
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

        private GameManager _gameManager;
        private ScoreManager _scoreManager;
        private DifficultyManager _difficultyManager;
        private LivesManager _livesManager;

        private Transform _viewer;
        private float _statusMessageTimer;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _placementInitialized;

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
            GameEvents.OnComboTierChanged += HandleComboTierChanged;
            GameEvents.OnHighScoreBeaten += HandleHighScoreBeaten;
            GameEvents.OnFruitMissed += HandleFruitMissed;
            GameEvents.OnLivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunEnded -= HandleRunEnded;
            GameEvents.OnComboTierChanged -= HandleComboTierChanged;
            GameEvents.OnHighScoreBeaten -= HandleHighScoreBeaten;
            GameEvents.OnFruitMissed -= HandleFruitMissed;
            GameEvents.OnLivesChanged -= HandleLivesChanged;
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
            SetStatus("Slice clean. Avoid bombs.", 0f);
            SetGameOverVisible(false, default, string.Empty);
        }

        private void HandleRunEnded(GameRunEndedEventArgs eventArgs)
        {
            SetGameOverVisible(true, eventArgs.Snapshot, eventArgs.EndReason);
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

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void SetStatus(string message, float duration)
        {
            if (statusText == null)
                return;

            statusText.text = message;
            _statusMessageTimer = duration;
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
