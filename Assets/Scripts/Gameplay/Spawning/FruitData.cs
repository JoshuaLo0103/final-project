using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public enum FruitType
    {
        Apple = 0,
        Banana = 1,
        Orange = 2,
        Watermelon = 3,
        Bomb = 4,
        Grapes = 5,
        Pineapple = 6,
        Cherry = 7
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
