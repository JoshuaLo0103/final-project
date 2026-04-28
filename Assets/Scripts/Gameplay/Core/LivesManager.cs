using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class LivesManager : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        [SerializeField] private int startingLives = 3;

        public int CurrentLives { get; private set; }
        public int MaxLives => Mathf.Max(1, maxLives);

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnBombHit += HandleBombHit;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnBombHit -= HandleBombHit;
        }

        private void Awake()
        {
            ResetLives();
        }

        public void ResetLives()
        {
            CurrentLives = Mathf.Clamp(startingLives, 1, MaxLives);
            GameEvents.RaiseLivesChanged(CurrentLives, MaxLives);
        }

        public bool LoseLife(int amount = 1)
        {
            if (CurrentLives <= 0)
                return false;

            CurrentLives = Mathf.Max(0, CurrentLives - Mathf.Max(1, amount));
            GameEvents.RaiseLivesChanged(CurrentLives, MaxLives);

            if (CurrentLives == 0)
                GameEvents.RaiseLivesDepleted();

            return true;
        }

        public bool RestoreLife(int amount = 1)
        {
            if (CurrentLives >= MaxLives)
                return false;

            CurrentLives = Mathf.Clamp(CurrentLives + Mathf.Max(1, amount), 0, MaxLives);
            GameEvents.RaiseLivesChanged(CurrentLives, MaxLives);
            return true;
        }

        private void HandleRunStarted()
        {
            ResetLives();
        }

        private void HandleBombHit(BombHitEventArgs _)
        {
            LoseLife();
        }
    }
}
