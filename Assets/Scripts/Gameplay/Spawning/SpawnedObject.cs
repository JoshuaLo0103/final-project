using UnityEngine;
using BladeFrenzy.Gameplay.Core;
using BladeFrenzy.Gameplay.Slicing;

namespace BladeFrenzy.Gameplay.Spawning
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FruitData))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SpawnedObject : MonoBehaviour
    {
        [SerializeField] private float missedYThreshold = 42f;
        [SerializeField] private SpawnedObject sourcePrefab;
        [Header("Slicing")]
        [SerializeField] private Material cutSurfaceMaterial;
        [SerializeField] private float sliceHalfLifetime = 4f;
        [SerializeField] private float sliceSeparationImpulse = 2.5f;
        [SerializeField] private float sliceForwardImpulse = 0.4f;
        [SerializeField] private float sliceUpwardImpulse = 0.6f;
        [SerializeField] private float sliceHalfOffset = 0.05f;

        private SpawnManager _owner;
        private Rigidbody _rigidbody;
        private FruitData _fruitData;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private BombFuseSizzleSound _bombFuseSizzleSound;
        private BombWarningGlow _bombWarningGlow;
        private bool _isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _fruitData = GetComponent<FruitData>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if (!_isActive)
                return;

            if (transform.position.y < missedYThreshold)
                TryReturnToPool(true);
        }

        public void Launch(
            SpawnManager owner,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            Vector3 angularVelocity)
        {
            _owner = owner;
            _isActive = true;

            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = angularVelocity;

            if (_fruitData != null && _fruitData.IsBomb)
            {
                if (_bombFuseSizzleSound == null)
                    _bombFuseSizzleSound = GetComponent<BombFuseSizzleSound>() ?? gameObject.AddComponent<BombFuseSizzleSound>();

                _bombFuseSizzleSound.Play();

                if (_bombWarningGlow == null)
                    _bombWarningGlow = GetComponent<BombWarningGlow>() ?? gameObject.AddComponent<BombWarningGlow>();

                _bombWarningGlow.BeginGlow();
            }
        }

        public bool TrySlice(Vector3 planePoint, Vector3 planeNormal, Vector3 swingDirection)
        {
            if (!_isActive || _fruitData == null)
                return false;

            if (_fruitData.IsBomb)
            {
                HandleBombHit();
                return true;
            }

            if (_meshFilter == null || _meshRenderer == null || _meshFilter.sharedMesh == null)
            {
                HandleSuccessfulSlice();
                return true;
            }

            if (!_meshFilter.sharedMesh.isReadable)
            {
                Debug.LogWarning(
                    $"{name} cannot spawn sliced halves because mesh '{_meshFilter.sharedMesh.name}' is not readable. Enable Read/Write on the mesh import settings.",
                    this);
                HandleSuccessfulSlice();
                return true;
            }

            Vector3 localNormal = transform.InverseTransformDirection(planeNormal).normalized;
            if (localNormal.sqrMagnitude < 0.0001f)
            {
                HandleSuccessfulSlice();
                return true;
            }

            Plane localSlicePlane = new Plane(localNormal, transform.InverseTransformPoint(planePoint));
            if (!MeshSlicer.Slice(
                    _meshFilter.sharedMesh,
                    localSlicePlane,
                    _meshRenderer.sharedMaterials,
                    cutSurfaceMaterial,
                    out SliceResult sliceResult))
            {
                HandleSuccessfulSlice();
                return true;
            }

            try
            {
                SpawnSliceHalf("A", sliceResult.PositiveMesh, sliceResult.PositiveMaterials, planeNormal, swingDirection);
                SpawnSliceHalf("B", sliceResult.NegativeMesh, sliceResult.NegativeMaterials, -planeNormal, swingDirection);
                HandleSuccessfulSlice();
                return true;
            }
            catch
            {
                if (sliceResult.PositiveMesh != null)
                    Destroy(sliceResult.PositiveMesh);
                if (sliceResult.NegativeMesh != null)
                    Destroy(sliceResult.NegativeMesh);
                throw;
            }
        }

        public void HandleSuccessfulSlice()
        {
            if (!_isActive || _fruitData == null)
                return;

            if (_fruitData.IsBomb)
            {
                GameEvents.RaiseBombHit(_fruitData, transform.position);
            }
            else
            {
                HealingFruitPickup healingFruitPickup = GetComponent<HealingFruitPickup>();
                healingFruitPickup?.RestoreLife();

                GameEvents.RaiseFruitSliced(_fruitData, transform.position);
            }

            TryReturnToPool(false);
        }

        public void HandleBombHit()
        {
            if (!_isActive)
                return;

            GameEvents.RaiseBombHit(_fruitData, transform.position);
            TryReturnToPool(false);
        }

        public void SetSourcePrefab(SpawnedObject prefab)
        {
            sourcePrefab = prefab;
        }

        public SpawnedObject GetSourcePrefab()
        {
            return sourcePrefab;
        }

        public void ReturnToPool()
        {
            TryReturnToPool(true);
        }

        public bool TryReturnToPool()
        {
            return TryReturnToPool(true);
        }

        public bool TryReturnToPool(bool reportMiss)
        {
            if (!_isActive)
                return false;

            _isActive = false;
            if (_bombFuseSizzleSound != null)
                _bombFuseSizzleSound.Stop();
            if (_bombWarningGlow != null)
                _bombWarningGlow.EndGlow();

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            if (reportMiss && _fruitData != null && !_fruitData.IsBomb)
                GameEvents.RaiseFruitMissed(_fruitData, transform.position);

            if (_owner != null)
                _owner.Release(this);
            else
                gameObject.SetActive(false);

            return true;
        }

        private void SpawnSliceHalf(
            string suffix,
            Mesh slicedMesh,
            Material[] slicedMaterials,
            Vector3 separationDirection,
            Vector3 swingDirection)
        {
            if (slicedMesh == null)
                return;

            GameObject sliceHalf = new GameObject($"{name}_Slice{suffix}");
            sliceHalf.layer = gameObject.layer;
            sliceHalf.tag = gameObject.tag;

            Transform sliceTransform = sliceHalf.transform;
            sliceTransform.SetPositionAndRotation(transform.position, transform.rotation);
            sliceTransform.localScale = transform.lossyScale;
            sliceTransform.position += separationDirection.normalized * sliceHalfOffset;

            MeshFilter meshFilter = sliceHalf.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = slicedMesh;

            MeshRenderer meshRenderer = sliceHalf.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = slicedMaterials;
            meshRenderer.shadowCastingMode = _meshRenderer.shadowCastingMode;
            meshRenderer.receiveShadows = _meshRenderer.receiveShadows;
            meshRenderer.motionVectorGenerationMode = _meshRenderer.motionVectorGenerationMode;
            meshRenderer.lightProbeUsage = _meshRenderer.lightProbeUsage;
            meshRenderer.reflectionProbeUsage = _meshRenderer.reflectionProbeUsage;

            MeshCollider meshCollider = sliceHalf.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = slicedMesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = false;

            Rigidbody sliceRigidbody = sliceHalf.AddComponent<Rigidbody>();
            sliceRigidbody.mass = Mathf.Max(0.05f, _rigidbody.mass * 0.5f);
            sliceRigidbody.linearDamping = _rigidbody.linearDamping;
            sliceRigidbody.angularDamping = _rigidbody.angularDamping;
            sliceRigidbody.useGravity = _rigidbody.useGravity;
            sliceRigidbody.interpolation = _rigidbody.interpolation;
            sliceRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            sliceRigidbody.linearVelocity = _rigidbody.linearVelocity;
            sliceRigidbody.angularVelocity = _rigidbody.angularVelocity;

            Vector3 separationImpulse = separationDirection.normalized * sliceSeparationImpulse;
            Vector3 travelImpulse = swingDirection.sqrMagnitude > 0.001f
                ? swingDirection.normalized * sliceForwardImpulse
                : Vector3.zero;
            sliceRigidbody.AddForce(separationImpulse + travelImpulse + Vector3.up * sliceUpwardImpulse, ForceMode.Impulse);

            sliceHalf.AddComponent<SlicedFruitPiece>();
            Destroy(sliceHalf, sliceHalfLifetime);
        }
    }
}
