# Phase 1 – Core Interface Extraction

Created the initial set of service contracts under `Assets/Scripst/Core/Interfaces/`:

| Interface | Purpose |
| --- | --- |
| `IWorldGenerator` + `WorldGenerationSettings` | Wraps procedural generation so we can pipeline/override implementations. |
| `IWorldStateRepository` | Encapsulates access to player/faction/settlement data and spatial lookups. |
| `IPartyService` | Abstracts party creation/query/movement routines from `PartyManager`. |
| `IQuestService` | Encapsulates quest lifecycle operations (start/complete/update) for DI. |
| `IBattleResolver` | Abstracts combat resolution, paving the way for pluggable battle systems. |
| `IAudioService` | Unhooks gameplay from the `AudioManager` singleton. |
| `ITimeService` | Encapsulates `TimeManager` functionality for event broadcasting/testing. |

## Adapter Implementations (Initial Pass)
- `WorldGenerator`, `WorldHandler`, `AudioManager`, `TimeManager`, and `Battle` now implement their respective interfaces.
- `Bootstrapper` ensures those components exist at runtime and registers them with `ServiceLocator` (`IAudioService`, `ITimeService`, `IWorldGenerator`, `IWorldStateRepository`, `IBattleResolver`).
- `PartyManager` implements `IPartyService` and is registered for DI; consumers can now resolve party data without hitting `PartyManager.Instance`.
- Added `QuestService` (MonoBehaviour) implementing `IQuestService`; `Bootstrapper` ensures it exists and registers it, while legacy `QuestManager` became a thin facade calling the service.
- `WorldGenerator` accepts optional `WorldGenerationSettings` overrides, enabling data-driven pipelines later.
- `WorldHandler` exposes `RebuildSpatialCaches()` through the repository contract to keep spatial queries valid after state changes.
- `AudioManager` adds `PlayAmbientTrack(string trackId)` to satisfy the audio interface; clips are pulled from `Resources`.
- `TimeManager` exposes `Tick`/`SetTime`, decoupling time progression from the Unity `Update` loop for testing.
- `Battle` implements `IBattleResolver`, allowing systems to request combat resolution without referencing the UI-specific battle object.

## Battle Flow Migration
- `BattleUI` no longer depends on a concrete `Battle` instance; it resolves `IBattleResolver` via `ServiceLocator`, stores only party references, and drives duels through `ResolveDuel`.
- `WorldMapBattleIntegration` resolves `IBattleResolver` once (with a `Battle` fallback) instead of `new`ing battles each time, and it hands attackers/defenders + terrain to the UI. If no UI exists (e.g., automated simulation), it invokes `ResolveParties` directly.
- `Battle` itself now pulls `IAudioService` from the locator, removing repeat `AudioManager.Instance` lookups.

## UI Service Adoption
- `WorldMapUI` now fetches `IAudioService`, `IWorldGenerator`, and `IWorldStateRepository` via the locator (with legacy fallbacks) so world map rendering and clicks no longer poke static singletons directly.
- Settlement/party/inventory/character stats UIs cache `IAudioService` once and drop the repeated `AudioManager.Instance` guards.
- `AudioSettingsUI` resolves a concrete `AudioManager` through the locator (or falls back) for slider bindings, keeping configuration centralized without static references.
- Gameplay systems such as `QuestManager` and `Inventory` now retrieve `IAudioService` on demand (via locator fallback), so quest completion/item pickup effects no longer reach into `AudioManager.Instance`.

Next steps: migrate remaining systems (UI panels, quests, inventory) away from `AudioManager.Instance`/other singletons and lean on the registered interfaces.
