using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class DifficultyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnManager spawnManager;

        [Header("Progression")]
        [SerializeField] private AnimationCurve spawnIntervalOverRun = AnimationCurve.Linear(0f, 2.0f, 1f, 1.7f);
        [SerializeField] private AnimationCurve bombRatioOverRun = AnimationCurve.Linear(0f, 0.04f, 1f, 0.13f);
        [SerializeField] private AnimationCurve launchSpeedMultiplierOverRun = AnimationCurve.Linear(0f, 1f, 1f, 1.3f);

        [Header("Score Tier Thresholds")]
        [SerializeField] private int mediumScoreThreshold = 100;
        [SerializeField] private int hardScoreThreshold = 200;
        [SerializeField] private int frenzyScoreThreshold = 300;

        [Header("Curve Ramp Tuning")]
        [SerializeField, Range(20f, 400f)] private float difficultyRampScore = 350f;
        [SerializeField] private bool allowOvershootPastMax = true;
        [SerializeField, Range(1f, 5f)] private float overshootCap = 1.2f;

        [Header("Tier Labels")]
        [SerializeField] private string easyLabel = "Easy";
        [SerializeField] private string mediumLabel = "Medium";
        [SerializeField] private string hardLabel = "Hard";
        [SerializeField] private string frenzyLabel = "Frenzy";

        public string CurrentTierLabel { get; private set; } = "Easy";
        public int CurrentTierIndex { get; private set; }

        private GameManager _gameManager;
        private ScoreManager _scoreManager;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            _scoreManager = GetComponent<ScoreManager>();

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();

        }

        private float EvaluateCurve(AnimationCurve curve, float progress)
        {
            if (curve == null || curve.length == 0)
                return 0f;

            if (progress <= 1f || !allowOvershootPastMax)
                return curve.Evaluate(Mathf.Clamp01(progress));

            float endValue = curve.Evaluate(1f);
            float slope = (endValue - curve.Evaluate(0.9f)) / 0.1f;
            float extrapolated = endValue + slope * Mathf.Min(progress - 1f, overshootCap - 1f);
            return extrapolated;
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsRunActive || spawnManager == null)
                return;

            if (_scoreManager == null)
                _scoreManager = GetComponent<ScoreManager>();

            int score = _scoreManager != null ? _scoreManager.Score : 0;
            float rampScore = Mathf.Max(1f, difficultyRampScore);
            float rawProgress = score / rampScore;
            float normalizedProgress = allowOvershootPastMax ? Mathf.Min(rawProgress, overshootCap) : Mathf.Clamp01(rawProgress);

            spawnManager.SetSpawnInterval(Mathf.Max(0.05f, EvaluateCurve(spawnIntervalOverRun, normalizedProgress)));
            spawnManager.SetBombRatio(Mathf.Clamp01(EvaluateCurve(bombRatioOverRun, normalizedProgress)));
            spawnManager.SetLaunchSpeedMultiplier(Mathf.Max(0.1f, EvaluateCurve(launchSpeedMultiplierOverRun, normalizedProgress)));

            int newTierIndex = GetTierIndexFromScore(score);
            if (newTierIndex != CurrentTierIndex)
            {
                CurrentTierIndex = newTierIndex;
                CurrentTierLabel = GetTierLabelByIndex(newTierIndex);
                GameEvents.RaiseDifficultyTierChanged(newTierIndex, CurrentTierLabel);
            }
            else
            {
                CurrentTierLabel = GetTierLabelByIndex(newTierIndex);
            }
        }

        public void ResetDifficulty()
        {
            CurrentTierLabel = easyLabel;
            CurrentTierIndex = 0;

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();

            if (spawnManager == null)
                return;

            spawnManager.SetSpawnInterval(EvaluateCurve(spawnIntervalOverRun, 0f));
            spawnManager.SetBombRatio(EvaluateCurve(bombRatioOverRun, 0f));
            spawnManager.SetLaunchSpeedMultiplier(EvaluateCurve(launchSpeedMultiplierOverRun, 0f));
        }

        private int GetTierIndexFromScore(int score)
        {
            if (score >= frenzyScoreThreshold) return 3;
            if (score >= hardScoreThreshold) return 2;
            if (score >= mediumScoreThreshold) return 1;
            return 0;
        }

        private string GetTierLabelByIndex(int tierIndex)
        {
            switch (tierIndex)
            {
                case 3: return frenzyLabel;
                case 2: return hardLabel;
                case 1: return mediumLabel;
                default: return easyLabel;
            }
        }
    }
}
