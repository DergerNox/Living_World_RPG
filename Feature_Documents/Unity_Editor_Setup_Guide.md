# Unity Editor Setup Guide for Living World RPG Features

This document provides a detailed guide on what to do in the Unity Editor for each feature in the Living World RPG project. It assumes you have the relevant scripts (e.g., C# classes) already implemented or available. For each feature, follow the steps to integrate it into your Unity project, including setting up scenes, prefabs, UI elements, and script assignments. Refer to the individual feature documents for class details and completeness status.

---

## 1. Save/Load System
**Classes Involved:** `SaveLoadManager`, `GameData`

**Unity Editor Steps:**
- Create a new GameObject in your main scene (e.g., "GameManager") and attach the `SaveLoadManager` script to it.
- In the Inspector for `SaveLoadManager`, assign any required references (e.g., paths for save files if not hardcoded).
- Set up UI for save/load menus: Create a Canvas, add Buttons for "Save" and "Load", and attach event handlers to call methods like `SaveGame()` and `LoadGame()` from `SaveLoadManager`.
- Ensure the `GameData` class is serializable; no direct editor setup needed beyond script compilation.
- Test by playing the scene and verifying saves/load via key bindings or UI.

---

## 2. Inventory and Equipment System
**Classes Involved:** `Item`, `Inventory`, `Equipment`

**Unity Editor Steps:**
- Create prefabs for items: For each item type (weapon, armor, consumable), create a GameObject, attach an `Item` script, and set properties in the Inspector (e.g., name, stats, icon sprite).
- For the player character: Attach `Inventory` and `Equipment` scripts to the Player GameObject.
- Set up UI: Create a Canvas with panels for inventory slots and equipment slots. Use Image components for slots and assign drag-and-drop scripts if implemented.
- In the Inspector for `Inventory`, assign the UI slot references and item prefabs.
- Configure equipment slots on character models: If using 3D models, add empty GameObjects as attachment points and reference them in `Equipment` for visual equipping.
- Test by adding items to inventory and equipping them, checking stat bonuses.

---

## 3. Skill System
**Classes Involved:** `Skill`

**Unity Editor Steps:**
- Create skill prefabs: For each skill, create a GameObject, attach `Skill` script, and set properties in the Inspector (e.g., type, level, description, effects).
- Attach to characters: On Player or NPC GameObjects, add a component to hold skills (e.g., a list in `Characters` script).
- Set up UI: Create a skill tree or list UI on a Canvas, with Buttons for each skill. Attach event handlers to activate skills.
- Integrate with combat: Ensure battle scripts reference skill effects; no direct editor setup beyond script logic.
- Test by assigning skills to characters and triggering them in play mode.

---

## 4. Battle System
**Classes Involved:** `Battle`

**Unity Editor Steps:**
- Create a battle scene: Duplicate your main scene or create a new one for battles.
- Attach `Battle` script to a GameObject in the battle scene (e.g., "BattleManager").
- Set up troop representations: Create prefabs for troops (using `Troop` script), with Sprites or 3D models, and assign them in `Battle` Inspector for attackers/defenders.
- Configure UI: Add a Canvas for battle UI (e.g., health bars, action buttons). Reference UI elements in `Battle` script for updates.
- For terrain effects: If implemented, assign terrain types via Tilemaps or GameObjects and reference in battle logic.
- Test by triggering a battle (e.g., via party collision) and verifying troop simulations.

---

## 5. Party System
**Classes Involved:** `Party`

**Unity Editor Steps:**
- Attach `Party` script to the Player GameObject or a dedicated PartyManager GameObject.
- Set up party members: In the Inspector, assign references to Player and NPC GameObjects as members/troops.
- Configure movement: If using pathfinding, ensure `Party` integrates with `PathfindingManager` (attach to the same GameObject or reference via Inspector).
- Set up UI: Create a party panel on a Canvas to display members, resources, and quests. Update dynamically via `Party` script.
- For exploration: Assign movement controls (e.g., via Input Manager) and ensure party follows player.
- Test by adding members to the party and moving around the world map.

---

## 6. Troop System
**Classes Involved:** `Troop`

**Unity Editor Steps:**
- Create troop prefabs: For each troop type/rank, create a GameObject with SpriteRenderer, attach `Troop` script, and set stats/images in the Inspector.
- Integrate with settlements: In Town/Castle prefabs, add lists of available troops and reference prefabs.
- Set up recruitment UI: On settlement UI Canvases, add buttons/cards for troops, linking to recruitment methods.
- For leveling: Ensure battle scripts update troop levels; visual changes (e.g., sprites) can be handled via script.
- Test by recruiting troops and viewing their stats in UI.

---

## 7. Settlement Management
**Classes Involved:** `Town`, `Castle`, `Village`

**Unity Editor Steps:**
- Create settlement prefabs: For each type, create a GameObject, attach the respective script (e.g., `Town`), and set properties in the Inspector (e.g., position, faction, buildings).
- Place in world: In your main scene, instantiate settlement prefabs at grid positions, managed by `WorldHandler`.
- Set up UI: Attach `SettlementUIManager` to a Canvas or GameObject, and configure panels for recruitment, building queues, etc.
- Configure buildings: If buildings are implemented, add child GameObjects to settlements with production logic.
- Test by visiting a settlement and interacting via UI.

---

## 8. Economy and Trade System
**Classes Involved:** `Market`, `TradeManager`

**Unity Editor Steps:**
- Attach `Market` and `TradeManager` scripts to settlement GameObjects (e.g., Towns).
- Set up trade UI: Create a Canvas with buy/sell interfaces, using Sliders or Inputs for quantities and prices.
- Configure resources: Assign resource types and initial prices in `Market` Inspector.
- Integrate with parties: Ensure `Party` scripts handle gold and tradeable items.
- Test by trading at a settlement and checking market fluctuations.

---

## 9. World Management
**Classes Involved:** `WorldHandler`

**Unity Editor Steps:**
- Create a WorldManager GameObject in the main scene and attach `WorldHandler` script.
- In the Inspector, assign references to Player, factions, and settlement lists.
- Set up grid/world: If using Tilemaps, configure the world grid and assign to `WorldHandler` for retrieval methods.
- For procedural generation: If implemented, trigger via script on scene load.
- Test by querying settlements at positions and verifying world state.

---

## 10. Quest and Story System
**Classes Involved:** `Quest`, `QuestGenerator`, `QuestManager`, `Dialogue`

**Unity Editor Steps:**
- Attach `QuestManager` to a GameObject (e.g., GameManager).
- Create quest prefabs: For each quest, create a GameObject with `Quest` script, setting objectives, rewards, etc., in the Inspector.
- Set up dialogue UI: Create a Canvas with TextMeshPro for dialogue, buttons for choices, and attach `Dialogue` script.
- Integrate with NPCs: On NPC GameObjects, assign quests and dialogue trees.
- Test by starting a quest and progressing through objectives.

---

## 11. Time and Weather System
**Classes Involved:** `TimeManager`, `WeatherManager`, `WeatherEffectManager`, `NPCManager`, `TimeWeatherUI`

**Unity Editor Steps:**
- Attach `TimeManager` and `WeatherManager` to a GameObject (e.g., EnvironmentManager).
- Set up UI: Create a Canvas for time/weather display (e.g., clock, weather icons) and attach `TimeWeatherUI`.
- Configure effects: For weather visuals, add ParticleSystems or lighting changes, referenced in `WeatherEffectManager`.
- Integrate with NPCs: Ensure `NPCManager` uses time for schedules.
- Test by advancing time and observing weather changes.

---

## 12. AI and NPC Behavior
**Classes Involved:** (Proposed; assume AI scripts like state machines)

**Unity Editor Steps:**
- Attach AI scripts to NPC GameObjects (e.g., state machine components).
- Set up behaviors: In Inspector, configure states (e.g., patrol, converse) and transitions.
- Configure dialogue: Link to dialogue systems for interactions.
- For factions: Assign faction data to NPCs and settlements.
- Test by interacting with NPCs and observing behaviors.

---

## 13. UI System
**Classes Involved:** `SettlementUIManager` (and others like `InventoryUI`, `CharacterStatsUI`)

**Unity Editor Steps:**
- Create a main Canvas for UI elements.
- Attach UI managers (e.g., `SettlementUIManager`) to the Canvas or child panels.
- Set up panels: For settlements, inventory, etc., add child GameObjects with layouts (e.g., GridLayoutGroup for slots).
- Assign references: In Inspectors, link buttons, texts, and images to script variables.
- Configure animations: Use Animator components for transitions.
- Test by opening UI panels and verifying interactions.

---

## 14. Pathfinding
**Classes Involved:** `PathfindingManager`, `Node`

**Unity Editor Steps:**
- Attach `PathfindingManager` to a GameObject in the scene.
- Set up grid: Create a Tilemap for the world, marking walkable/obstacle tiles.
- Configure nodes: If using `Node` prefabs, place them on the grid and assign to `PathfindingManager`.
- Integrate with movement: Reference `PathfindingManager` in party/troop scripts for path requests.
- Test by requesting paths between points and visualizing them.

---

## 15. Resources and Buildings
**Classes Involved:** `Buildings`, `Resources`, `AdvancedResources`

**Unity Editor Steps:**
- Attach `Resources` and `AdvancedResources` to settlement or party GameObjects.
- Create building prefabs: For each building, create a GameObject with `Buildings` script, setting production/effects in Inspector.
- Set up UI: Add resource panels to settlement UI, displaying amounts and crafting options.
- Integrate with economy: Link to `Market` for trading.
- Test by constructing buildings and checking resource production.

---

## 16. Visual Effects and Graphics Enhancements
**Classes Involved:** (Proposed; use Unity built-ins)

**Unity Editor Steps:**
- Add ParticleSystems to GameObjects for effects (e.g., battle impacts).
- Set up animations: Use Animator on character models for actions.
- Configure camera: Attach Cinemachine or custom scripts for controls/cinematics.
- Enhance graphics: Adjust materials, lighting, and post-processing in Scene view.
- Test by triggering effects during gameplay.

---

## 17. Sound and Audio System
**Classes Involved:** (Proposed; use Unity Audio)

**Unity Editor Steps:**
- Create AudioSources on GameObjects (e.g., for ambient sounds).
- Set up AudioClips: Import sounds and assign to sources or scripts.
- Configure dynamic audio: Use scripts to change clips based on events.
- Add music: Attach AudioSource to a persistent GameObject for BGM.
- Test by playing sounds on events (e.g., battles).

---

## 18. Character System
**Classes Involved:** `Characters`, `Player`, `NPC`

**Unity Editor Steps:**
- Create character prefabs: For Player and NPCs, attach respective scripts and set base stats in Inspector.
- Set up visuals: Add SpriteRenderer or 3D models, and reference in scripts.
- Integrate systems: Attach inventory, skills, etc., as child components.
- Configure AI: For NPCs, add behavior scripts.
- Test by leveling characters and checking stats.

---

This guide is a starting point; adjust based on your specific implementations and test iteratively in Play Mode. Ensure all scripts compile without errors before setup.
