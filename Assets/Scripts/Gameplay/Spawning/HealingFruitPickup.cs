using BladeFrenzy.Gameplay.Core;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class HealingFruitPickup : MonoBehaviour
    {
        [SerializeField] private int restoreAmount = 1;

        public void RestoreLife()
        {
            LivesManager livesManager = FindFirstObjectByType<LivesManager>();
            livesManager?.RestoreLife(restoreAmount);
        }
    }
}
