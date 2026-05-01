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

        private CoinManager _coinManager;

        private void Awake()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            _coinManager = GetComponent<CoinManager>();
        }

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnFruitSliced += HandleFruitSliced;
            GameEvents.OnFruitMissed += HandleFruitMissed;
            GameEvents.OnBombHit += HandleBombHit;
            GameEvents.OnCoinCollected += HandleCoinCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnFruitSliced -= HandleFruitSliced;
            GameEvents.OnFruitMissed -= HandleFruitMissed;
            GameEvents.OnBombHit -= HandleBombHit;
            GameEvents.OnCoinCollected -= HandleCoinCollected;
        }

        public ScoreSnapshot GetSnapshot()
        {
            if (_coinManager == null)
                _coinManager = GetComponent<CoinManager>();

            int coinCount = _coinManager != null ? _coinManager.CurrentCoins : 0;
            int coinBonusPoints = _coinManager != null ? coinCount * _coinManager.BonusPointsPerCoin : 0;
            return new ScoreSnapshot(Score, ComboCount, MaxCombo, Multiplier, HighScore, coinCount, coinBonusPoints);
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

            UpdateHighScore();

            if (previousMultiplier != Multiplier)
                GameEvents.RaiseComboTierChanged(ComboCount, Multiplier);
        }

        private void HandleCoinCollected(CoinCollectedEventArgs eventArgs)
        {
            int awardedPoints = Mathf.Max(0, eventArgs.BonusPoints);
            if (awardedPoints <= 0)
                return;

            Score += awardedPoints;
            GameEvents.RaiseScoreChanged(Score, awardedPoints, ComboCount, Multiplier, eventArgs.WorldPosition);
            UpdateHighScore();
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

        private void UpdateHighScore()
        {
            if (Score <= HighScore)
                return;

            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
            GameEvents.RaiseHighScoreBeaten(HighScore);
        }
    }
}
