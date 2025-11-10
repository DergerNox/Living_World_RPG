# Dependency & Singleton Audit

## High-Priority Singletons
| System | File | Pattern | Notes |
| --- | --- | --- | --- |
| World handling | `Assets/Scripst/WorldHandler.cs` | Static data lists + static accessors | Central choke point; must migrate to repositories/service interfaces.
| World generation | `Assets/Scripst/WorldGenerator.cs` | `public static WorldGenerator Instance` | MonoBehaviour singleton driving procedural generation; stage-based pipeline needed.
| Game loop | `Assets/Scripst/GameManager.cs` | Singleton Monobehaviour | Performs cross-system coordination; candidate for orchestration service injection.
| Party management | `Assets/Scripst/PartySystem/PartyManager.cs` | Singleton | Directly referenced by world + UI; heavy coupling to lists.
| Battle | `Assets/Scripst/WorldMapBattleIntegration.cs`, `Assets/Scripst/Battle.cs` | Singleton + static calls (AudioManager) | Needs interface-based resolver for combat + audio services.
| Audio | `Assets/Scripst/Audio/AudioManager.cs` | Singleton | Should become injectable `IAudioService`.
| Time/Weather | `Assets/Scripst/TimeAndWeather/TimeManager.cs`, `WeatherManager.cs`, `WeatherEffectManager.cs` | Singletons broadcasting via direct calls | Convert to event-driven services.
| Visuals | `GraphicsOptimizer`, `VisualEffectManager`, UI singletons | Replace with presenter/controllers injected via ServiceLocator.
| Skill system | `Assets/Scripst/Skills/Core/SkillManager.cs` | Lazy singleton | Already partially decoupled but still static event reliance.
| Diplomacy/NPC | `Assets/Scripst/Characters/FactionDiplomacy.cs`, `NPCManager.cs` | Singletons referencing static world lists | Need repository injection.

## Static Classes / Globals
- `QuestGenerator`, `QuestManager`: pure static orchestration; should expose interfaces for testability and service registration.
- `TradeManager`: static methods mutate world economy state directly; replace with `IEconomyService`.
- `ServiceLocator`: exists but only a simple dictionary. Plan to evolve into bootstrapper-managed registry to transition away from ad-hoc singletons.
- `SkillEvents`: ok as event hub but document thread-safety + ensure unsubscribes.

## Observations
1. **Global State Everywhere** – Most managers expose `public static` fields; few use dependency inversion. This confirms Phase 1 needs interface extraction + registration.
2. **MonoBehaviour Constructors Unused** – Unity lifecycle prevents DI via constructors; we must rely on serialized references or bootstrap scripts to populate services (or use ScriptableObject providers).
3. **ServiceLocator Present** – Provides migration path: start registering old singletons behind interfaces, then phase out direct `Class.Instance` usages.
4. **Save/Load Couplings** – Serialization touches static world lists and manager states; modular snapshots will need interfaces as well.

## Next Steps
- Catalogue per-system dependencies during refactor (start with World, Battle, Skills as highest impact).
- Introduce interfaces in `Assets/Scripst/Core/Interfaces/` for world repositories, time service, battle resolver, etc.
- Update documentation as modules get decoupled, referencing this audit for progress tracking.
