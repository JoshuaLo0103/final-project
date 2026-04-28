using UnityEngine;

namespace BladeFrenzy.Gameplay.Core
{
    public static class BladeFrenzyRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeSystems()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null)
                return;

            GameObject runtimeRoot = new("BladeFrenzyRuntime");
            runtimeRoot.AddComponent<ScoreManager>();
            runtimeRoot.AddComponent<DifficultyManager>();
            runtimeRoot.AddComponent<GameManager>();
            runtimeRoot.AddComponent<ScoreboardUI>();
            runtimeRoot.AddComponent<SliceParticleBurst>();
            runtimeRoot.AddComponent<SliceSoundEffect>();
        }
    }
}
