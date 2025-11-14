using System;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Lightweight global flag so systems can pause noise/logging while the world is being generated.
    /// </summary>
    public static class WorldGenerationState
    {
        public static event Action<bool> GenerationStateChanged;

        public static bool IsGenerating { get; private set; }

        public static void BeginGeneration()
        {
            SetState(true);
        }

        public static void EndGeneration()
        {
            SetState(false);
        }

        private static void SetState(bool value)
        {
            if (IsGenerating == value)
            {
                return;
            }

            IsGenerating = value;
            GenerationStateChanged?.Invoke(IsGenerating);
        }
    }
}
