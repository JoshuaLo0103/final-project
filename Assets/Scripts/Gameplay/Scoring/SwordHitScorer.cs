using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    public class SwordHitScorer : MonoBehaviour
    {
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private int defaultFruitPoints = 1;
        [SerializeField] private int defaultBombPenalty = 1;

        private void Awake()
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryScoreHit(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryScoreHit(other);
        }

        private void TryScoreHit(Collider other)
        {
            if (other == null)
                return;

            SpawnedObject spawnedObject = ResolveComponent<SpawnedObject>(other);
            FruitData fruitData = ResolveComponent<FruitData>(other);
            if (spawnedObject == null || fruitData == null)
                return;

            int scoreDelta = ResolveScoreDelta(fruitData);
            if (!spawnedObject.TryReturnToPool())
                return;

            if (scoreManager != null)
                scoreManager.AddScore(scoreDelta);
        }

        private int ResolveScoreDelta(FruitData fruitData)
        {
            int pointValue = Mathf.Max(1, fruitData.PointValue);
            if (fruitData.IsBomb)
                return -Mathf.Max(defaultBombPenalty, pointValue);

            return Mathf.Max(defaultFruitPoints, pointValue);
        }

        private static T ResolveComponent<T>(Collider other) where T : Component
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out T attachedComponent))
                return attachedComponent;

            if (other.TryGetComponent(out T colliderComponent))
                return colliderComponent;

            return other.GetComponentInParent<T>();
        }
    }
}
