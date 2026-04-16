using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public enum FruitType
    {
        Apple,
        Banana,
        Orange,
        Watermelon,
        Bomb
    }

    public class FruitData : MonoBehaviour
    {
        [SerializeField] private FruitType fruitType = FruitType.Apple;
        [SerializeField] private int pointValue = 1;

        public FruitType FruitType => fruitType;
        public int PointValue => pointValue;
        public bool IsBomb => fruitType == FruitType.Bomb;
    }
}
