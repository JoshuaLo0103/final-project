using BladeFrenzy.Gameplay.Spawning;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public class SliceParticleBurst : MonoBehaviour
    {
        [Header("Fruit Burst")]
        [SerializeField] private int fruitParticleCount = 24;
        [SerializeField] private float fruitParticleLifetime = 0.55f;
        [SerializeField] private float fruitParticleSpeed = 3.75f;
        [SerializeField] private float fruitParticleSize = 0.12f;
        [SerializeField] private float fruitSizeVariation = 0.04f;
        [SerializeField] private float fruitGravityModifier = 0.18f;
        [SerializeField] private float fruitBurstRadius = 0.08f;

        [Header("Bomb Burst")]
        [SerializeField] private int bombParticleCount = 42;
        [SerializeField] private float bombParticleLifetime = 0.8f;
        [SerializeField] private float bombParticleSpeed = 5.5f;
        [SerializeField] private float bombParticleSize = 0.2f;
        [SerializeField] private float bombSizeVariation = 0.08f;
        [SerializeField] private float bombGravityModifier = 0.12f;
        [SerializeField] private float bombBurstRadius = 0.22f;

        private void OnEnable()
        {
            GameEvents.OnFruitSliced += HandleFruitSliced;
            GameEvents.OnBombHit += HandleBombHit;
        }

        private void OnDisable()
        {
            GameEvents.OnFruitSliced -= HandleFruitSliced;
            GameEvents.OnBombHit -= HandleBombHit;
        }

        private void HandleFruitSliced(FruitSliceEventArgs eventArgs)
        {
            if (eventArgs.FruitData == null)
                return;

            CreateBurst(
                burstName: "SliceParticleBurst",
                worldPosition: eventArgs.WorldPosition,
                burstColor: ResolveFruitColor(eventArgs.FruitData.FruitType),
                particleCount: fruitParticleCount,
                particleLifetime: fruitParticleLifetime,
                particleSpeed: fruitParticleSpeed,
                particleSize: fruitParticleSize,
                sizeVariation: fruitSizeVariation,
                gravityModifier: fruitGravityModifier,
                burstRadius: fruitBurstRadius);
        }

        private void HandleBombHit(BombHitEventArgs eventArgs)
        {
            CreateBurst(
                burstName: "BombExplosionBurst",
                worldPosition: eventArgs.WorldPosition,
                burstColor: ResolveBombColor(),
                particleCount: bombParticleCount,
                particleLifetime: bombParticleLifetime,
                particleSpeed: bombParticleSpeed,
                particleSize: bombParticleSize,
                sizeVariation: bombSizeVariation,
                gravityModifier: bombGravityModifier,
                burstRadius: bombBurstRadius);
        }

        private void CreateBurst(
            string burstName,
            Vector3 worldPosition,
            Color burstColor,
            int particleCount,
            float particleLifetime,
            float particleSpeed,
            float particleSize,
            float sizeVariation,
            float gravityModifier,
            float burstRadius)
        {
            GameObject burstObject = new GameObject(burstName);
            burstObject.transform.position = worldPosition;

            ParticleSystem particleSystem = burstObject.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = particleLifetime;
            main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.65f, particleLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.7f, particleSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(
                Mathf.Max(0.01f, particleSize - sizeVariation),
                particleSize + sizeVariation);
            main.startColor = new ParticleSystem.MinMaxGradient(
                Color.Lerp(burstColor, Color.white, 0.18f),
                Color.Lerp(burstColor, Color.black, 0.08f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = gravityModifier;
            main.maxParticles = particleCount;

            var emission = particleSystem.emission;
            emission.enabled = false;

            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = burstRadius;

            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.3f, 0.75f);

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient colorGradient = new Gradient();
            colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.Lerp(burstColor, Color.white, 0.12f), 0f),
                    new GradientColorKey(burstColor, 0.45f),
                    new GradientColorKey(Color.Lerp(burstColor, Color.black, 0.25f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = colorGradient;

            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.9f),
                new Keyframe(0.35f, 1.15f),
                new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;

            particleSystem.Emit(particleCount);
            particleSystem.Play();

            Destroy(burstObject, particleLifetime + 0.35f);
        }

        private static Color ResolveFruitColor(FruitType fruitType)
        {
            return fruitType switch
            {
                FruitType.Apple => new Color(0.82f, 0.12f, 0.16f),
                FruitType.Banana => new Color(0.97f, 0.85f, 0.2f),
                FruitType.Orange => new Color(0.96f, 0.48f, 0.08f),
                FruitType.Watermelon => new Color(0.9f, 0.18f, 0.27f),
                _ => Color.white
            };
        }

        private static Color ResolveBombColor()
        {
            return new Color(0.75f, 0.18f, 0.04f);
        }
    }
}
