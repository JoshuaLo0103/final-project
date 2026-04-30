using System.Collections;
using System.Collections.Generic;
using BladeFrenzy.Gameplay.Core;
using BladeFrenzy.Gameplay.Slicing;
using BladeFrenzy.Gameplay.Scoring;
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
        [SerializeField] private SpawnedObject healingFruitPrefab;
        [SerializeField] private SpawnPointFlashFeedback spawnPointFlashFeedback;

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
        [SerializeField] private float spawnPointGapOffset = 0.2f;
        [SerializeField] private float maxReachDistance = 0.15f;
        [SerializeField] private float swordReachPadding = 0.18f;
        [SerializeField] private float minimumReachFloor = 0.45f;
        [SerializeField] private Vector2 verticalTargetOffsetRange = new(-0.12f, 0.32f);

        [Header("Distribution")]
        [SerializeField, Range(0f, 1f)] private float bombChance = 0.15f;
        [SerializeField, Range(0f, 1f)] private float healingFruitChance = 0.08f;

        private readonly Dictionary<SpawnedObject, Queue<SpawnedObject>> _pools = new();
        private Coroutine _spawnLoop;
        private float _runStartTime;
        private float _baseLaunchSpeed;
        private SwordHitScorer _cachedSword;

        private void Start()
        {
            _baseLaunchSpeed = launchSpeed;
            if (healingFruitPrefab == null)
                healingFruitPrefab = Resources.Load<SpawnedObject>("HealingAvocadoFruit");

            if (spawnPointFlashFeedback == null)
                spawnPointFlashFeedback = GetComponent<SpawnPointFlashFeedback>();
            spawnPointFlashFeedback?.EnsureMarkersFor(spawnPoints);

            if (spawnOnStart && FindFirstObjectByType<GameManager>() == null)
                BeginRun();
        }

        private void OnValidate()
        {
            if (spawnPointFlashFeedback == null)
                spawnPointFlashFeedback = GetComponent<SpawnPointFlashFeedback>();
            spawnPointFlashFeedback?.EnsureMarkersFor(spawnPoints);
        }

        public void BeginRun()
        {
            StopRun();
            _runStartTime = Time.time;
            spawnPointFlashFeedback?.EnsureMarkersFor(spawnPoints);
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

        public void ResetSpawnedObjects()
        {
            StopRun();

            SpawnedObject[] spawnedObjects = GetComponentsInChildren<SpawnedObject>(true);
            foreach (SpawnedObject spawnedObject in spawnedObjects)
            {
                if (spawnedObject != null && spawnedObject.gameObject.activeInHierarchy)
                    spawnedObject.TryReturnToPool(false);
            }

            SlicedFruitPiece[] slicedPieces = FindObjectsByType<SlicedFruitPiece>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (SlicedFruitPiece slicedPiece in slicedPieces)
            {
                if (slicedPiece != null)
                    Destroy(slicedPiece.gameObject);
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
            spawnPointFlashFeedback?.EnsureMarkersFor(spawnPoints);
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
            spawnPointFlashFeedback?.EnsureMarkersFor(spawnPoints);
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
            bool spawnHealingFruit = !spawnBomb && healingFruitPrefab != null && Random.value < healingFruitChance;
            SpawnedObject prefab = spawnBomb ? bombPrefab : spawnHealingFruit ? healingFruitPrefab : GetRandomFruitPrefab();
            if (prefab == null)
                return;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnedObject instance = GetOrCreate(prefab);
            spawnPointFlashFeedback?.Trigger(spawnPoint);

            Vector3 target = ResolveDynamicTargetPoint();
            Vector3 spawnPosition = ResolveSpawnPosition(spawnPoint, target);

            Vector3 launchDirection = (target - spawnPosition).normalized;
            Vector3 velocity = launchDirection * launchSpeed + Vector3.up * upwardBoost;
            Vector3 angularVelocity = Random.insideUnitSphere * torqueStrength;

            instance.Launch(
                this,
                spawnPosition,
                Random.rotation,
                velocity,
                angularVelocity);
        }

        private Vector3 ResolveSpawnPosition(Transform spawnPoint, Vector3 target)
        {
            if (spawnPoint == null)
                return target;

            Vector3 spawnPosition = spawnPoint.position;
            if (spawnPointGapOffset <= 0f)
                return spawnPosition;

            Vector3 towardTarget = (target - spawnPosition).normalized;
            if (towardTarget.sqrMagnitude < 0.0001f)
                return spawnPosition;

            return spawnPosition + towardTarget * spawnPointGapOffset;
        }

        private Vector3 ResolveDynamicTargetPoint()
        {
            Vector3 playerOrigin = targetPoint.position;

            Transform viewer = Camera.main != null ? Camera.main.transform : targetPoint;
            Vector3 forward = Vector3.ProjectOnPlane(viewer.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(viewer.right, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.right;

            float swordReach = ResolveSwordReach();
            float suggestedMinimum = Mathf.Max(minimumReachFloor, swordReach - swordReachPadding);
            float maximumReachDistance = Mathf.Max(0.05f, maxReachDistance);
            float minimumReachDistance = Mathf.Min(suggestedMinimum, Mathf.Max(0.01f, maximumReachDistance - 0.01f));
            float reachDistance = Random.Range(minimumReachDistance, maximumReachDistance);

            Vector3 target = playerOrigin + forward * reachDistance;
            target += right * Random.Range(-targetSpread, targetSpread);
            target += Vector3.up * Random.Range(verticalTargetOffsetRange.x, verticalTargetOffsetRange.y);
            return target;
        }

        private float ResolveSwordReach()
        {
            if (_cachedSword == null)
                _cachedSword = FindFirstObjectByType<SwordHitScorer>();

            if (_cachedSword == null)
                return minimumReachFloor + 0.5f;

            Renderer swordRenderer = _cachedSword.GetComponent<Renderer>();
            if (swordRenderer == null)
                return minimumReachFloor + 0.5f;

            Vector3 swordSize = swordRenderer.bounds.size;
            return Mathf.Max(swordSize.x, Mathf.Max(swordSize.y, swordSize.z));
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

            if (spawnedObject.GetComponent<HealingFruitPickup>() != null)
                return healingFruitPrefab;

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
