using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    [ExecuteAlways]
    public class StarCopierShrine : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private string orbPrefabPath = "Copier/RollingBalls_Sci-fi_1_3";
        [SerializeField] private string starPrefabPath = "Copier/Star_Up";

        [Header("Placement")]
        [SerializeField] private float forwardDistance = 1.55f;
        [SerializeField] private float rightDistance = 0.9f;
        [SerializeField] private float verticalOffset = 0f;
        [SerializeField] private bool autoPositionInEditor = false;

        [Header("Burst")]
        [SerializeField] private float activationCooldown = 0.9f;
        [SerializeField] private int minBurstCount = 5;
        [SerializeField] private int maxBurstCount = 7;
        [SerializeField] private float starForwardSpeed = 2.9f;
        [SerializeField] private float starUpwardSpeed = 2.2f;
        [SerializeField] private float starSideSpread = 1.1f;
        [SerializeField] private float spawnJitter = 0.1f;
        [SerializeField] private float starLifetime = 8f;
        [SerializeField] private float starScale = 0.14f;

        private GameObject _orbPrefab;
        private GameObject _starPrefab;
        private Transform _viewer;
        private Transform _shrineRoot;
        private float _nextActivationTime;

        private void Awake()
        {
            RefreshReferences();
            EnsureShrine();
        }

        private void OnEnable()
        {
            RefreshReferences();
            EnsureShrine();
        }

        private void OnValidate()
        {
            RefreshReferences();
            EnsureShrine();
        }

        public bool TryActivate(Vector3 hitPoint)
        {
            if (Time.time < _nextActivationTime || _starPrefab == null)
                return false;

            _nextActivationTime = Time.time + activationCooldown;
            SpawnBurst(hitPoint);
            return true;
        }

        private void EnsureShrine()
        {
            bool createdShrine = false;
            if (_shrineRoot == null)
            {
                Transform existing = transform.Find("StarCopierShrine");
                if (existing != null)
                    _shrineRoot = existing;
                else
                {
                    _shrineRoot = new GameObject("StarCopierShrine").transform;
                    _shrineRoot.SetParent(transform, false);
                    createdShrine = true;
                }
            }

            if (createdShrine || autoPositionInEditor)
                PositionShrine();
            EnsureStand();
            EnsureOrb();
        }

        private void RefreshReferences()
        {
            if (_orbPrefab == null)
                _orbPrefab = Resources.Load<GameObject>(orbPrefabPath);

            if (_starPrefab == null)
                _starPrefab = Resources.Load<GameObject>(starPrefabPath);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                _viewer = mainCamera.transform;
            else if (_viewer == null)
                _viewer = FindFirstObjectByType<Camera>()?.transform;
        }

        private void PositionShrine()
        {
            Vector3 viewerPosition = _viewer != null ? _viewer.position : new Vector3(13.8f, 46f, 0.26f);
            Vector3 viewerForward = _viewer != null ? Vector3.ProjectOnPlane(_viewer.forward, Vector3.up).normalized : Vector3.right;
            Vector3 viewerRight = _viewer != null ? Vector3.ProjectOnPlane(_viewer.right, Vector3.up).normalized : Vector3.back;

            if (viewerForward.sqrMagnitude < 0.001f)
                viewerForward = Vector3.right;
            if (viewerRight.sqrMagnitude < 0.001f)
                viewerRight = Vector3.back;

            Vector3 groundedPosition = viewerPosition
                + viewerForward * forwardDistance
                + viewerRight * rightDistance;
            groundedPosition.y = ResolveGroundHeight(groundedPosition) + verticalOffset;

            _shrineRoot.position = groundedPosition;
            _shrineRoot.rotation = Quaternion.LookRotation(-viewerRight, Vector3.up);
        }

        private float ResolveGroundHeight(Vector3 targetPosition)
        {
            Terrain activeTerrain = Terrain.activeTerrain;
            if (activeTerrain != null)
                return activeTerrain.SampleHeight(targetPosition) + activeTerrain.transform.position.y;

            Vector3 rayOrigin = targetPosition + Vector3.up * 20f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return hitInfo.point.y;

            return targetPosition.y;
        }

        private void EnsureStand()
        {
            CreateStandPiece("ShrineBase", new Vector3(0f, 0.08f, 0f), new Vector3(0.32f, 0.08f, 0.32f), new Color(0.22f, 0.19f, 0.17f));
            CreateStandPiece("ShrineColumn", new Vector3(0f, 0.48f, 0f), new Vector3(0.14f, 0.38f, 0.14f), new Color(0.36f, 0.31f, 0.28f));
            CreateStandPiece("ShrineTop", new Vector3(0f, 0.92f, 0f), new Vector3(0.24f, 0.06f, 0.24f), new Color(0.18f, 0.18f, 0.2f));
        }

        private void CreateStandPiece(string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            Transform existing = _shrineRoot.Find(name);
            GameObject piece = existing != null
                ? existing.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            piece.name = name;
            piece.transform.SetParent(_shrineRoot, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;
            piece.layer = gameObject.layer;

            Renderer renderer = piece.GetComponent<Renderer>();
            if (renderer != null)
                ApplyStandColor(renderer, color);
        }

        private static void ApplyStandColor(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            Material standMaterial = renderer.sharedMaterial;
            if (standMaterial == null || standMaterial.shader == null || standMaterial.shader.name == "Hidden/InternalErrorShader")
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader == null)
                    litShader = Shader.Find("Standard");

                if (litShader != null)
                {
                    standMaterial = new Material(litShader)
                    {
                        color = color
                    };
                    renderer.sharedMaterial = standMaterial;
                    return;
                }
            }

            standMaterial.color = color;
            renderer.sharedMaterial = standMaterial;
        }

        private void EnsureOrb()
        {
            Transform existing = _shrineRoot.Find("StarCopierOrb");
            bool createdOrb = existing == null;
            GameObject orb = existing != null
                ? existing.gameObject
                : _orbPrefab != null
                    ? Instantiate(_orbPrefab, _shrineRoot)
                    : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            orb.name = "StarCopierOrb";
            if (createdOrb)
            {
                orb.transform.localPosition = new Vector3(0f, 1.18f, 0f);
                orb.transform.localRotation = Quaternion.identity;
                orb.transform.localScale = Vector3.one * 0.38f;
            }
            orb.layer = gameObject.layer;

            SphereCollider collider = orb.GetComponent<SphereCollider>();
            if (collider == null)
                collider = orb.AddComponent<SphereCollider>();

            collider.isTrigger = true;
            collider.radius = 1.6f;

            Rigidbody rigidbody = orb.GetComponent<Rigidbody>();
            if (rigidbody == null)
                rigidbody = orb.AddComponent<Rigidbody>();

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            StarCopierOrbTrigger trigger = orb.GetComponent<StarCopierOrbTrigger>();
            if (trigger == null)
                trigger = orb.AddComponent<StarCopierOrbTrigger>();

            trigger.Initialize(this);
        }

        private void SpawnBurst(Vector3 hitPoint)
        {
            Vector3 origin = _shrineRoot.TransformPoint(new Vector3(0f, 1.18f, 0f));
            Vector3 burstForward = _viewer != null ? Vector3.ProjectOnPlane(_viewer.forward, Vector3.up).normalized : Vector3.right;
            Vector3 burstRight = _viewer != null ? Vector3.ProjectOnPlane(_viewer.right, Vector3.up).normalized : Vector3.back;

            if (burstForward.sqrMagnitude < 0.001f)
                burstForward = Vector3.right;
            if (burstRight.sqrMagnitude < 0.001f)
                burstRight = Vector3.back;

            int burstCount = Random.Range(minBurstCount, maxBurstCount + 1);
            for (int index = 0; index < burstCount; index++)
            {
                Vector3 spawnPosition = origin
                    + Random.insideUnitSphere * spawnJitter
                    + Vector3.up * 0.05f;

                GameObject star = Instantiate(_starPrefab, spawnPosition, Random.rotation);
                star.name = "CopierStar";
                star.transform.localScale = Vector3.one * starScale;

                Rigidbody rigidbody = star.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = star.AddComponent<Rigidbody>();

                rigidbody.mass = 0.2f;
                rigidbody.useGravity = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rigidbody.linearDamping = 0.1f;
                rigidbody.angularDamping = 0.05f;

                SphereCollider collider = star.GetComponent<SphereCollider>();
                if (collider == null)
                    collider = star.AddComponent<SphereCollider>();

                collider.isTrigger = false;
                collider.radius = 0.45f;

                Vector3 lateral = burstRight * Random.Range(-starSideSpread, starSideSpread);
                Vector3 velocity = burstForward * starForwardSpeed + lateral + Vector3.up * starUpwardSpeed;
                rigidbody.linearVelocity = velocity;
                rigidbody.angularVelocity = Random.insideUnitSphere * 3.2f;

                Destroy(star, starLifetime);
            }
        }
    }
}
