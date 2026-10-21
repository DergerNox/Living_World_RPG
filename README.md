# Living World RPG

A 2D strategy RPG in Unity where the world keeps running without the player. AI factions own castles, towns and villages, parties travel the map, and town markets set prices from supply and demand.

**Status:** work in progress. The simulation core runs; parts of the UI and art are still placeholders, and performance hasn't been benchmarked yet.

<!-- Screenshot: add an image or GIF here, e.g. ![World map](docs/world-map.png) -->

## Features

- **Procedural world map:** Perlin-noise terrain with cellular smoothing, computed in parallel with the Unity Job System (`IJobParallelFor` over `NativeArray`s). Village sites are picked in a parallel job too.
- **Factions and settlements:** castles, towns and villages are assigned to AI factions, with diplomacy between them.
- **Living simulation:** a tick system drives AI parties that roam the map, plus world events such as sieges.
- **Economy:** each town has a market whose prices follow supply and demand; parties buy and sell through a trade service.
- **Pathfinding:** grid-based A* with a binary-heap open set, plus a multi-agent pathfinder.
- **RPG systems:** characters and skills, party management, inventory, quests, battles, time and weather, save/load.

## Architecture

- **Services behind interfaces.** World simulation, world state, world generation, trade, quests, parties, time and battle resolution each sit behind an interface (`IWorldSimulationService`, `ITradeService`, …). A bootstrapper registers them at startup and systems resolve them through a small `ServiceLocator`.
- **One owner for world state.** `WorldSimulationService` initialises the world and answers queries about settlements and factions, so other systems don't reach into global lists.
- **Event-driven time.** `TickSystem` publishes tick, half-second and one-second events that the simulation subscribes to.

| Folder (`Assets/Scripts/`) | What's in it |
| --- | --- |
| `Core/` | Bootstrapping, `ServiceLocator`, `TickSystem`, service interfaces |
| `World/` | Map generation, `WorldSimulationService`, world events |
| `Settlements/`, `Economy/` | Castles, towns, villages; markets and trading |
| `PartySystem/`, `Troops/`, `Characters/` | Parties, troops and characters |
| `Interactions/` | A* pathfinding (`Heap`, `PathfindingManager`, `MultiAgentPathfinder`) |
| `Skills/`, `Inventory/`, `UI/`, `SaveLoad/`, `TimeAndWeather/` | Gameplay systems and interface |

More detail: [`Living_World_RPG_Documentation.md`](Living_World_RPG_Documentation.md) walks through each system, and [`PlantUML_All_Classes.puml`](PlantUML_All_Classes.puml) has the class diagram.

## Running it

1. Install **Unity 6000.2.6f2** (Unity 6) through Unity Hub.
2. Clone this repository and add the folder in Unity Hub (**Add → Add project from disk**).
3. Open `Assets/Scenes/MainMenu.unity` and press Play.

## Credits

- Tiles: [Kenney](https://kenney.nl) Tiny Town pack (CC0).

Made by Ferenc Csiszér, Engineering Informatics student at Óbuda University.
