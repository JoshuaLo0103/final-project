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
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text finalCoinText;
        [SerializeField] private TMP_Text finalComboText;
        [SerializeField] private TMP_Text finalHighScoreText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text comboPopupText;

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
        [SerializeField] private AudioClip comboChimeClip;
        [SerializeField, Range(0f, 1f)] private float comboChimeVolume = 1f;
        [SerializeField] private float comboChimeMinDistance = 8f;
        [SerializeField] private float comboChimeMaxDistance = 30f;
        [SerializeField] private float comboPopupDuration = 1.6f;
        [SerializeField] private Vector2 comboPopupAnchoredPosition = new(0f, 60f);
        [SerializeField] private Vector2 comboPopupTravel = new(0f, 70f);
        [SerializeField] private Color comboPopupColor = new(1f, 0.82f, 0.24f, 1f);
        [SerializeField] private float comboPopupFontSize = 96f;
        [SerializeField] private Vector2 comboPopupSize = new(820f, 140f);
        [SerializeField] private float highScoreFlashDuration = 1.8f;
        [SerializeField] private int highScoreFlashCount = 3;
        [SerializeField] private Color highScoreFlashColor = new(1f, 0.84f, 0.18f, 0.95f);
        [SerializeField] private Color highScorePopupColor = new(1f, 0.95f, 0.45f, 1f);
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField, Range(0f, 1f)] private float gameOverVolume = 1f;
        [SerializeField] private float gameOverMinDistance = 14f;
        [SerializeField] private float gameOverMaxDistance = 60f;
        [SerializeField] private float gameOverPanelScaleDuration = 0.5f;
        [SerializeField] private float gameOverStatCountDuration = 0.75f;
        [SerializeField] private float gameOverStatStaggerDelay = 0.18f;
        [SerializeField] private AudioClip[] difficultyTierClips = new AudioClip[4];
        [SerializeField, Range(0f, 1.5f)] private float difficultyTierVolume = 1f;
        [SerializeField] private float difficultyTierMinDistance = 10f;
        [SerializeField] private float difficultyTierMaxDistance = 45f;
        [SerializeField] private float difficultyTierPopupDuration = 1.8f;
        [SerializeField] private float difficultyTierPopupSlideDistance = 480f;
        [SerializeField] private Vector2 difficultyTierPopupAnchoredPosition = new(0f, -10f);
        [SerializeField] private Vector2 difficultyTierPopupSize = new(900f, 150f);
        [SerializeField] private float difficultyTierPopupFontSize = 110f;
        [SerializeField] private Color difficultyTierEasyColor = new(0.6f, 0.95f, 0.7f, 1f);
        [SerializeField] private Color difficultyTierMediumColor = new(0.95f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color difficultyTierHardColor = new(1f, 0.55f, 0.25f, 1f);
        [SerializeField] private Color difficultyTierFrenzyColor = new(1f, 0.3f, 0.45f, 1f);
        [SerializeField] private float gameOverStatHighlightScale = 1.35f;
        [SerializeField] private Color gameOverStatHighlightColor = new(1f, 0.86f, 0.32f, 1f);
        [SerializeField] private int highScoreParticleCount = 140;
        [SerializeField] private float highScoreParticleLifetime = 2.4f;
        [SerializeField] private float highScoreParticleSpeed = 4.2f;
        [SerializeField] private float highScoreParticleSize = 0.11f;
        [SerializeField] private float highScoreParticleSizeVariation = 0.05f;
        [SerializeField] private float highScoreParticleGravity = 1.15f;
        [SerializeField] private float highScoreParticleBurstRadius = 0.18f;
        [SerializeField] private Vector3 highScoreParticleOffset = new(0f, -0.05f, 0.1f);
        [SerializeField] private Color highScoreParticleColorPrimary = new(1f, 0.86f, 0.22f, 1f);
        [SerializeField] private Color highScoreParticleColorSecondary = new(1f, 0.97f, 0.55f, 1f);
        [SerializeField] private Color highScoreParticleColorAccent = new(1f, 0.55f, 0.1f, 1f);

        private GameManager _gameManager;
        private ScoreManager _scoreManager;
        private DifficultyManager _difficultyManager;
        private LivesManager _livesManager;
        private CoinManager _coinManager;

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
        private AudioSource _comboChimeSource;
        private Coroutine _comboPopupRoutine;
        private static AudioClip s_generatedComboChime;
        private AudioSource _gameOverSource;
        private static AudioClip s_generatedGameOverSting;
        private AudioSource _difficultyTierSource;
        private static readonly AudioClip[] s_generatedDifficultyTierClips = new AudioClip[4];
        private TMP_Text _difficultyTierPopupText;
        private Coroutine _difficultyTierPopupRoutine;
        private TMP_Text _highScorePopupText;
        private Image _panelImage;
        private Color _panelOriginalColor;
        private bool _hasPanelOriginalColor;
        private Coroutine _highScoreFlashRoutine;
        private Coroutine _highScorePopupRoutine;
        private Coroutine _gameOverIntroRoutine;
        private Vector3 _gameOverPanelOriginalScale = Vector3.one;
        private bool _hasGameOverPanelScale;
        private ParticleSystem _highScoreParticles;
        private Material _highScoreParticleMaterial;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _scoreManager = GetComponent<ScoreManager>();
            _difficultyManager = GetComponent<DifficultyManager>();
            _livesManager = GetComponent<LivesManager>();
            _coinManager = GetComponent<CoinManager>();

            ApplySerializedReferences();

            if (scoreboardCanvas == null && buildRuntimeHudIfMissing)
                BuildRuntimeHud();
            else
            {
                EnsureLivesDisplay();
                EnsureCoinDisplay();
            }

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
            GameEvents.OnDifficultyTierChanged += HandleDifficultyTierChanged;
            GameEvents.OnHighScoreBeaten += HandleHighScoreBeaten;
            GameEvents.OnFruitMissed += HandleFruitMissed;
            GameEvents.OnLivesChanged += HandleLivesChanged;
            GameEvents.OnCoinCollected += HandleCoinCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunEnded -= HandleRunEnded;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnComboTierChanged -= HandleComboTierChanged;
            GameEvents.OnDifficultyTierChanged -= HandleDifficultyTierChanged;
            GameEvents.OnHighScoreBeaten -= HandleHighScoreBeaten;
            GameEvents.OnFruitMissed -= HandleFruitMissed;
            GameEvents.OnLivesChanged -= HandleLivesChanged;
            GameEvents.OnCoinCollected -= HandleCoinCollected;

            StopFeedbackAnimations();
        }

        private void OnDestroy()
        {
            if (_highScoreParticleMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(_highScoreParticleMaterial);
            else
                DestroyImmediate(_highScoreParticleMaterial);

            _highScoreParticleMaterial = null;
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
            canvasRect.sizeDelta = new Vector2(920f, 620f);

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

            CreateText(
                "CoinText",
                panel.transform,
                string.Empty,
                36,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -438f),
                new Vector2(820f, 48f),
                out coinText,
                new Color(1f, 0.82f, 0.2f));

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

        private void EnsureCoinDisplay()
        {
            if (coinText != null || scoreboardCanvas == null)
                return;

            Transform panel = scoreboardCanvas.transform.Find("Panel");
            if (panel == null)
                panel = scoreboardCanvas.transform;

            Transform existing = panel.Find("CoinText");
            if (existing != null)
            {
                coinText = existing.GetComponent<TMP_Text>();
                if (coinText != null)
                    return;
            }

            RectTransform canvasRect = scoreboardCanvas.GetComponent<RectTransform>();
            if (canvasRect != null && canvasRect.sizeDelta.y < 620f)
                canvasRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x, 620f);

            CreateText(
                "CoinText",
                panel,
                string.Empty,
                36,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -438f),
                new Vector2(820f, 48f),
                out coinText,
                new Color(1f, 0.82f, 0.2f));
        }

        private void BuildGameOverPanel(Transform parent)
        {
            gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(parent, false);

            RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(680f, 280f);
            panelRect.anchoredPosition = new Vector2(0f, 140f);

            Image panelImage = gameOverPanel.GetComponent<Image>();
            panelImage.color = new Color(0.11f, 0.04f, 0.04f, 0.92f);

            CreateText("GameOverTitle", gameOverPanel.transform, "RUN OVER", 38, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(560f, 48f), out _);
            CreateText("FinalScoreText", gameOverPanel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(540f, 34f), out finalScoreText);
            CreateText("FinalCoinText", gameOverPanel.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -114f), new Vector2(540f, 32f), out finalCoinText, new Color(1f, 0.82f, 0.2f));
            CreateText("FinalComboText", gameOverPanel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(540f, 34f), out finalComboText);
            CreateText("FinalHighScoreText", gameOverPanel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(540f, 34f), out finalHighScoreText);

            CreateButton("RestartButton", gameOverPanel.transform, "Restart", new Vector2(-110f, -248f), () => _gameManager?.RestartRun());
            CreateButton("QuitButton", gameOverPanel.transform, "Quit", new Vector2(110f, -248f), () => _gameManager?.QuitGame());
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
            if (coinText != null)
            {
                if (_coinManager == null)
                    _coinManager = GetComponent<CoinManager>();
                coinText.text = $"Coins  {(_coinManager != null ? _coinManager.CurrentCoins : 0)}";
            }
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(_gameManager.RemainingTime).ToString();
            if (difficultyText != null)
                difficultyText.text = _difficultyManager.CurrentTierLabel;
        }

        private void SetGameOverVisible(bool visible, ScoreSnapshot snapshot, string reason)
        {
            if (gameOverPanel == null)
                return;

            EnsureGameOverStatLayout();
            CaptureGameOverPanelScale();

            if (_gameOverIntroRoutine != null)
            {
                StopCoroutine(_gameOverIntroRoutine);
                _gameOverIntroRoutine = null;
            }

            gameOverPanel.SetActive(visible);

            if (!visible)
            {
                if (_hasGameOverPanelScale)
                    gameOverPanel.transform.localScale = _gameOverPanelOriginalScale;
                if (finalScoreText != null)
                    finalScoreText.text = string.Empty;
                if (finalCoinText != null)
                    finalCoinText.text = string.Empty;
                if (finalComboText != null)
                    finalComboText.text = string.Empty;
                if (finalHighScoreText != null)
                    finalHighScoreText.text = string.Empty;
                return;
            }

            if (statusText != null)
                statusText.text = reason;

            if (finalScoreText != null)
                finalScoreText.text = "Final Score: 0";
            if (finalCoinText != null)
                finalCoinText.text = "Coins: 0 (+0)";
            if (finalComboText != null)
                finalComboText.text = "Max Combo: 0";
            if (finalHighScoreText != null)
                finalHighScoreText.text = "High Score: 0";

            _gameOverIntroRoutine = StartCoroutine(AnimateGameOverIntro(snapshot));
        }

        private void CaptureGameOverPanelScale()
        {
            if (_hasGameOverPanelScale || gameOverPanel == null)
                return;

            Vector3 current = gameOverPanel.transform.localScale;
            _gameOverPanelOriginalScale = current.sqrMagnitude > 0.0001f ? current : Vector3.one;
            _hasGameOverPanelScale = true;
        }

        private void EnsureGameOverStatLayout()
        {
            if (gameOverPanel == null)
                return;

            Transform panelTransform = gameOverPanel.transform;

            if (finalScoreText == null)
            {
                Transform existing = panelTransform.Find("FinalScoreText");
                if (existing != null)
                    finalScoreText = existing.GetComponent<TMP_Text>();
            }

            if (finalComboText == null)
            {
                Transform existing = panelTransform.Find("FinalComboText");
                if (existing != null)
                    finalComboText = existing.GetComponent<TMP_Text>();
            }

            if (finalCoinText == null)
            {
                Transform existing = panelTransform.Find("FinalCoinText");
                if (existing != null)
                    finalCoinText = existing.GetComponent<TMP_Text>();
            }

            if (finalCoinText == null)
            {
                CreateText(
                    "FinalCoinText",
                    panelTransform,
                    string.Empty,
                    24,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -114f),
                    new Vector2(540f, 32f),
                    out finalCoinText,
                    new Color(1f, 0.82f, 0.2f));
            }

            if (finalHighScoreText == null)
            {
                Transform existing = panelTransform.Find("FinalHighScoreText");
                if (existing != null)
                    finalHighScoreText = existing.GetComponent<TMP_Text>();
            }

            if (finalHighScoreText == null)
            {
                CreateText(
                    "FinalHighScoreText",
                    panelTransform,
                    string.Empty,
                    26,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -158f),
                    new Vector2(540f, 34f),
                    out finalHighScoreText);
            }

            RectTransform panelRect = (RectTransform)panelTransform;
            const float requiredHeight = 330f;
            if (panelRect.sizeDelta.y < requiredHeight)
            {
                Vector2 size = panelRect.sizeDelta;
                size.y = requiredHeight;
                panelRect.sizeDelta = size;
            }

            SetTopAnchoredY(finalScoreText, -80f);
            SetTopAnchoredY(finalCoinText, -114f);
            SetTopAnchoredY(finalComboText, -148f);
            SetTopAnchoredY(finalHighScoreText, -184f);
            SetTopAnchoredY(panelTransform.Find("RestartButton") as RectTransform, -248f);
            SetTopAnchoredY(panelTransform.Find("QuitButton") as RectTransform, -248f);
        }

        private static void SetTopAnchoredY(TMP_Text text, float y)
        {
            if (text == null)
                return;
            SetTopAnchoredY((RectTransform)text.transform, y);
        }

        private static void SetTopAnchoredY(RectTransform rect, float y)
        {
            if (rect == null)
                return;
            Vector2 pos = rect.anchoredPosition;
            pos.y = y;
            rect.anchoredPosition = pos;
        }

        private IEnumerator AnimateGameOverIntro(ScoreSnapshot snapshot)
        {
            Transform panelTransform = gameOverPanel.transform;
            Vector3 targetScale = _hasGameOverPanelScale ? _gameOverPanelOriginalScale : Vector3.one;

            float scaleDuration = Mathf.Max(0.05f, gameOverPanelScaleDuration);
            float elapsed = 0f;
            panelTransform.localScale = Vector3.zero;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(Mathf.Clamp01(elapsed / scaleDuration));
                panelTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, t);
                yield return null;
            }
            panelTransform.localScale = targetScale;

            if (gameOverStatStaggerDelay > 0f)
                yield return new WaitForSeconds(gameOverStatStaggerDelay);

            if (finalScoreText != null)
                yield return CountUpStat(
                    finalScoreText,
                    snapshot.Score,
                    value => finalScoreText.text = $"Final Score: {value}");

            if (gameOverStatStaggerDelay > 0f)
                yield return new WaitForSeconds(gameOverStatStaggerDelay);

            if (finalCoinText != null)
            {
                int bonusPerCoin = snapshot.CoinCount > 0
                    ? Mathf.RoundToInt(snapshot.CoinBonusPoints / (float)snapshot.CoinCount)
                    : 0;
                yield return CountUpStat(
                    finalCoinText,
                    snapshot.CoinCount,
                    value => finalCoinText.text = $"Coins: {value} (+{value * bonusPerCoin})");
            }

            if (gameOverStatStaggerDelay > 0f)
                yield return new WaitForSeconds(gameOverStatStaggerDelay);

            if (finalComboText != null)
                yield return CountUpStat(
                    finalComboText,
                    snapshot.MaxCombo,
                    value => finalComboText.text = $"Max Combo: {value}");

            if (gameOverStatStaggerDelay > 0f)
                yield return new WaitForSeconds(gameOverStatStaggerDelay);

            if (finalHighScoreText != null)
                yield return CountUpStat(
                    finalHighScoreText,
                    snapshot.HighScore,
                    value => finalHighScoreText.text = $"High Score: {value}");

            _gameOverIntroRoutine = null;
        }

        private IEnumerator CountUpStat(TMP_Text statText, int target, System.Action<int> apply)
        {
            float duration = Mathf.Max(0.05f, gameOverStatCountDuration);
            Transform statTransform = statText.transform;
            Vector3 baseScale = statTransform.localScale;
            Color baseColor = statText.color;
            Vector3 peakScale = baseScale * Mathf.Max(1f, gameOverStatHighlightScale);
            float scaleInPhase = Mathf.Min(0.18f, duration * 0.25f);
            float scaleOutPhase = Mathf.Min(0.22f, duration * 0.3f);
            float holdEnd = Mathf.Max(scaleInPhase, duration - scaleOutPhase);

            float elapsed = 0f;
            int lastValue = -1;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float clamped = Mathf.Clamp01(elapsed / duration);
                int value = Mathf.RoundToInt(Mathf.LerpUnclamped(0f, target, EaseOutCubic(clamped)));
                if (value != lastValue)
                {
                    apply(value);
                    lastValue = value;
                }

                Vector3 currentScale;
                float colorBlend;
                if (elapsed <= scaleInPhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / scaleInPhase));
                    currentScale = Vector3.LerpUnclamped(baseScale, peakScale, t);
                    colorBlend = t;
                }
                else if (elapsed >= holdEnd)
                {
                    float t = EaseOutCubic(Mathf.Clamp01((elapsed - holdEnd) / Mathf.Max(0.0001f, scaleOutPhase)));
                    currentScale = Vector3.LerpUnclamped(peakScale, baseScale, t);
                    colorBlend = 1f - t;
                }
                else
                {
                    currentScale = peakScale;
                    colorBlend = 1f;
                }

                statTransform.localScale = currentScale;
                statText.color = Color.LerpUnclamped(baseColor, gameOverStatHighlightColor, colorBlend);

                yield return null;
            }

            apply(target);
            statTransform.localScale = baseScale;
            statText.color = baseColor;
        }

        private static float EaseOutBack(float value)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float inverted = value - 1f;
            return 1f + c3 * inverted * inverted * inverted + c1 * inverted * inverted;
        }

        private void HandleRunStarted()
        {
            RestoreCanvasPlacement();
            SetStatus("Slice clean. Avoid bombs.", 0f);
            SetGameOverVisible(false, default, string.Empty);
            HideComboPopup();
            HideHighScoreCelebration();
            HideDifficultyTierPopup();
        }

        private void HandleRunEnded(GameRunEndedEventArgs eventArgs)
        {
            PlaceGameOverCanvasInReach();
            SetGameOverVisible(true, eventArgs.Snapshot, eventArgs.EndReason);
            SuppressSwordButtonActivation(Mathf.Max(MinimumGameOverButtonActivationDelay, gameOverButtonActivationDelay));
            PlayGameOverSound();
        }

        private void HandleScoreChanged(ScoreChangedEventArgs eventArgs)
        {
            if (scoreText != null)
                scoreText.text = eventArgs.Score.ToString();

            if (eventArgs.PointsAdded > 0)
                PlayScorePunch();
        }

        private void HandleCoinCollected(CoinCollectedEventArgs eventArgs)
        {
            if (coinText != null)
                coinText.text = $"Coins  {eventArgs.TotalCoins}";

            SetStatus($"+{eventArgs.BonusPoints} coin bonus", 1.1f);
        }

        private void HandleComboTierChanged(ComboTierChangedEventArgs eventArgs)
        {
            if (eventArgs.Multiplier <= 1)
                return;

            SetStatus($"Combo tier up: {eventArgs.Multiplier}x", 1.5f);
            PlayComboChime(eventArgs.Multiplier);
            PlayComboPopup(eventArgs.Multiplier);
        }

        private void HandleDifficultyTierChanged(DifficultyTierChangedEventArgs eventArgs)
        {
            if (eventArgs.TierIndex <= 0)
                return;

            SetStatus($"Difficulty: {eventArgs.TierLabel}", 1.4f);
            PlayDifficultyTierSound(eventArgs.TierIndex);
            PlayDifficultyTierPopup(eventArgs.TierIndex, eventArgs.TierLabel);
        }

        private void HandleHighScoreBeaten(HighScoreBeatenEventArgs eventArgs)
        {
            SetStatus($"New high score: {eventArgs.NewHighScore}", 1.5f);
            PlayHighScoreCelebration(eventArgs.NewHighScore);
        }

        private void PlayHighScoreCelebration(int newHighScore)
        {
            EnsureHighScoreElements();

            if (_panelImage != null)
            {
                if (_highScoreFlashRoutine != null)
                    StopCoroutine(_highScoreFlashRoutine);
                _highScoreFlashRoutine = StartCoroutine(AnimatePanelFlash());
            }

            if (_highScorePopupText != null)
            {
                if (_highScorePopupRoutine != null)
                    StopCoroutine(_highScorePopupRoutine);
                _highScorePopupRoutine = StartCoroutine(AnimateHighScorePopup(newHighScore));
            }

            PlayHighScoreParticles();
        }

        private void PlayHighScoreParticles()
        {
            EnsureHighScoreParticles();

            if (_highScoreParticles == null || scoreboardCanvas == null)
                return;

            Transform canvasTransform = scoreboardCanvas.transform;
            Vector3 burstPosition = canvasTransform.position
                + canvasTransform.right * highScoreParticleOffset.x
                + canvasTransform.up * highScoreParticleOffset.y
                + canvasTransform.forward * highScoreParticleOffset.z;

            _highScoreParticles.transform.position = burstPosition;
            _highScoreParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _highScoreParticles.Clear(true);
            _highScoreParticles.Emit(Mathf.Max(1, highScoreParticleCount));
            _highScoreParticles.Play(true);
        }

        private IEnumerator AnimatePanelFlash()
        {
            if (_panelImage == null)
                yield break;

            int pulses = Mathf.Max(1, highScoreFlashCount);
            float totalDuration = Mathf.Max(0.2f, highScoreFlashDuration);
            float pulseDuration = totalDuration / pulses;
            Color baseColor = _hasPanelOriginalColor ? _panelOriginalColor : _panelImage.color;

            for (int pulse = 0; pulse < pulses; pulse++)
            {
                float elapsed = 0f;
                while (elapsed < pulseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / pulseDuration);
                    float wave = Mathf.Sin(t * Mathf.PI);
                    _panelImage.color = Color.LerpUnclamped(baseColor, highScoreFlashColor, wave);
                    yield return null;
                }
            }

            _panelImage.color = baseColor;
            _highScoreFlashRoutine = null;
        }

        private IEnumerator AnimateHighScorePopup(int newHighScore)
        {
            _highScorePopupText.gameObject.SetActive(true);
            _highScorePopupText.transform.SetAsLastSibling();
            _highScorePopupText.text = $"NEW HIGH SCORE!\n{newHighScore}";
            _highScorePopupText.color = highScorePopupColor;

            RectTransform popupRect = (RectTransform)_highScorePopupText.transform;
            Vector2 anchoredPosition = popupRect.anchoredPosition;
            Vector3 startScale = Vector3.one * 0.4f;
            Vector3 peakScale = Vector3.one * 1.6f;
            Vector3 settleScale = Vector3.one * 1.3f;
            float duration = Mathf.Max(0.4f, highScoreFlashDuration);
            float popInPhase = Mathf.Min(0.22f, duration * 0.18f);
            float settlePhase = Mathf.Min(0.16f, duration * 0.14f);
            float fadeStart = duration * 0.65f;
            float elapsed = 0f;

            popupRect.localScale = startScale;
            SetTextAlpha(_highScorePopupText, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                Vector3 currentScale;
                if (elapsed <= popInPhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / popInPhase));
                    currentScale = Vector3.LerpUnclamped(startScale, peakScale, t);
                }
                else if (elapsed <= popInPhase + settlePhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01((elapsed - popInPhase) / settlePhase));
                    currentScale = Vector3.LerpUnclamped(peakScale, settleScale, t);
                }
                else
                {
                    currentScale = settleScale;
                }

                popupRect.localScale = currentScale;

                float alpha = elapsed < fadeStart
                    ? 1f
                    : 1f - EaseOutCubic(Mathf.Clamp01((elapsed - fadeStart) / Mathf.Max(0.0001f, duration - fadeStart)));
                SetTextAlpha(_highScorePopupText, alpha);

                yield return null;
            }

            _highScorePopupText.gameObject.SetActive(false);
            _highScorePopupText.color = highScorePopupColor;
            popupRect.localScale = Vector3.one;
            popupRect.anchoredPosition = anchoredPosition;
            _highScorePopupRoutine = null;
        }

        private void HideHighScoreCelebration()
        {
            if (_highScoreFlashRoutine != null)
            {
                StopCoroutine(_highScoreFlashRoutine);
                _highScoreFlashRoutine = null;
            }

            if (_highScorePopupRoutine != null)
            {
                StopCoroutine(_highScorePopupRoutine);
                _highScorePopupRoutine = null;
            }

            if (_panelImage != null && _hasPanelOriginalColor)
                _panelImage.color = _panelOriginalColor;

            if (_highScorePopupText != null)
            {
                _highScorePopupText.gameObject.SetActive(false);
                _highScorePopupText.color = highScorePopupColor;
                _highScorePopupText.transform.localScale = Vector3.one;
            }
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
            EnsureComboChimeSource();
            EnsureComboPopupText();
            EnsureHighScoreElements();
            EnsureHighScoreParticles();
            EnsureGameOverSoundSource();
            EnsureDifficultyTierSoundSource();
            EnsureDifficultyTierPopupText();
        }

        private void EnsureHighScoreParticles()
        {
            if (_highScoreParticles != null)
                return;

            GameObject particleObject = new("HighScoreCelebrationParticles");
            particleObject.transform.SetParent(transform, worldPositionStays: false);
            particleObject.transform.localPosition = Vector3.zero;
            particleObject.transform.localRotation = Quaternion.identity;
            particleObject.transform.localScale = Vector3.one;

            _highScoreParticles = particleObject.AddComponent<ParticleSystem>();
            _highScoreParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _highScoreParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = highScoreParticleLifetime;
            main.startLifetime = new ParticleSystem.MinMaxCurve(highScoreParticleLifetime * 0.7f, highScoreParticleLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(highScoreParticleSpeed * 0.55f, highScoreParticleSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(
                Mathf.Max(0.01f, highScoreParticleSize - highScoreParticleSizeVariation),
                highScoreParticleSize + highScoreParticleSizeVariation);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                highScoreParticleColorPrimary,
                highScoreParticleColorSecondary);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = highScoreParticleGravity;
            main.maxParticles = Mathf.Max(highScoreParticleCount * 2, 256);

            var emission = _highScoreParticles.emission;
            emission.enabled = false;

            var shape = _highScoreParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = highScoreParticleBurstRadius;

            var velocityOverLifetime = _highScoreParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);

            var rotationOverLifetime = _highScoreParticles.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-Mathf.PI * 2f, Mathf.PI * 2f);

            var colorOverLifetime = _highScoreParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient colorGradient = new();
            colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.Lerp(highScoreParticleColorSecondary, Color.white, 0.25f), 0f),
                    new GradientColorKey(highScoreParticleColorPrimary, 0.4f),
                    new GradientColorKey(highScoreParticleColorAccent, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = colorGradient;

            var sizeOverLifetime = _highScoreParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new(
                new Keyframe(0f, 0.6f),
                new Keyframe(0.2f, 1.1f),
                new Keyframe(0.85f, 0.95f),
                new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = _highScoreParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            if (_highScoreParticleMaterial == null)
                _highScoreParticleMaterial = CreateHighScoreParticleMaterial();
            renderer.sharedMaterial = _highScoreParticleMaterial;
        }

        private static Material CreateHighScoreParticleMaterial()
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Legacy Shaders/Particles/Alpha Blended");

            if (shader == null)
                return null;

            Material material = new(shader)
            {
                name = "HighScoreCelebrationParticles_Runtime",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = 3000
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_TintColor"))
                material.SetColor("_TintColor", Color.white);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);
            return material;
        }

        private void EnsureHighScoreElements()
        {
            if (scoreboardCanvas == null)
                return;

            Transform panel = scoreboardCanvas.transform.Find("Panel");
            if (panel == null)
                panel = scoreboardCanvas.transform;

            if (_panelImage == null)
                _panelImage = panel.GetComponent<Image>();

            if (_panelImage != null && !_hasPanelOriginalColor)
            {
                _panelOriginalColor = _panelImage.color;
                _hasPanelOriginalColor = true;
            }

            if (_highScorePopupText == null)
            {
                Transform existing = panel.Find("HighScorePopupText");
                if (existing != null)
                    _highScorePopupText = existing.GetComponent<TMP_Text>();
            }

            if (_highScorePopupText == null)
            {
                CreateText(
                    "HighScorePopupText",
                    panel,
                    string.Empty,
                    78f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -20f),
                    new Vector2(860f, 130f),
                    out _highScorePopupText,
                    highScorePopupColor);
                _highScorePopupText.enableAutoSizing = false;
                _highScorePopupText.outlineWidth = 0.3f;
                _highScorePopupText.outlineColor = new Color32(60, 28, 0, 255);
            }

            _highScorePopupText.raycastTarget = false;
            _highScorePopupText.transform.SetAsLastSibling();
            _highScorePopupText.gameObject.SetActive(false);
        }

        private void EnsureComboPopupText()
        {
            if (comboPopupText == null)
            {
                if (scoreboardCanvas == null)
                    return;

                Transform parent = scoreboardCanvas.transform.Find("Panel");
                if (parent == null)
                    parent = scoreboardCanvas.transform;

                Transform existing = parent.Find("ComboPopupText");
                if (existing != null)
                    comboPopupText = existing.GetComponent<TMP_Text>();

                if (comboPopupText == null)
                {
                    CreateText(
                        "ComboPopupText",
                        parent,
                        string.Empty,
                        comboPopupFontSize,
                        FontStyles.Bold,
                        TextAlignmentOptions.Center,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        comboPopupAnchoredPosition,
                        comboPopupSize,
                        out comboPopupText,
                        comboPopupColor);
                    comboPopupText.enableAutoSizing = false;
                    comboPopupText.outlineWidth = 0.25f;
                    comboPopupText.outlineColor = new Color32(40, 8, 0, 255);
                }
            }

            comboPopupText.raycastTarget = false;
            comboPopupText.transform.SetAsLastSibling();
            comboPopupText.gameObject.SetActive(false);
        }

        private void EnsureComboChimeSource()
        {
            if (scoreboardCanvas == null)
                return;

            if (_comboChimeSource == null)
            {
                _comboChimeSource = scoreboardCanvas.GetComponent<AudioSource>();
                if (_comboChimeSource == null)
                    _comboChimeSource = scoreboardCanvas.gameObject.AddComponent<AudioSource>();
            }

            _comboChimeSource.playOnAwake = false;
            _comboChimeSource.spatialBlend = 1f;
            _comboChimeSource.spatialize = false;
            _comboChimeSource.dopplerLevel = 0f;
            _comboChimeSource.rolloffMode = AudioRolloffMode.Linear;
            _comboChimeSource.minDistance = Mathf.Max(0.1f, comboChimeMinDistance);
            _comboChimeSource.maxDistance = Mathf.Max(_comboChimeSource.minDistance, comboChimeMaxDistance);
            _comboChimeSource.volume = comboChimeVolume;
            _comboChimeSource.bypassReverbZones = true;
        }

        private void EnsureGameOverSoundSource()
        {
            if (scoreboardCanvas == null)
                return;

            if (_gameOverSource == null)
            {
                Transform existing = scoreboardCanvas.transform.Find("GameOverAudio");
                if (existing != null)
                    _gameOverSource = existing.GetComponent<AudioSource>();
            }

            if (_gameOverSource == null)
            {
                GameObject audioObject = new("GameOverAudio", typeof(AudioSource));
                audioObject.transform.SetParent(scoreboardCanvas.transform, false);
                _gameOverSource = audioObject.GetComponent<AudioSource>();
            }

            _gameOverSource.playOnAwake = false;
            _gameOverSource.spatialBlend = 1f;
            _gameOverSource.spatialize = false;
            _gameOverSource.dopplerLevel = 0f;
            _gameOverSource.rolloffMode = AudioRolloffMode.Linear;
            _gameOverSource.minDistance = Mathf.Max(0.1f, gameOverMinDistance);
            _gameOverSource.maxDistance = Mathf.Max(_gameOverSource.minDistance, gameOverMaxDistance);
            _gameOverSource.volume = gameOverVolume;
            _gameOverSource.bypassReverbZones = false;
            _gameOverSource.reverbZoneMix = 1.1f;
        }

        private void EnsureDifficultyTierPopupText()
        {
            if (scoreboardCanvas == null)
                return;

            Transform parent = scoreboardCanvas.transform.Find("Panel");
            if (parent == null)
                parent = scoreboardCanvas.transform;

            if (_difficultyTierPopupText == null)
            {
                Transform existing = parent.Find("DifficultyTierPopupText");
                if (existing != null)
                    _difficultyTierPopupText = existing.GetComponent<TMP_Text>();
            }

            if (_difficultyTierPopupText == null)
            {
                CreateText(
                    "DifficultyTierPopupText",
                    parent,
                    string.Empty,
                    difficultyTierPopupFontSize,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    difficultyTierPopupAnchoredPosition,
                    difficultyTierPopupSize,
                    out _difficultyTierPopupText,
                    difficultyTierMediumColor);
                _difficultyTierPopupText.enableAutoSizing = false;
                _difficultyTierPopupText.outlineWidth = 0.3f;
                _difficultyTierPopupText.outlineColor = new Color32(20, 6, 0, 255);
            }

            _difficultyTierPopupText.raycastTarget = false;
            _difficultyTierPopupText.transform.SetAsLastSibling();
            _difficultyTierPopupText.gameObject.SetActive(false);
        }

        private void PlayDifficultyTierPopup(int tierIndex, string tierLabel)
        {
            EnsureDifficultyTierPopupText();

            if (_difficultyTierPopupText == null)
                return;

            if (_difficultyTierPopupRoutine != null)
                StopCoroutine(_difficultyTierPopupRoutine);

            _difficultyTierPopupRoutine = StartCoroutine(AnimateDifficultyTierPopup(tierIndex, tierLabel));
        }

        private IEnumerator AnimateDifficultyTierPopup(int tierIndex, string tierLabel)
        {
            _difficultyTierPopupText.gameObject.SetActive(true);
            _difficultyTierPopupText.transform.SetAsLastSibling();
            _difficultyTierPopupText.text = string.IsNullOrWhiteSpace(tierLabel) ? "—" : tierLabel.ToUpperInvariant();
            Color tierColor = GetDifficultyTierColor(tierIndex);
            _difficultyTierPopupText.color = tierColor;

            RectTransform popupRect = (RectTransform)_difficultyTierPopupText.transform;
            float slide = difficultyTierPopupSlideDistance * (tierIndex >= 3 ? -1f : 1f);
            Vector2 startPos = difficultyTierPopupAnchoredPosition + new Vector2(slide, 0f);
            Vector2 settlePos = difficultyTierPopupAnchoredPosition;

            float duration = Mathf.Max(0.4f, difficultyTierPopupDuration);
            float slideInPhase = Mathf.Min(0.35f, duration * 0.32f);
            float pulseStart = slideInPhase;
            float pulsePhase = Mathf.Min(0.35f, duration * 0.3f);
            float fadeStart = duration * 0.7f;

            float elapsed = 0f;
            popupRect.anchoredPosition = startPos;
            popupRect.localScale = Vector3.one * 0.85f;
            SetTextAlpha(_difficultyTierPopupText, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                Vector2 currentPos;
                if (elapsed <= slideInPhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / slideInPhase));
                    currentPos = Vector2.LerpUnclamped(startPos, settlePos, t);
                }
                else
                {
                    currentPos = settlePos;
                }
                popupRect.anchoredPosition = currentPos;

                Vector3 scale;
                if (elapsed <= slideInPhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / slideInPhase));
                    scale = Vector3.LerpUnclamped(Vector3.one * 0.85f, Vector3.one * 1.15f, t);
                }
                else if (elapsed <= pulseStart + pulsePhase)
                {
                    float pulseT = Mathf.Clamp01((elapsed - pulseStart) / Mathf.Max(0.0001f, pulsePhase));
                    float wave = Mathf.Sin(pulseT * Mathf.PI * 2f) * (1f - pulseT);
                    scale = Vector3.one * (1.15f + wave * 0.18f);
                }
                else
                {
                    float settleT = EaseOutCubic(Mathf.Clamp01((elapsed - (pulseStart + pulsePhase)) / Mathf.Max(0.0001f, duration - (pulseStart + pulsePhase))));
                    scale = Vector3.LerpUnclamped(Vector3.one * 1.15f, Vector3.one, settleT);
                }
                popupRect.localScale = scale;

                float alpha;
                if (elapsed <= slideInPhase)
                    alpha = EaseOutCubic(Mathf.Clamp01(elapsed / slideInPhase));
                else if (elapsed < fadeStart)
                    alpha = 1f;
                else
                    alpha = 1f - EaseOutCubic(Mathf.Clamp01((elapsed - fadeStart) / Mathf.Max(0.0001f, duration - fadeStart)));
                SetTextAlpha(_difficultyTierPopupText, alpha);

                yield return null;
            }

            _difficultyTierPopupText.gameObject.SetActive(false);
            popupRect.anchoredPosition = settlePos;
            popupRect.localScale = Vector3.one;
            _difficultyTierPopupText.color = tierColor;
            _difficultyTierPopupRoutine = null;
        }

        private void HideDifficultyTierPopup()
        {
            if (_difficultyTierPopupRoutine != null)
            {
                StopCoroutine(_difficultyTierPopupRoutine);
                _difficultyTierPopupRoutine = null;
            }

            if (_difficultyTierPopupText == null)
                return;

            _difficultyTierPopupText.gameObject.SetActive(false);
            ((RectTransform)_difficultyTierPopupText.transform).anchoredPosition = difficultyTierPopupAnchoredPosition;
            _difficultyTierPopupText.transform.localScale = Vector3.one;
        }

        private Color GetDifficultyTierColor(int tierIndex)
        {
            switch (Mathf.Clamp(tierIndex, 0, 3))
            {
                case 3: return difficultyTierFrenzyColor;
                case 2: return difficultyTierHardColor;
                case 1: return difficultyTierMediumColor;
                default: return difficultyTierEasyColor;
            }
        }

        private void EnsureDifficultyTierSoundSource()
        {
            if (scoreboardCanvas == null)
                return;

            if (_difficultyTierSource == null)
            {
                Transform existing = scoreboardCanvas.transform.Find("DifficultyTierAudio");
                if (existing != null)
                    _difficultyTierSource = existing.GetComponent<AudioSource>();
            }

            if (_difficultyTierSource == null)
            {
                GameObject audioObject = new("DifficultyTierAudio", typeof(AudioSource));
                audioObject.transform.SetParent(scoreboardCanvas.transform, false);
                _difficultyTierSource = audioObject.GetComponent<AudioSource>();
            }

            _difficultyTierSource.playOnAwake = false;
            _difficultyTierSource.spatialBlend = 1f;
            _difficultyTierSource.spatialize = false;
            _difficultyTierSource.dopplerLevel = 0f;
            _difficultyTierSource.rolloffMode = AudioRolloffMode.Linear;
            _difficultyTierSource.minDistance = Mathf.Max(0.1f, difficultyTierMinDistance);
            _difficultyTierSource.maxDistance = Mathf.Max(_difficultyTierSource.minDistance, difficultyTierMaxDistance);
            _difficultyTierSource.volume = Mathf.Clamp(difficultyTierVolume, 0f, 1.5f);
            _difficultyTierSource.bypassReverbZones = true;
        }

        private void PlayDifficultyTierSound(int tierIndex)
        {
            EnsureDifficultyTierSoundSource();

            if (_difficultyTierSource == null)
                return;

            int clamped = Mathf.Clamp(tierIndex, 0, 3);
            AudioClip clip = (difficultyTierClips != null && clamped < difficultyTierClips.Length && difficultyTierClips[clamped] != null)
                ? difficultyTierClips[clamped]
                : GetGeneratedDifficultyTierClip(clamped);

            if (clip == null)
                return;

            _difficultyTierSource.pitch = 1f;
            _difficultyTierSource.PlayOneShot(clip, Mathf.Clamp(difficultyTierVolume, 0f, 1.5f));
        }

        private void PlayGameOverSound()
        {
            EnsureGameOverSoundSource();

            if (_gameOverSource == null)
                return;

            AudioClip clip = gameOverClip != null ? gameOverClip : GetGeneratedGameOverSting();
            if (clip == null)
                return;

            _gameOverSource.pitch = 1f;
            _gameOverSource.PlayOneShot(clip, Mathf.Clamp(gameOverVolume * 1.5f, 0f, 2f));
        }

        private void PlayComboChime(int multiplier)
        {
            EnsureComboChimeSource();

            if (_comboChimeSource == null)
                return;

            AudioClip clip = comboChimeClip != null ? comboChimeClip : GetGeneratedComboChime();
            if (clip == null)
                return;

            _comboChimeSource.pitch = Mathf.Clamp(0.92f + (multiplier - 2) * 0.08f, 0.8f, 1.24f);
            _comboChimeSource.PlayOneShot(clip, comboChimeVolume);
        }

        private void PlayComboPopup(int multiplier)
        {
            EnsureComboPopupText();

            if (comboPopupText == null)
                return;

            if (_comboPopupRoutine != null)
                StopCoroutine(_comboPopupRoutine);

            _comboPopupRoutine = StartCoroutine(AnimateComboPopup(multiplier));
        }

        private IEnumerator AnimateComboPopup(int multiplier)
        {
            comboPopupText.gameObject.SetActive(true);
            comboPopupText.transform.SetAsLastSibling();
            comboPopupText.text = $"{multiplier}x COMBO!";
            comboPopupText.color = comboPopupColor;

            RectTransform popupRect = (RectTransform)comboPopupText.transform;
            Vector2 startPosition = comboPopupAnchoredPosition;
            Vector2 endPosition = startPosition + comboPopupTravel;
            Vector3 startScale = Vector3.one * 0.35f;
            Vector3 peakScale = Vector3.one * 1.55f;
            Vector3 settleScale = Vector3.one * 1.25f;
            float duration = Mathf.Max(0.1f, comboPopupDuration);
            float popInPhase = Mathf.Min(0.18f, duration * 0.18f);
            float settlePhase = Mathf.Min(0.12f, duration * 0.12f);
            float fadeStart = duration * 0.55f;
            float elapsed = 0f;

            popupRect.anchoredPosition = startPosition;
            popupRect.localScale = startScale;
            SetTextAlpha(comboPopupText, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);

                Vector3 currentScale;
                if (elapsed <= popInPhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / popInPhase));
                    currentScale = Vector3.LerpUnclamped(startScale, peakScale, t);
                }
                else if (elapsed <= popInPhase + settlePhase)
                {
                    float t = EaseOutCubic(Mathf.Clamp01((elapsed - popInPhase) / settlePhase));
                    currentScale = Vector3.LerpUnclamped(peakScale, settleScale, t);
                }
                else
                {
                    currentScale = settleScale;
                }

                float travelEase = EaseOutCubic(normalized);
                popupRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, travelEase);
                popupRect.localScale = currentScale;

                float alpha = elapsed < fadeStart
                    ? 1f
                    : 1f - EaseOutCubic(Mathf.Clamp01((elapsed - fadeStart) / Mathf.Max(0.0001f, duration - fadeStart)));
                SetTextAlpha(comboPopupText, alpha);

                yield return null;
            }

            comboPopupText.gameObject.SetActive(false);
            comboPopupText.color = comboPopupColor;
            popupRect.anchoredPosition = comboPopupAnchoredPosition;
            popupRect.localScale = Vector3.one;
            _comboPopupRoutine = null;
        }

        private void HideComboPopup()
        {
            if (_comboPopupRoutine != null)
            {
                StopCoroutine(_comboPopupRoutine);
                _comboPopupRoutine = null;
            }

            if (comboPopupText == null)
                return;

            comboPopupText.gameObject.SetActive(false);
            comboPopupText.color = comboPopupColor;
            ((RectTransform)comboPopupText.transform).anchoredPosition = comboPopupAnchoredPosition;
            comboPopupText.transform.localScale = Vector3.one;
        }

        private static void SetTextAlpha(TMP_Text text, float alpha)
        {
            Color color = text.color;
            color.a = Mathf.Clamp01(alpha);
            text.color = color;
        }

        private static AudioClip GetGeneratedComboChime()
        {
            if (s_generatedComboChime != null)
                return s_generatedComboChime;

            const int sampleRate = 44100;
            const float lengthSeconds = 0.42f;
            const float frequencyA = 659.25f;
            const float frequencyB = 987.77f;

            int sampleCount = Mathf.CeilToInt(sampleRate * lengthSeconds);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float attack = 1f - Mathf.Exp(-36f * time);
                float decay = Mathf.Exp(-7.5f * time);
                float envelope = attack * decay;
                float toneA = Mathf.Sin(2f * Mathf.PI * frequencyA * time);
                float toneB = Mathf.Sin(2f * Mathf.PI * frequencyB * time);
                samples[index] = (toneA * 0.62f + toneB * 0.38f) * envelope * 0.32f;
            }

            s_generatedComboChime = AudioClip.Create("Generated Combo Chime", sampleCount, 1, sampleRate, false);
            s_generatedComboChime.SetData(samples, 0);
            return s_generatedComboChime;
        }

        private static AudioClip GetGeneratedGameOverSting()
        {
            if (s_generatedGameOverSting != null)
                return s_generatedGameOverSting;

            const int sampleRate = 44100;
            const float lengthSeconds = 5.5f;
            const float startFreqRoot = 440f;
            const float endFreqRoot = 220f;
            const float startFreqThird = 523.25f;
            const float endFreqThird = 261.63f;
            const float startFreqFifth = 659.25f;
            const float endFreqFifth = 329.63f;

            int sampleCount = Mathf.CeilToInt(sampleRate * lengthSeconds);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float normalized = time / lengthSeconds;

                float bend = Mathf.Pow(normalized, 1.2f);
                float rootFreq = Mathf.Lerp(startFreqRoot, endFreqRoot, bend);
                float thirdFreq = Mathf.Lerp(startFreqThird, endFreqThird, bend);
                float fifthFreq = Mathf.Lerp(startFreqFifth, endFreqFifth, bend);

                float attack = 1f - Mathf.Exp(-90f * time);
                float tail = Mathf.Exp(-0.32f * time);
                float fadeOut = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.85f, 1f, normalized));
                float envelope = attack * tail * fadeOut;

                float vibrato = 1f + 0.022f * Mathf.Sin(2f * Mathf.PI * 5.5f * time);
                float root = Mathf.Sin(2f * Mathf.PI * rootFreq * time * vibrato);
                float third = Mathf.Sin(2f * Mathf.PI * thirdFreq * time * vibrato);
                float fifth = Mathf.Sin(2f * Mathf.PI * fifthFreq * time * vibrato);
                float octaveUp = Mathf.Sin(2f * Mathf.PI * rootFreq * 2f * time * vibrato) * 0.45f;
                float subOctave = Mathf.Sin(2f * Mathf.PI * rootFreq * 0.5f * time) * 0.95f;
                float deepBass = Mathf.Sin(2f * Mathf.PI * rootFreq * 0.25f * time) * 0.75f;
                float crashEnvelope = Mathf.Exp(-5.5f * time);
                float crash = (Random.value * 2f - 1f) * crashEnvelope * 1.35f;
                float shimmerEnvelope = Mathf.Exp(-1.6f * time);
                float shimmer = (Random.value * 2f - 1f) * shimmerEnvelope * 0.28f;

                float mixed = root * 0.85f + third * 0.65f + fifth * 0.55f + octaveUp + subOctave * 0.7f + deepBass * 0.55f + crash + shimmer;
                float saturated = (float)System.Math.Tanh(mixed * 1.9f);
                samples[index] = Mathf.Clamp(saturated * envelope * 1.05f, -0.99f, 0.99f);
            }

            s_generatedGameOverSting = AudioClip.Create("Generated Game Over Sting", sampleCount, 1, sampleRate, false);
            s_generatedGameOverSting.SetData(samples, 0);
            return s_generatedGameOverSting;
        }

        private static AudioClip GetGeneratedDifficultyTierClip(int tierIndex)
        {
            int idx = Mathf.Clamp(tierIndex, 0, 3);
            if (s_generatedDifficultyTierClips[idx] != null)
                return s_generatedDifficultyTierClips[idx];

            const int sampleRate = 44100;
            float length;
            float[] arpeggioFrequencies;
            float noteDuration;
            float crashGain;
            float saturation;
            float baseGain;

            switch (idx)
            {
                case 1:
                    length = 0.85f;
                    arpeggioFrequencies = new[] { 392f, 523.25f, 659.25f };
                    noteDuration = 0.18f;
                    crashGain = 0.12f;
                    saturation = 1.05f;
                    baseGain = 0.55f;
                    break;
                case 2:
                    length = 1.0f;
                    arpeggioFrequencies = new[] { 466.16f, 587.33f, 739.99f, 880f };
                    noteDuration = 0.16f;
                    crashGain = 0.32f;
                    saturation = 1.4f;
                    baseGain = 0.7f;
                    break;
                case 3:
                    length = 1.4f;
                    arpeggioFrequencies = new[] { 220f, 277.18f, 329.63f, 415.30f, 523.25f, 659.25f };
                    noteDuration = 0.14f;
                    crashGain = 0.7f;
                    saturation = 1.9f;
                    baseGain = 0.95f;
                    break;
                default:
                    length = 0.6f;
                    arpeggioFrequencies = new[] { 523.25f, 659.25f };
                    noteDuration = 0.2f;
                    crashGain = 0.06f;
                    saturation = 1f;
                    baseGain = 0.45f;
                    break;
            }

            int sampleCount = Mathf.CeilToInt(sampleRate * length);
            float[] samples = new float[sampleCount];

            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                float time = sampleIndex / (float)sampleRate;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt(time / noteDuration), 0, arpeggioFrequencies.Length - 1);
                float noteTime = time - noteIndex * noteDuration;
                float currentFreq = arpeggioFrequencies[noteIndex];

                float noteAttack = 1f - Mathf.Exp(-70f * noteTime);
                float noteDecay = Mathf.Exp(-6f * noteTime);
                float noteEnvelope = noteAttack * noteDecay;

                float fundamental = Mathf.Sin(2f * Mathf.PI * currentFreq * noteTime);
                float harmonic = Mathf.Sin(2f * Mathf.PI * currentFreq * 2f * noteTime) * 0.35f;
                float subOctave = Mathf.Sin(2f * Mathf.PI * currentFreq * 0.5f * noteTime) * 0.45f;

                float globalDecay = Mathf.Exp(-1.6f * time);
                float crashEnvelope = Mathf.Exp(-12f * time);
                float crash = (Random.value * 2f - 1f) * crashEnvelope * crashGain;

                float mixed = (fundamental + harmonic + subOctave) * noteEnvelope + crash;
                float saturated = (float)System.Math.Tanh(mixed * saturation);
                samples[sampleIndex] = Mathf.Clamp(saturated * globalDecay * baseGain, -0.99f, 0.99f);
            }

            AudioClip clip = AudioClip.Create($"Generated Difficulty Tier {idx}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            s_generatedDifficultyTierClips[idx] = clip;
            return clip;
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

            HideComboPopup();
            HideHighScoreCelebration();
            HideDifficultyTierPopup();

            if (_gameOverIntroRoutine != null)
            {
                StopCoroutine(_gameOverIntroRoutine);
                _gameOverIntroRoutine = null;
            }

            if (gameOverPanel != null && _hasGameOverPanelScale)
                gameOverPanel.transform.localScale = _gameOverPanelOriginalScale;
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
