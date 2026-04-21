using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class DifficultyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnManager spawnManager;

        [Header("Progression")]
        [SerializeField] private AnimationCurve spawnIntervalOverRun = AnimationCurve.Linear(0f, 1.25f, 1f, 0.5f);
        [SerializeField] private AnimationCurve bombRatioOverRun = AnimationCurve.Linear(0f, 0.1f, 1f, 0.3f);
        [SerializeField] private AnimationCurve launchSpeedMultiplierOverRun = AnimationCurve.Linear(0f, 1f, 1f, 1.8f);

        [Header("Tier Labels")]
        [SerializeField] private string easyLabel = "Easy";
        [SerializeField] private string mediumLabel = "Medium";
        [SerializeField] private string hardLabel = "Hard";
        [SerializeField] private string frenzyLabel = "Frenzy";

        public string CurrentTierLabel { get; private set; } = "Easy";

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsRunActive || spawnManager == null)
                return;

            float normalizedProgress = _gameManager.RunDuration <= 0f
                ? 1f
                : Mathf.Clamp01(_gameManager.ElapsedTime / _gameManager.RunDuration);

            spawnManager.SetSpawnInterval(spawnIntervalOverRun.Evaluate(normalizedProgress));
            spawnManager.SetBombRatio(bombRatioOverRun.Evaluate(normalizedProgress));
            spawnManager.SetLaunchSpeedMultiplier(launchSpeedMultiplierOverRun.Evaluate(normalizedProgress));
            CurrentTierLabel = GetTierLabel(normalizedProgress);
        }

        public void ResetDifficulty()
        {
            CurrentTierLabel = easyLabel;

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();

            if (spawnManager == null)
                return;

            spawnManager.SetSpawnInterval(spawnIntervalOverRun.Evaluate(0f));
            spawnManager.SetBombRatio(bombRatioOverRun.Evaluate(0f));
            spawnManager.SetLaunchSpeedMultiplier(launchSpeedMultiplierOverRun.Evaluate(0f));
        }

        private string GetTierLabel(float normalizedProgress)
        {
            if (normalizedProgress >= 0.85f)
                return frenzyLabel;
            if (normalizedProgress >= 0.6f)
                return hardLabel;
            if (normalizedProgress >= 0.3f)
                return mediumLabel;
            return easyLabel;
        }
    }
}
