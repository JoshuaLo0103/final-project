using System.Collections;
using System.Collections.Generic;
using BladeFrenzy.Gameplay.Core;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private SpawnedObject[] fruitPrefabs;
        [SerializeField] private SpawnedObject bombPrefab;

        [Header("Timing")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private float startDelay = 2f;
        [SerializeField] private float spawnInterval = 1.25f;
        [SerializeField] private float initialBombGracePeriod = 3f;

        [Header("Launch")]
        [SerializeField] private float launchSpeed = 6.5f;
        [SerializeField] private float upwardBoost = 2.5f;
        [SerializeField] private float targetSpread = 1.1f;
        [SerializeField] private float torqueStrength = 7f;

        [Header("Distribution")]
        [SerializeField, Range(0f, 1f)] private float bombChance = 0.15f;

        private readonly Dictionary<SpawnedObject, Queue<SpawnedObject>> _pools = new();
        private Coroutine _spawnLoop;
        private float _runStartTime;
        private float _baseLaunchSpeed;

        private void Start()
        {
            _baseLaunchSpeed = launchSpeed;

            if (spawnOnStart && FindFirstObjectByType<GameManager>() == null)
                BeginRun();
        }

        public void BeginRun()
        {
            StopRun();
            _runStartTime = Time.time;
            _spawnLoop = StartCoroutine(SpawnLoop());
        }

        public void StopRun()
        {
            if (_spawnLoop != null)
            {
                StopCoroutine(_spawnLoop);
                _spawnLoop = null;
            }
        }

        public void SetSpawnInterval(float value)
        {
            spawnInterval = Mathf.Max(0.1f, value);
        }

        public void SetBombRatio(float value)
        {
            bombChance = Mathf.Clamp01(value);
        }

        public void SetLaunchSpeedMultiplier(float multiplier)
        {
            launchSpeed = Mathf.Max(1f, _baseLaunchSpeed * Mathf.Max(0.1f, multiplier));
        }

        public void SetActiveSpawnPoints(Transform[] points)
        {
            spawnPoints = points;
        }

        public void SetActiveSpawnPointCount(int count)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return;

            int clampedCount = Mathf.Clamp(count, 1, spawnPoints.Length);
            Transform[] activePoints = new Transform[clampedCount];
            for (int index = 0; index < clampedCount; index++)
                activePoints[index] = spawnPoints[index];

            spawnPoints = activePoints;
        }

        public void SetFruitPrefabs(SpawnedObject[] prefabs)
        {
            fruitPrefabs = prefabs;
        }

        public void Release(SpawnedObject spawnedObject)
        {
            if (spawnedObject == null)
                return;

            spawnedObject.gameObject.SetActive(false);

            SpawnedObject prefabKey = GetPrefabKey(spawnedObject);
            if (prefabKey == null)
                return;

            if (!_pools.TryGetValue(prefabKey, out Queue<SpawnedObject> pool))
            {
                pool = new Queue<SpawnedObject>();
                _pools[prefabKey] = pool;
            }

            pool.Enqueue(spawnedObject);
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(startDelay);

            while (enabled)
            {
                SpawnNext();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnNext()
        {
            if (targetPoint == null || spawnPoints == null || spawnPoints.Length == 0)
                return;

            bool allowBomb = Time.time - _runStartTime >= initialBombGracePeriod;
            bool spawnBomb = allowBomb && bombPrefab != null && Random.value < bombChance;
            SpawnedObject prefab = spawnBomb ? bombPrefab : GetRandomFruitPrefab();
            if (prefab == null)
                return;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnedObject instance = GetOrCreate(prefab);

            Vector3 target = targetPoint.position;
            target += targetPoint.right * Random.Range(-targetSpread, targetSpread);
            target += targetPoint.up * Random.Range(-0.2f, 0.5f);

            Vector3 launchDirection = (target - spawnPoint.position).normalized;
            Vector3 velocity = launchDirection * launchSpeed + Vector3.up * upwardBoost;
            Vector3 angularVelocity = Random.insideUnitSphere * torqueStrength;

            instance.Launch(
                this,
                spawnPoint.position,
                Random.rotation,
                velocity,
                angularVelocity);
        }

        private SpawnedObject GetOrCreate(SpawnedObject prefab)
        {
            if (_pools.TryGetValue(prefab, out Queue<SpawnedObject> pool))
            {
                while (pool.Count > 0)
                {
                    SpawnedObject pooled = pool.Dequeue();
                    if (pooled != null)
                        return pooled;
                }
            }

            SpawnedObject created = Instantiate(prefab, transform);
            created.SetSourcePrefab(prefab);
            created.gameObject.SetActive(false);
            return created;
        }

        private SpawnedObject GetPrefabKey(SpawnedObject spawnedObject)
        {
            FruitData data = spawnedObject.GetComponent<FruitData>();
            if (data != null && data.IsBomb)
                return bombPrefab;

            SpawnedObject sourcePrefab = spawnedObject.GetSourcePrefab();
            if (sourcePrefab != null)
                return sourcePrefab;

            return GetRandomFruitPrefab();
        }

        private SpawnedObject GetRandomFruitPrefab()
        {
            if (fruitPrefabs == null || fruitPrefabs.Length == 0)
                return null;

            List<SpawnedObject> availablePrefabs = new List<SpawnedObject>();
            foreach (SpawnedObject prefab in fruitPrefabs)
            {
                if (prefab != null)
                    availablePrefabs.Add(prefab);
            }

            if (availablePrefabs.Count == 0)
                return null;

            return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
        }

        private void OnDrawGizmosSelected()
        {
            if (targetPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(targetPoint.position, 0.18f);
            }

            if (spawnPoints == null)
                return;

            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                    continue;

                Gizmos.DrawSphere(spawnPoint.position, 0.15f);
                if (targetPoint != null)
                    Gizmos.DrawLine(spawnPoint.position, targetPoint.position);
            }
        }
    }
}
