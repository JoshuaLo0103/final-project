using UnityEngine;
using BladeFrenzy.Gameplay.Spawning;

namespace BladeFrenzy.Gameplay.Core
{
    public static class BladeFrenzyRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeSystems()
        {
            GameObject runtimeRoot = ResolveRuntimeRoot();
            EnsureComponent<LivesManager>(runtimeRoot);
            EnsureComponent<CoinManager>(runtimeRoot);
            EnsureComponent<ScoreManager>(runtimeRoot);
            EnsureComponent<DifficultyManager>(runtimeRoot);
            EnsureComponent<GameManager>(runtimeRoot);
            EnsureComponent<ScoreboardUI>(runtimeRoot);
            EnsureComponent<SliceParticleBurst>(runtimeRoot);
            EnsureComponent<SliceSoundEffect>(runtimeRoot);
            EnsureComponent<MissWhooshSoundEffect>(runtimeRoot);
            EnsureComponent<MissScreenFlashFeedback>(runtimeRoot);
            EnsureComponent<SwordAutoEquip>(runtimeRoot);
            EnsureComponent<StarCopierShrine>(runtimeRoot);
        }

        private static GameObject ResolveRuntimeRoot()
        {
            GameManager existingGameManager = Object.FindFirstObjectByType<GameManager>();
            if (existingGameManager != null)
                return existingGameManager.gameObject;

            GameObject existingRuntimeRoot = GameObject.Find("BladeFrenzyRuntime");
            if (existingRuntimeRoot != null)
                return existingRuntimeRoot;

            return new GameObject("BladeFrenzyRuntime");
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.TryGetComponent(out T existingComponent))
                return existingComponent;

            return target.AddComponent<T>();
        }
    }
}
