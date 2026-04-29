using System;
using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public static class GameEvents
    {
        public static event Action OnRunStarted;
        public static event Action<GameRunEndedEventArgs> OnRunEnded;
        public static event Action<FruitSliceEventArgs> OnFruitSliced;
        public static event Action<BombHitEventArgs> OnBombHit;
        public static event Action<FruitMissedEventArgs> OnFruitMissed;
        public static event Action<ScoreChangedEventArgs> OnScoreChanged;
        public static event Action<ComboTierChangedEventArgs> OnComboTierChanged;
        public static event Action<HighScoreBeatenEventArgs> OnHighScoreBeaten;
        public static event Action<LivesChangedEventArgs> OnLivesChanged;
        public static event Action OnLivesDepleted;

        public static void RaiseRunStarted()
        {
            OnRunStarted?.Invoke();
        }

        public static void RaiseRunEnded(GameRunEndedEventArgs eventArgs)
        {
            OnRunEnded?.Invoke(eventArgs);
        }

        public static void RaiseFruitSliced(FruitData fruitData, Vector3 worldPosition)
        {
            if (fruitData == null)
                return;

            OnFruitSliced?.Invoke(new FruitSliceEventArgs(fruitData, worldPosition));
        }

        public static void RaiseBombHit(FruitData fruitData, Vector3 worldPosition)
        {
            OnBombHit?.Invoke(new BombHitEventArgs(fruitData, worldPosition));
        }

        public static void RaiseFruitMissed(FruitData fruitData, Vector3 worldPosition)
        {
            if (fruitData == null || fruitData.IsBomb)
                return;

            OnFruitMissed?.Invoke(new FruitMissedEventArgs(fruitData, worldPosition));
        }

        public static void RaiseComboTierChanged(int comboCount, int multiplier)
        {
            OnComboTierChanged?.Invoke(new ComboTierChangedEventArgs(comboCount, multiplier));
        }

        public static void RaiseScoreChanged(int score, int pointsAdded, int comboCount, int multiplier, Vector3 worldPosition)
        {
            OnScoreChanged?.Invoke(new ScoreChangedEventArgs(score, pointsAdded, comboCount, multiplier, worldPosition));
        }

        public static void RaiseHighScoreBeaten(int newHighScore)
        {
            OnHighScoreBeaten?.Invoke(new HighScoreBeatenEventArgs(newHighScore));
        }

        public static void RaiseLivesChanged(int currentLives, int maxLives)
        {
            OnLivesChanged?.Invoke(new LivesChangedEventArgs(currentLives, maxLives));
        }

        public static void RaiseLivesDepleted()
        {
            OnLivesDepleted?.Invoke();
        }
    }

    public readonly struct FruitSliceEventArgs
    {
        public FruitSliceEventArgs(FruitData fruitData, Vector3 worldPosition)
        {
            FruitData = fruitData;
            WorldPosition = worldPosition;
        }

        public FruitData FruitData { get; }
        public Vector3 WorldPosition { get; }
    }

    public readonly struct BombHitEventArgs
    {
        public BombHitEventArgs(FruitData fruitData, Vector3 worldPosition)
        {
            FruitData = fruitData;
            WorldPosition = worldPosition;
        }

        public FruitData FruitData { get; }
        public Vector3 WorldPosition { get; }
    }

    public readonly struct FruitMissedEventArgs
    {
        public FruitMissedEventArgs(FruitData fruitData, Vector3 worldPosition)
        {
            FruitData = fruitData;
            WorldPosition = worldPosition;
        }

        public FruitData FruitData { get; }
        public Vector3 WorldPosition { get; }
    }

    public readonly struct ComboTierChangedEventArgs
    {
        public ComboTierChangedEventArgs(int comboCount, int multiplier)
        {
            ComboCount = comboCount;
            Multiplier = multiplier;
        }

        public int ComboCount { get; }
        public int Multiplier { get; }
    }

    public readonly struct ScoreChangedEventArgs
    {
        public ScoreChangedEventArgs(int score, int pointsAdded, int comboCount, int multiplier, Vector3 worldPosition)
        {
            Score = score;
            PointsAdded = pointsAdded;
            ComboCount = comboCount;
            Multiplier = multiplier;
            WorldPosition = worldPosition;
        }

        public int Score { get; }
        public int PointsAdded { get; }
        public int ComboCount { get; }
        public int Multiplier { get; }
        public Vector3 WorldPosition { get; }
    }

    public readonly struct HighScoreBeatenEventArgs
    {
        public HighScoreBeatenEventArgs(int newHighScore)
        {
            NewHighScore = newHighScore;
        }

        public int NewHighScore { get; }
    }

    public readonly struct LivesChangedEventArgs
    {
        public LivesChangedEventArgs(int currentLives, int maxLives)
        {
            CurrentLives = currentLives;
            MaxLives = maxLives;
        }

        public int CurrentLives { get; }
        public int MaxLives { get; }
    }

    public readonly struct GameRunEndedEventArgs
    {
        public GameRunEndedEventArgs(ScoreSnapshot snapshot, string endReason)
        {
            Snapshot = snapshot;
            EndReason = endReason;
        }

        public ScoreSnapshot Snapshot { get; }
        public string EndReason { get; }
    }

    public readonly struct ScoreSnapshot
    {
        public ScoreSnapshot(int score, int comboCount, int maxCombo, int multiplier, int highScore)
        {
            Score = score;
            ComboCount = comboCount;
            MaxCombo = maxCombo;
            Multiplier = multiplier;
            HighScore = highScore;
        }

        public int Score { get; }
        public int ComboCount { get; }
        public int MaxCombo { get; }
        public int Multiplier { get; }
        public int HighScore { get; }
    }
}
