using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class ScoreManager : MonoBehaviour
    {
        private const string HighScoreKey = "BladeFrenzy.HighScore";

        public int Score { get; private set; }
        public int ComboCount { get; private set; }
        public int MaxCombo { get; private set; }
        public int Multiplier { get; private set; } = 1;
        public int HighScore { get; private set; }

        private void Awake()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnFruitSliced += HandleFruitSliced;
            GameEvents.OnFruitMissed += HandleFruitMissed;
            GameEvents.OnBombHit += HandleBombHit;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnFruitSliced -= HandleFruitSliced;
            GameEvents.OnFruitMissed -= HandleFruitMissed;
            GameEvents.OnBombHit -= HandleBombHit;
        }

        public ScoreSnapshot GetSnapshot()
        {
            return new ScoreSnapshot(Score, ComboCount, MaxCombo, Multiplier, HighScore);
        }

        private void HandleRunStarted()
        {
            Score = 0;
            ComboCount = 0;
            MaxCombo = 0;
            SetMultiplierFromCombo();
        }

        private void HandleFruitSliced(FruitSliceEventArgs eventArgs)
        {
            ComboCount++;
            MaxCombo = Mathf.Max(MaxCombo, ComboCount);

            int previousMultiplier = Multiplier;
            SetMultiplierFromCombo();

            int awardedPoints = Mathf.Max(0, eventArgs.FruitData.PointValue) * Multiplier;
            Score += awardedPoints;

            GameEvents.RaiseScoreChanged(Score, awardedPoints, ComboCount, Multiplier, eventArgs.WorldPosition);

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
                GameEvents.RaiseHighScoreBeaten(HighScore);
            }

            if (previousMultiplier != Multiplier)
                GameEvents.RaiseComboTierChanged(ComboCount, Multiplier);
        }

        private void HandleFruitMissed(FruitMissedEventArgs _)
        {
            ResetCombo();
        }

        private void HandleBombHit(BombHitEventArgs _)
        {
            ResetCombo();
        }

        private void ResetCombo()
        {
            int previousMultiplier = Multiplier;
            ComboCount = 0;
            SetMultiplierFromCombo();

            if (previousMultiplier != Multiplier)
                GameEvents.RaiseComboTierChanged(ComboCount, Multiplier);
        }

        private void SetMultiplierFromCombo()
        {
            if (ComboCount >= 30)
                Multiplier = 4;
            else if (ComboCount >= 15)
                Multiplier = 3;
            else if (ComboCount >= 5)
                Multiplier = 2;
            else
                Multiplier = 1;
        }
    }
}
