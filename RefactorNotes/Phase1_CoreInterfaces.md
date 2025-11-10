# Phase 1 – Core Interface Extraction

Created the initial set of service contracts under `Assets/Scripst/Core/Interfaces/`:

| Interface | Purpose |
| --- | --- |
| `IWorldGenerator` + `WorldGenerationSettings` | Wraps procedural generation so we can pipeline/override implementations. |
| `IWorldStateRepository` | Encapsulates access to player/faction/settlement data and spatial lookups. |
| `IBattleResolver` | Abstracts combat resolution, paving the way for pluggable battle systems. |
| `IAudioService` | Unhooks gameplay from the `AudioManager` singleton. |
| `ITimeService` | Encapsulates `TimeManager` functionality for event broadcasting/testing. |

## Adapter Implementations (Initial Pass)
- `WorldGenerator`, `WorldHandler`, `AudioManager`, `TimeManager`, and `Battle` now implement their respective interfaces.
- `Bootstrapper` ensures those components exist at runtime and registers them with `ServiceLocator` (`IAudioService`, `ITimeService`, `IWorldGenerator`, `IWorldStateRepository`, `IBattleResolver`).
- `WorldGenerator` accepts optional `WorldGenerationSettings` overrides, enabling data-driven pipelines later.
- `WorldHandler` exposes `RebuildSpatialCaches()` through the repository contract to keep spatial queries valid after state changes.
- `AudioManager` adds `PlayAmbientTrack(string trackId)` to satisfy the audio interface; clips are pulled from `Resources`.
- `TimeManager` exposes `Tick`/`SetTime`, decoupling time progression from the Unity `Update` loop for testing.
- `Battle` implements `IBattleResolver`, allowing systems to request combat resolution without referencing the UI-specific battle object.

Next steps: migrate call sites to request these services via `ServiceLocator.Resolve<T>()`, then progressively remove direct singleton usage.
