using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BladeFrenzy.Gameplay.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Run Settings")]
        [SerializeField] private float runDuration = 180f;
        [SerializeField] private bool autoStart = true;

        [Header("References")]
        [SerializeField] private SpawnManager spawnManager;

        public bool IsRunActive { get; private set; }
        public float RemainingTime { get; private set; }
        public float ElapsedTime => Mathf.Max(0f, runDuration - RemainingTime);
        public float RunDuration => runDuration;

        private ScoreManager _scoreManager;
        private DifficultyManager _difficultyManager;

        private void Awake()
        {
            _scoreManager = GetComponent<ScoreManager>();
            _difficultyManager = GetComponent<DifficultyManager>();

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        private void OnEnable()
        {
            GameEvents.OnBombHit += HandleBombHit;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.OnBombHit -= HandleBombHit;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            if (autoStart)
                StartRun();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                RestartRun();

            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();

            if (!IsRunActive)
                return;

            RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
            if (RemainingTime <= 0f)
                EndRun("Time Up");
        }

        public void StartRun()
        {
            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();

            RemainingTime = runDuration;
            IsRunActive = true;

            _difficultyManager?.ResetDifficulty();
            GameEvents.RaiseRunStarted();

            if (spawnManager != null)
                spawnManager.BeginRun();
        }

        public void RestartRun()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested from Blade Frenzy runtime UI.");
#else
            Application.Quit();
#endif
        }

        private void EndRun(string reason)
        {
            if (!IsRunActive)
                return;

            IsRunActive = false;

            if (spawnManager != null)
                spawnManager.StopRun();

            GameEvents.RaiseRunEnded(new GameRunEndedEventArgs(
                _scoreManager != null ? _scoreManager.GetSnapshot() : default,
                reason));
        }

        private void HandleBombHit(BombHitEventArgs _)
        {
            EndRun("Bomb Hit");
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != gameObject.scene)
                return;

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<SpawnManager>();
        }
    }
}
