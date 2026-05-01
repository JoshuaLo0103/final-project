using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private int bonusPointsPerCoin = 25;

        public int CurrentCoins { get; private set; }
        public int BonusPointsPerCoin => Mathf.Max(0, bonusPointsPerCoin);

        private void OnEnable()
        {
            GameEvents.OnRunStarted += HandleRunStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnRunStarted -= HandleRunStarted;
        }

        public void CollectCoin(Vector3 worldPosition)
        {
            CurrentCoins++;
            GameEvents.RaiseCoinCollected(CurrentCoins, BonusPointsPerCoin, worldPosition);
        }

        private void HandleRunStarted()
        {
            CurrentCoins = 0;
        }
    }
}
