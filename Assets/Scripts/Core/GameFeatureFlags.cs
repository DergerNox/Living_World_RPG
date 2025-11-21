namespace LivingWorldRPG.Core
{
    /// <summary>
    /// Central toggle for game systems that are not part of the launch build.
    /// </summary>
    public static class GameFeatureFlags
    {
        /// <summary>
        /// Enables the skill/ability progression system.
        /// </summary>
        public static readonly bool SkillsEnabled = false;

        /// <summary>
        /// Enables inventory, equipment, markets, and items.
        /// </summary>
        public static readonly bool ItemsEnabled = false;

        /// <summary>
        /// Enables trading interfaces and market interactions.
        /// </summary>
        public static readonly bool TradingEnabled = true;
    }
}
