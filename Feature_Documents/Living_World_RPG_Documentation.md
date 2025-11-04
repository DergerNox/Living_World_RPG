# Living World RPG - Development Documentation

## Overview

Living World RPG is a Unity-based strategy RPG featuring procedural world generation, faction-based settlements, character progression, party management, turn-based battles, and dynamic world events. The game simulates a living world with NPCs, economies, quests, and time/weather systems.

## Architecture Overview

The project follows a component-based architecture using Unity's MonoBehaviour system for managers and data classes for game entities. Key patterns include:
- Singleton pattern for global managers (e.g., WorldHandler, GameManager)
- Interface-based design (e.g., ISettlement)
- Event-driven systems (e.g., TimeManager events)
- Data serialization for save/load functionality

## Core Systems

### World Generation and Management

#### WorldHandler.cs
**Purpose**: Serves as the central hub for managing global game state in a living world RPG, ensuring all entities (player, settlements, factions, parties) are accessible and synchronized. This system is crucial for creating a dynamic, interconnected world where player actions and AI behaviors can influence global events, fostering emergent gameplay and replayability. By centralizing state management, it enables features like faction diplomacy, settlement interactions, and party movements that respond to real-time changes, making the world feel alive and responsive rather than static. Without this hub, the game's complex web of relationships and updates would be fragmented, leading to inconsistencies and reduced immersion.

**Implementation**:
- Utilizes a static class design with private static variables and public static properties for global access, following a singleton-like pattern without instantiation to ensure thread-safe, centralized data management.
- Data flow begins with world generation: WorldGenerator populates the static lists (Castles, Towns, Villages, Factions) during GenerateWorld(), which WorldHandler accesses via static properties. Settlement retrieval methods iterate through these lists to find matches by position, with error handling for null entries to prevent crashes.
- Integrates with PartyManager for periodic updates (every 300 frames, or ~5 seconds at 60fps) to move parties, simulating ongoing world activity. This creates a living simulation where AI parties roam, settlements evolve, and events unfold independently.
- Performance considerations: List iterations in Update() and retrieval methods can scale poorly with large worlds (e.g., 100x100 maps with many settlements), potentially causing frame drops. Memory usage grows linearly with settlement/faction counts.
- Optimization opportunities: Cache frequently accessed positions in dictionaries for O(1) lookups instead of O(n) iterations. Reduce update frequency or use coroutines for non-critical updates to improve frame rates. Consider spatial partitioning (e.g., quadtrees) for large worlds to limit search spaces.

**Unity Editor Modifications**:
- No direct inspector variables (static class)

**Effects of Changes**:
- Modifying the Update() method's frame count (currently 300) changes how often parties move (every 5 seconds at 60fps), affecting world dynamism—higher frequencies increase activity but may impact performance.

#### WorldGenerator.cs
**Purpose**: Drives procedural world generation to create diverse, replayable game worlds, enabling emergent storytelling and strategic variety. By generating unique terrain, settlements, and faction layouts each playthrough, it ensures no two games are identical, fostering exploration and adaptation. This system is fundamental to the RPG's "living world" concept, where procedural elements lead to unexpected alliances, conflicts, and opportunities, making the game feel organic rather than scripted. Without procedural generation, the world would feel static and predictable, reducing long-term engagement.

**Implementation**:
- Employs a singleton MonoBehaviour pattern with Awake() ensuring a single instance, allowing global access for world generation calls.
- Data flow: GenerateWorld() orchestrates the process—first terrain via GenerateTerrain(), then factions via GenerateFactions(), settlements via GenerateSettlements(), faction assignments via AssignSettlementsToFactions(), and diplomacy initialization via InitializeDiplomacy(). TerrainMap is a 2D array storing TerrainType values for each grid cell.
- Terrain generation uses Perlin noise for natural-looking landscapes: loops over worldWidth x worldHeight, sampling Mathf.PerlinNoise(x * 0.1f, y * 0.1f) to assign terrain types (e.g., noise < 0.3f = Water, < 0.5f = Forest, etc.), creating coherent biomes without manual design.
- Settlement placement: Randomly positions castles, towns, and villages within world bounds, ensuring variety but risking overlaps; factions are created with random centers and initialized lists for settlements.
- Faction assignment: Castles assigned sequentially to factions (i % factionCount), towns/villages randomly distributed, promoting balanced or imbalanced starts based on randomness.
- Integration points: Populates WorldHandler static lists, triggers FactionDiplomacy initialization for inter-faction relations.
- Performance considerations: Terrain generation scales with world size (O(width * height)), potentially slow for large maps (e.g., 100x100 = 10,000 iterations); memory usage for TerrainMap array grows quadratically.
- Optimization opportunities: Precompute or cache Perlin noise values in a separate pass; use multi-threading for terrain generation; implement settlement placement constraints to avoid overcrowding and reduce random retries.

**Unity Editor Modifications**:
- `worldWidth` (default: 100) - Controls world size
- `worldHeight` (default: 100) - Controls world size
- `numberOfFactions` (default: 5) - Number of AI factions
- `numberOfCastles` (default: 5) - Number of castles to generate
- `numberOfTowns` (default: 10) - Number of towns to generate
- `numberOfVillages` (default: 20) - Number of villages to generate

**Effects of Changes**:
- Increasing world dimensions creates larger maps but increases memory usage
- More factions create more complex diplomacy but may impact performance
- Settlement counts affect world density and gameplay variety

#### WalkerGenerator.cs
**Purpose**: Generates detailed tile-based world maps using a drunkard's walk algorithm, providing intricate exploration opportunities and varied terrain layouts. This system enhances replayability by creating unique dungeon-like or regional maps within the broader world, encouraging discovery and tactical navigation. It supports the game's emphasis on exploration by producing organic, maze-like structures that feel hand-crafted yet procedural, adding depth to settlement interactions and faction territories. Without this, maps would be too uniform, reducing the sense of adventure in a living world.

**Implementation**:
- Utilizes a grid-based approach with a 2D array for map tiles, employing the drunkard's walk (random walk) algorithm: multiple "walkers" start from random positions and move randomly, carving out floors while leaving walls, creating natural cave-like or dungeon structures.
- Data flow: Initializes grid with walls, then spawns maxNumOfWalkers walkers that move for a set number of steps or until fillPercentage is reached, ensuring connected landmasses. Settlement placement follows, respecting minimum distance constraints to avoid overcrowding.
- Integration points: Assigns settlements to factions post-placement, feeding into WorldHandler for global state. Supports faction assignment by ensuring settlements are placed in faction-controlled areas.
- Performance considerations: Algorithm scales with MapWidth * MapHeight and maxNumOfWalkers; high walker counts or large maps can increase generation time significantly (e.g., 30x30 grid with 10 walkers may take seconds). Memory usage for the grid array is O(width * height).
- Optimization opportunities: Limit walker steps or use early termination when fillPercentage is met; precompute distance constraints in a lookup table; parallelize walker movements if possible.

**Unity Editor Modifications**:
- `MapWidth` (default: 30) - Grid width
- `MapHeight` (default: 30) - Grid height
- `maxNumOfWalkers` (default: 10) - Maximum concurrent walkers
- `fillPercentage` (default: 0.4f) - Percentage of map to fill with floors
- `CastleCount` (default: 30) - Number of castles to place
- `TownCount` (default: 20) - Number of towns to place
- `VillageCount` (default: 50) - Number of villages to place

**Effects of Changes**:
- Larger maps increase generation time and memory usage
- Higher fill percentage creates more connected landmasses
- More settlements increase world complexity but may cause overcrowding

### Settlement System

#### ISettlement.cs
**Purpose**: Defines a common interface for all settlement types in the living world RPG, ensuring modularity and extensibility. This abstraction allows settlements to be treated uniformly across systems like faction assignment, pathfinding, and world events, promoting code reusability and easier addition of new settlement types. Without this interface, each settlement would require custom handling, leading to fragmented code and increased maintenance complexity.

**Implementation**:
- Simple interface with read-only properties for PosX (x-coordinate), PosY (y-coordinate), and FactionId (affiliation). Data flow: Settlements implement these properties, accessed by WorldHandler for global queries (e.g., GetCastleAtPosition iterates through lists to find matches by position).
- Integration points: Used in FactionMaker for assignment, WorldEventManager for event targeting, and PathfindingManager for obstacle detection. No direct inheritance beyond interface implementation.
- Performance considerations: Properties are lightweight getters, but frequent position-based lookups in WorldHandler can scale poorly with settlement density (O(n) iterations); memory usage is minimal as it's just an interface.
- Optimization opportunities: Cache settlement positions in a spatial data structure (e.g., quadtree) for O(log n) queries instead of linear searches.

**Unity Editor Modifications**:
- No direct inspector variables (interface only)

**Effects of Changes**:
- Adding new properties to the interface requires updating all implementing classes, potentially breaking existing code if not handled carefully.

#### Castle.cs
**Purpose**: Represents fortified military hubs in the game world, serving as strategic strongholds for factions. Castles enable late-game content like sieges, high-tier troop recruitment, and defensive bonuses, adding depth to territorial control and warfare. They act as economic and military centers, influencing faction power balance and player conquest strategies. Without castles, the game would lack high-stakes objectives, reducing replayability and strategic variety.

**Implementation**:
- Implements ISettlement with position and faction properties; extends with military-focused features like garrison size, lord assignment, visitor lists, and building queues. Data flow: Constructor initializes with lord, sets default values (e.g., MaxGarrisonSize=500), and populates AvailableUnits with rank 4-5 troops via AddRank4And5Troops(). ProcessBuildingQueue() advances construction, applying effects like increased garrison capacity.
- Integration points: Populated by WorldGenerator, queried by WorldHandler for position-based access, and affected by WorldEventManager sieges (sets IsCastleUnderSiege flag). BuildingsQueue uses Queue<Buildings> for FIFO processing.
- Performance considerations: Building queue processing is O(1) per tick, but frequent updates could impact frame rate if many castles; memory scales with buildings and visitors (lists grow dynamically).
- Optimization opportunities: Use object pooling for buildings to reduce instantiation overhead; cache defense bonuses to avoid recalculating on every access.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Increasing MaxGarrisonSize allows larger armies but may unbalance defense if not adjusted with costs; building effects (e.g., DefenseBonus) directly impact siege outcomes.

#### Town.cs
**Purpose**: Serves as economic centers in the world, facilitating trade, taxation, and mid-tier military support. Towns drive player economy through markets and income, encouraging exploration and settlement management. They provide a middle ground between villages (population-focused) and castles (military-focused), adding variety to faction territories and player interactions. Without towns, economic gameplay would be limited, reducing depth in resource management and trade.

**Implementation**:
- Implements ISettlement; adds economic features like Market (dynamic pricing), income/expenses tracking, and tax collection. Data flow: Constructor sets defaults (Income=1000, Expenses=500, Taxes=200), initializes market with sample items, and populates AvailableUnits with rank 2-3 troops. CollectTaxes() adds to income; ProcessBuildingQueue() applies income bonuses.
- Integration points: Market updates prices via UpdatePrices() (supply/demand formula), integrated with WorldHandler for global state; buildings use Queue<Buildings> for construction.
- Performance considerations: Market price updates are O(n) for available items, efficient for small inventories; building queues process linearly but infrequently.
- Optimization opportunities: Precompute tax calculations if called often; use dictionaries for market data to speed lookups.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Adjusting Income/Expenses affects town prosperity and player gold rewards; tax rates influence long-term economy but may frustrate if too high.

#### Village.cs
**Purpose**: Represents basic population centers focused on growth and resource production, forming the foundation of faction territories. Villages enable early-game recruitment and resource gathering, simulating organic world expansion. They add realism by growing over time and being vulnerable to events, encouraging protective strategies. Without villages, worlds would feel static, lacking the emergent population dynamics that drive quests and conflicts.

**Implementation**:
- Implements ISettlement; emphasizes population and resources with growth mechanics. Data flow: Constructor initializes Population=100, Resources=100, and AvailableUnits with rank 1-2 troops. ProcessGrowthEvent() increases population/resources randomly; ProcessSiegeEvent() applies damage probabilistically.
- Integration points: Affected by WorldEventManager growth/siege events; buildings limited to 3 for simplicity, with effects on population/resources.
- Performance considerations: Random events are lightweight (O(1)), but frequent growth calls could accumulate; memory usage low with small lists.
- Optimization opportunities: Batch growth updates to reduce per-frame overhead; use coroutines for event processing.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Population growth rates affect militia availability and resource output; siege vulnerability encourages strategic positioning.

### Character and Party System

#### Characters.cs
**Purpose**: Serves as the foundational class for all character entities in the RPG, enabling character progression, customization, and interaction. This system is essential for player agency and narrative depth, allowing characters to grow, equip items, and engage in combat/skills. Without this base class, individual characters would lack consistent stat management, leading to fragmented gameplay and difficulty balancing. It supports the game's emphasis on personalization and long-term investment in character development.

**Implementation**:
- Data class with public properties for stats (Health, Mana, Attack, Defense), progression (Level, Experience), and integration (SkillBook, Inventory, Equipment). Data flow: Constructor initializes stats and creates new instances of SkillBook/Inventory/Equipment; GainExperience() triggers LevelUp() for stat increases; EquipItem() applies/removes bonuses and updates inventory.
- Integration points: Inherited by Player.cs; referenced in Party.cs for member management; used in settlements for lords/visitors; skills and equipment affect combat in Battle.cs.
- Performance considerations: Stat updates are O(1), efficient for frequent calls; inventory/equipment lists scale with item count but are lightweight.
- Optimization opportunities: Cache calculated stats (e.g., total attack) to avoid recomputation; use events for stat changes to notify dependent systems.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Adjusting level-up stat bonuses affects power scaling; equipment integration directly impacts combat effectiveness and player choices.

#### Player.cs
**Purpose**: Represents the player-controlled character, serving as the primary driver of story and agency in the living world RPG. This class enables player immersion by providing a customizable avatar with starting advantages, fostering early-game engagement and progression. Without a distinct player class, the game would lack a clear protagonist, reducing narrative focus and player investment. It bridges base character mechanics with player-specific features like party leadership and faction ties.

**Implementation**:
- Inherits from Characters.cs, extending with player-specific setup. Data flow: Constructor calls base constructor with maxHealth=100, maxMana=100, level=1, then adds starting gold (1000), equipment (Sword, Health Potion), and initializes Party with self as leader and faction 0.
- Integration points: Instance stored in WorldHandler.Player; party managed by PartyManager; equipment/inventory saved by SaveLoadManager.
- Performance considerations: Initialization is one-time (O(1)), no ongoing updates.
- Optimization opportunities: Lazy-load starting items if inventory grows large; parameterize starting values for modding.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Starting gold/equipment levels influence tutorial difficulty and early economy; faction assignment affects initial alliances.

#### Party.cs
**Purpose**: Represents dynamic groups of characters and troops, simulating group dynamics and resource management in a living world. This system adds strategic depth by requiring players to manage logistics, morale, and composition, encouraging tactical decisions in exploration and combat. Without parties, the game would feel solitary, lacking the social and logistical challenges of group adventuring. It supports emergent gameplay through shared resources and movement constraints.

**Implementation**:
- Data class with properties for members, troops, resources, and states (Morale, Fatigue). Data flow: Constructor initializes lists and sets defaults (Gold=1000, Morale=100); AddMember()/AddTroop() updates size; SplitParty() divides resources/members evenly; MoveTo() (assumed method) updates Position.
- Integration points: Player party created in Player.cs; AI parties generated by PartyManager; positions updated by WorldHandler; resources consumed in travel/combat.
- Performance considerations: List operations (add/remove) are O(1) amortized, but splitting scales with party size (O(n)); frequent position updates could impact frame rate if many parties.
- Optimization opportunities: Use HashSets for members/troops if frequent lookups; batch position updates; cache resource totals.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Morale/fatigue thresholds affect movement and combat; resource management influences long-term survival and strategy.

#### PartyManager.cs, PartyTypeEnum.cs
**Purpose**: Manages all parties in the world, including AI behavior and world activity simulation, to create a dynamic, populated environment. This system ensures the world feels alive with moving parties, supporting emergent interactions and player immersion. Without party management, the world would be static, reducing replayability and the sense of a living ecosystem. It enables AI-driven events and faction activities.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: Start() sets PlayerParty from WorldHandler and generates 5 AI parties with random types/positions; UpdatePartyPositions() moves AI parties randomly every call (integrated with WorldHandler's periodic updates).
- PartyTypeEnum defines roles (Trader, ResourceConvoy, etc.) for AI behavior differentiation. Integration points: AllParties list synced with WorldHandler; positions queried by GetPartiesAtPosition(); AI movements simulate world activity.
- Performance considerations: UpdatePartyPositions() scales with party count (O(n)), called frequently (e.g., every 300 frames); random movement is simple but could be optimized for pathfinding.
- Optimization opportunities: Use spatial partitioning for position queries; throttle AI updates based on distance to player; integrate with PathfindingManager for smarter movement.

**Unity Editor Modifications**:
- No direct inspector variables (manager class)

**Effects of Changes**:
- AI party count affects world density and activity; movement frequency impacts performance and dynamism.

### Battle System

#### Battle.cs
**Purpose**: Resolves combat encounters between parties, providing risk/reward mechanics through stat-based damage, terrain advantages, and audio feedback. This system is crucial for player engagement in battles, allowing strategic decisions on troop composition and positioning to influence outcomes, fostering replayability and tactical depth in a living world where conflicts arise from faction rivalries or player actions.

**Implementation**:
- Utilizes a data class design with public properties for attacking/defending parties and terrain. Data flow: BattleIndividual() handles alternating attacks between two troops until one dies, applying terrain modifiers via GetTerrainModifier() (e.g., Archers +20% in Forest, Infantry +30% in Mountains), and integrating audio via AudioManager for attack/hit/death sounds. BattleParties() aggregates troops from parties, uses Parallel.For for concurrent individual battles (thread-safe with ConcurrentBag), aggregates survivors, and redistributes them back to parties based on size limits.
- Integration points: Called by WorldMapBattleIntegration for world-initiated battles; uses Troop.cs for stats and IsAlive; audio feedback enhances immersion without blocking gameplay.
- Performance considerations: Parallel processing scales well for large battles (e.g., 100+ troops) but may increase CPU usage; memory for ConcurrentBag and lists grows with troop counts; audio calls are lightweight but frequent in rapid battles.
- Optimization opportunities: Cache terrain modifiers per troop type to avoid repeated calculations; reduce audio frequency or pool sound sources; implement formation bonuses fully by accessing Party formations (currently placeholder in GetFormationModifier()).

**Unity Editor Modifications**:
- `Terrain` property (set programmatically)

**Effects of Changes**:
- Terrain modifiers encourage strategic positioning (e.g., avoiding Rivers for penalties), directly impacting balance and player tactics; damage calculations affect troop viability, requiring balancing to prevent overpowered units.

#### WorldMapBattleIntegration.cs
**Purpose**: Seamlessly integrates turn-based battles into the world map, enabling strategic positioning and emergent conflicts. This system bridges exploration and combat, allowing players to initiate battles at world positions, with terrain influencing outcomes, promoting thoughtful decision-making in a dynamic world where AI parties and faction wars create opportunities for engagement.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: InitiateBattle() creates a Battle instance, sets parties/terrain, and delegates to BattleUI for turn-based execution; HandleBattleResults() checks survivor counts for win/loss, awards experience (50 per troop) to winners, and logs outcomes. GetTerrainAtPosition() determines terrain (placeholder using defaultTerrain, intended for map lookup).
- Integration points: Populated by WorldHandler for party positions; uses Battle.cs for resolution; BattleUI handles UI flow; experience gains feed into Troop leveling.
- Performance considerations: Lightweight initialization, but result processing scales with troop numbers; terrain lookup could be optimized with spatial data if map is large.
- Optimization opportunities: Cache terrain per position; batch experience awards; integrate with PathfindingManager for battle accessibility.

**Unity Editor Modifications**:
- `defaultTerrain` - Fallback terrain type

**Effects of Changes**:
- Default terrain affects unmapped battles, influencing fairness; experience rewards drive progression, but high values may accelerate leveling, requiring tuning for balance.

#### Troop.cs
**Purpose**: Defines troop entities as the core units in battles, providing variety and progression to add tactical depth. This system enables diverse army compositions, where troop types offer unique strengths (e.g., Cavalry speed), ranks reflect experience, and special abilities introduce unique tactics, making battles more than stat comparisons and encouraging strategic recruitment and leveling.

**Implementation**:
- Data class with public properties for stats (Damage, Health, Armor, Speed), progression (Level, Experience), and features (TroopType, Rank, SpecialAbilities, SkillBook). Data flow: Constructor sets base stats by type (e.g., Infantry: 50 HP, 10 DMG), generates name, initializes abilities, and loads image via GetTroopImage() from Resources. GainExperience() triggers LevelUp() for stat boosts (type-specific, e.g., Infantry +10 HP), UpdateRank() adjusts rank based on level, and reassigns image. Special abilities are initialized per type (e.g., Infantry: Shield Wall for defense).
- Integration points: Used in Party.cs for troop lists; Battle.cs accesses stats for combat; Settlement recruitment populates AvailableUnits; images loaded for UI (TroopCard).
- Performance considerations: Stat updates are O(1), efficient for frequent calls; image loading from Resources may cache well but could lag on first load; ability lists scale with troop count.
- Optimization opportunities: Precompute stat totals; lazy-load images; pool ability effects to reduce instantiation.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Base stats affect troop roles (e.g., higher Archer damage for ranged focus), influencing army balance; level-up bonuses drive progression pacing, with type-specific boosts encouraging specialization.

#### TroopTypeEnum.cs
**Purpose**: Enumerates troop types to categorize units, enabling type-based mechanics like terrain bonuses and abilities. This provides unit variety for tactical depth, allowing players to build diverse armies where each type excels in specific scenarios (e.g., Engineers in swamps), enhancing strategic choices in recruitment and battle planning.

**Implementation**:
- Simple enum with Infantry, Cavalry, Archer, Engineer. Data flow: Used in Troop.cs constructor for stat/ability initialization, Battle.cs for terrain modifiers (e.g., Archer bonus in Forest), and Formation.cs implicitly for type-specific tactics.
- Integration points: Referenced in Troop.cs, Battle.cs, and settlement recruitment; affects UI displays and AI party composition.
- Performance considerations: Enum comparisons are O(1), negligible impact.
- Optimization opportunities: Minimal, as it's a lightweight enum.

**Unity Editor Modifications**:
- No direct inspector variables (enum only)

**Effects of Changes**:
- Adding/removing types requires updating dependent code (e.g., stat tables in Troop.cs), potentially breaking balance if not adjusted; type variety directly impacts gameplay depth.

#### EnumTroopRanks.cs
**Purpose**: Defines rank progression for troops, simulating experience and specialization. This adds long-term investment to troop management, where higher ranks offer better stats and unlocks, encouraging players to nurture units through battles, tying into the RPG's progression theme.

**Implementation**:
- Enum with type-specific ranks (e.g., Infantry: Recruit to Champion). Data flow: Troop.UpdateRank() sets rank based on level (switch statement), triggering image reassignments and potential stat tweaks; ranks influence UI and recruitment costs.
- Integration points: Used in Troop.cs for progression; affects TroopCard displays and settlement AvailableUnits.
- Performance considerations: Rank updates are infrequent (on level-up), O(1).
- Optimization opportunities: Cache rank strings for UI to avoid repeated ToString().

**Unity Editor Modifications**:
- No direct inspector variables (enum only)

**Effects of Changes**:
- Rank thresholds affect leveling speed; type-specific ranks add flavor but require consistent stat scaling to maintain balance.

#### Formation.cs
**Purpose**: Provides tactical formations for parties, offering bonuses/penalties to simulate battlefield positioning. This enhances combat strategy by allowing players to choose formations (e.g., Wedge for offense), adding depth beyond individual troop stats and encouraging adaptive tactics in response to terrain or enemy composition.

**Implementation**:
- Data class with FormationType enum (Line, Square, Wedge, Circle, Skirmish) and bonuses (AttackBonus, DefenseBonus, SpeedPenalty). Data flow: Constructor sets bonuses via SetFormationBonuses() (e.g., Wedge: +40% attack, -30% defense); intended for Party.cs integration to apply modifiers in battles.
- Integration points: Placeholder in Battle.GetFormationModifier(); could extend to Party.cs for movement/combat effects.
- Performance considerations: Bonuses are static lookups, O(1); memory minimal.
- Optimization opportunities: Precompute combined modifiers if formations stack.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Bonus values directly influence strategy (e.g., higher attack bonuses favor aggressive play), requiring balancing to prevent dominant formations.

#### SpecialAbility.cs
**Purpose**: Implements unique abilities for troops, adding tactical variety and replayability. This system allows units to perform special actions (e.g., Charge for Cavalry), differentiating troop types and enabling emergent strategies in battles, where cooldowns prevent spamming and encourage timing.

**Implementation**:
- Data class with properties for Name, Description, Type (Offensive/Defensive/Support), Cooldown, and Effect (Action delegate). Data flow: UseAbility() checks cooldown, invokes Effect (e.g., Shield Wall increases Defense), and resets cooldown; ReduceCooldown() decrements over time. Abilities initialized in Troop.cs per type.
- Integration points: Used in Troop.cs for ability lists; could be triggered in Battle.cs or UI.
- Performance considerations: Delegate invocations are lightweight; cooldown tracking scales with ability count.
- Optimization opportunities: Pool effects to reduce GC; cache ability states.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Cooldown durations affect ability frequency, influencing tactics; effect strengths impact balance, requiring tuning for fairness.

#### TroopCard.cs
**Purpose**: Serves as a UI component for displaying troop information, enhancing player management and decision-making. This system provides visual feedback on troop stats and images, aiding in recruitment, party composition, and battle planning, making the game more accessible and engaging.

**Implementation**:
- MonoBehaviour with UI references (Image for sprite, TextMeshProUGUI for name/stats). Data flow: SetUpTroopCard() assigns troop data, loads image via Troop.GetTroopImage(), and formats stats (Level, Health, Damage, Defense, Speed).
- Integration points: Used in UI panels (e.g., TroopSelectionPanel); pulls from Troop.cs for data.
- Performance considerations: UI updates are on-demand, efficient; image loading may cache.
- Optimization opportunities: Pool TroopCard instances; lazy-load images.

**Unity Editor Modifications**:
- `troopImage` - Image component for troop sprite
- `troopName` - TextMeshProUGUI for name
- `troopStats` - TextMeshProUGUI for stats display

**Effects of Changes**:
- Stat display clarity affects usability; image quality impacts visual appeal and memory usage.

### Economy and Resources

#### Market.cs
**Purpose**: Manages dynamic market economies in settlements, simulating supply and demand to create player-driven trade opportunities. This system encourages exploration and strategic resource management by making prices fluctuate based on market activity, fostering emergent gameplay where player actions (e.g., over-supplying a resource) can influence global economies. Without dynamic pricing, trading would feel static and less engaging, reducing the RPG's simulation depth.

**Implementation**:
- Utilizes a data class with lists for available items, supply/demand dictionaries, and prices. Data flow: UpdatePrices() iterates through availableItems, calculating prices using a simple formula (basePrice + (demand - supply) * 10), ensuring minimum price of 1. AddItem() initializes new items with zero supply/demand and triggers price updates. GetPrice() retrieves or falls back to base value.
- Integration points: Used in Town.cs for market initialization and price queries; supply/demand could be updated by player trades or settlement production (placeholder in code).
- Performance considerations: UpdatePrices() scales O(n) with item count, efficient for small markets (e.g., 10-20 items); memory for dictionaries grows with items but is lightweight.
- Optimization opportunities: Cache prices to avoid recalculating on every query; use events for price changes to notify UI; implement historical trends for more realistic fluctuations.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Adjusting the price formula multiplier (currently 10) changes volatility—higher values create more extreme price swings, affecting trade profitability and player strategies.

#### Resources.cs
**Purpose**: Provides a foundational data structure for basic resources, enabling tracking of quantities and types across the game world. This abstraction supports resource management in settlements, parties, and crafting, allowing for scarcity-driven gameplay where resource availability influences decisions. Without this class, resource handling would be inconsistent, leading to bugs and reduced economic depth.

**Implementation**:
- Simple data class with properties for amount, type, and name (auto-set from type). Data flow: Constructor initializes with amount and type, setting name via ToString(). No methods beyond getters/setters.
- Integration points: Used in AdvancedResources.cs for input requirements; referenced in settlement production and party consumption (assumed in Party.cs).
- Performance considerations: Lightweight with no operations, minimal memory impact.
- Optimization opportunities: Minimal, as it's a basic struct-like class; consider making it a struct for better performance if instantiated frequently.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Resource types (via ResourceTypeEnum) determine crafting possibilities; amount caps could be added to prevent exploits but may frustrate players if too low.

#### AdvancedResources.cs
**Purpose**: Enables advanced crafting mechanics requiring multiple resource inputs, adding depth to resource management and production. This system simulates workshop-based manufacturing, where player investments in buildings yield higher-tier resources, encouraging long-term settlement development. Without advanced resources, crafting would be linear, reducing replayability and strategic variety in economy building.

**Implementation**:
- Data class with properties for name, type, inputs (two Resources), amounts, and production. Data flow: Constructor sets workshop-level-dependent min/max inputs (e.g., level 1: 2-50, level 3: 2-200); MakeAdvancedResource() checks input availability, consumes resources, and increments output. Input amounts are tracked separately for current vs. required.
- Integration points: Presumably used in settlement buildings for production; inputs reference Resources.cs; workshop levels tie into settlement upgrades.
- Performance considerations: MakeAdvancedResource() is O(1), efficient for infrequent calls; memory scales with input tracking but low.
- Optimization opportunities: Batch production for multiple units; validate inputs on assignment to prevent runtime errors.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Workshop level scaling affects input requirements and production amounts, influencing crafting depth and resource scarcity; higher levels enable more efficient production but require advanced buildings, impacting player progression and economy balance.

### Skill and Progression System

#### Skill.cs
**Purpose**: Defines individual skills with mana costs, effects, and progression, enabling character specialization and tactical combat options. This system adds depth to character builds by allowing players to choose skills that enhance stats or provide unique abilities, fostering replayability through different playstyles. Without skills, combat would be purely stat-based, reducing strategic variety and player expression.

**Implementation**:
- Data class with properties for type, level, name, description, modifier, cost, and passive flag. Data flow: Constructor initializes with parameters, setting level to 1; UseSkill() checks mana, applies effects via ApplyEffect() (switch on SkillType for stat boosts or healing/damage), and logs usage. ApplyEffect() modifies user stats temporarily (e.g., Warrior increases attack, Mage heals or damages). LevelUp() increments level and modifier (+0.1 per level).
- Integration points: Used in SkillBook for organization; effects applied to Characters.cs instances; passive skills could auto-apply (placeholder in code).
- Performance considerations: UseSkill() is O(1), efficient for frequent calls; stat modifications are lightweight.
- Optimization opportunities: Cache modified stats to avoid recalculating; use events for skill effects to notify UI.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Mana costs balance skill frequency—higher costs encourage strategic use; modifier increases affect power scaling, requiring tuning to prevent overpowered builds.

#### SkillBook.cs
**Purpose**: Organizes skills into categorized collections, enabling class-based progression and active/passive distinctions. This system supports character customization by grouping skills logically, allowing players to build specialized characters (e.g., offensive Warrior vs. defensive). Without organization, skill management would be chaotic, hindering usability and balance.

**Implementation**:
- Data class with lists for all skills, active/passive, and class-specific (warrior, mage, rogue). Data flow: Constructor initializes empty lists and calls InitializeSkills() to populate with predefined skills (e.g., Warrior: Power Strike, Shield Bash, Berserker Rage passive). AddSkill() categorizes by type and passive flag; RemoveSkill() removes from all lists. GetSkillsByType() returns class-specific lists; GetActiveSkills()/GetPassiveSkills() filter accordingly.
- Integration points: Owned by Characters.cs; skills added via SkillTree learning; used in UI for skill selection.
- Performance considerations: List operations (add/remove) are O(1) amortized; initialization is one-time.
- Optimization opportunities: Use dictionaries for faster type-based lookups; lazy-initialize skills if not all are needed.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Predefined skills in InitializeSkills() determine class variety—adding/removing affects balance; active/passive separation influences build strategies.

#### SkillTree.cs
**Purpose**: Provides hierarchical skill trees with prerequisites, simulating long-term character development and specialization. This system encourages investment in specific paths (e.g., Mage focusing on offense vs. support), adding progression depth and replayability. Without trees, skills would be unlocked arbitrarily, reducing strategic planning.

**Implementation**:
- Data class with dictionary mapping SkillType to skill lists. Data flow: Constructor initializes empty dictionary and calls InitializeSkillTrees() to populate with class-specific trees (e.g., Warrior: Power Strike, Shield Bash, Berserker Rage, Charge, Fortress). CanLearnSkill() checks character level >= skill level; LearnSkill() adds to character SkillBook if eligible.
- Integration points: Used by Characters.cs for learning; trees define available skills per class.
- Performance considerations: Dictionary lookups O(1); initialization O(n) for skills but one-time.
- Optimization opportunities: Cache learned skills; implement true prerequisites (e.g., skill dependencies) beyond level checks.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Tree depth (number of skills) affects progression pacing; level requirements control unlock speed, influencing early/mid-game power.

### Quest System

#### Quest.cs
**Purpose**: Defines the core structure of quests as player-driven objectives, providing narrative structure and progression milestones in the living world RPG. This system enables storytelling through sequential tasks, rewards player investment with tangible benefits, and supports replayability by allowing multiple active quests. Without quests, the game would lack directed goals, reducing player engagement and the sense of accomplishment from completing challenges. It fosters emergent narratives by tying objectives to world elements like settlements, factions, or resources, making the world feel purposeful and interconnected.

**Implementation**:
- Data class with serializable properties for id (unique identifier), title, description, status (enum: NotStarted, InProgress, Completed, Failed), difficulty (enum: Easy, Normal, Hard), level (player level scaling), objectives (List<QuestObjective>), and rewards (List<string>). Data flow: Constructor initializes with id, title, description, and sets default status to NotStarted, difficulty to Normal, level to 1, and empty lists. AddObjective() appends new QuestObjective instances; AddReward() adds string-based rewards (e.g., "Gold: 100"). IsCompleted() checks if all objectives are completed using LINQ All().
- QuestObjective nested class tracks individual tasks with description and completed bool; constructor sets description and defaults completed to false.
- Integration points: Generated by QuestGenerator; managed by QuestManager for status updates and completion; rewards distributed to WorldHandler.Player.Party.Gold.
- Performance considerations: List operations are O(1) amortized for additions, but IsCompleted() iterates objectives (O(n)), efficient for small quest sizes (1-4 objectives). Memory usage scales with objective/reward counts but is lightweight.
- Optimization opportunities: Cache completion status to avoid repeated iterations; use HashSets for rewards if frequent lookups are needed; serialize quests for save/load integration.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Increasing objective counts adds complexity but may frustrate if too many; reward strings (e.g., gold amounts) directly impact player economy, requiring balancing to avoid inflation.

#### QuestManager.cs
**Purpose**: Acts as a centralized static manager for quest lifecycle management, ensuring quests are tracked, updated, and resolved consistently across the game world. This system provides a hub for quest progression, enabling dynamic storytelling where player actions trigger objective completions and rewards, enhancing immersion in a living world. Without centralized management, quests would be fragmented, leading to inconsistencies in status tracking and reward distribution.

**Implementation**:
- Static class with public static List<Quest> activeQuests for global access. Data flow: StartQuest() sets quest.status to InProgress and adds to activeQuests. CompleteQuest() sets status to Completed, distributes rewards (parses "Gold: X" strings to add to WorldHandler.Player.Party.Gold), removes from activeQuests, and triggers AudioManager.Instance.PlayQuestCompleteSound() for feedback. UpdateObjective() checks objective index validity, sets completed to true, and calls CompleteQuest() if all objectives are done.
- Integration points: Called by external systems (e.g., settlement interactions or event triggers) to start/update quests; rewards affect Party gold; audio enhances user feedback.
- Performance considerations: List operations (add/remove) are O(1) amortized; activeQuests iteration in potential queries scales O(n) with active quests, but typically low (e.g., 5-10). Memory for active quests grows linearly.
- Optimization opportunities: Use a Dictionary<string, Quest> for id-based lookups (O(1) vs. O(n) searches); batch reward distributions; integrate with event system for objective updates to reduce polling.

**Unity Editor Modifications**:
- No direct inspector variables (static manager class)

**Effects of Changes**:
- Reward parsing logic assumes "Gold: X" format—changing this requires updating dependent code; audio feedback enhances satisfaction but may be skipped if AudioManager is null, affecting immersion.

#### QuestGenerator.cs
**Purpose**: Enables procedural generation of quests to create varied, replayable content that adapts to player progression, ensuring no two playthroughs have identical objectives. This system adds depth to the living world by generating contextually relevant tasks (e.g., based on player level), promoting exploration and strategic decision-making. Without procedural generation, quests would be static, reducing long-term engagement and the organic feel of the world.

**Implementation**:
- Static class with predefined arrays for questTitles (e.g., "Retrieve the Artifact") and objectiveDescriptions (e.g., "Kill enemies"). Data flow: GenerateQuest(int playerLevel) selects random difficulty (0-2 enum), calculates level (playerLevel + offset based on difficulty, clamped to 1+), picks random title, creates Quest with unique id (using Random.Range), sets difficulty/level, generates 1-3 random objectives via AddObjective(), and adds gold reward ("Gold: " + level * 10 * (difficulty+1)).
- Integration points: Called by systems needing new quests (e.g., settlement NPCs or world events); generated quests fed into QuestManager.StartQuest().
- Performance considerations: Random selections are O(1), efficient for on-demand generation; no persistent state, minimal memory impact.
- Optimization opportunities: Expand title/objective arrays for more variety; add thematic generation (e.g., faction-specific quests); cache generated quests if frequent calls occur.

**Unity Editor Modifications**:
- No direct inspector variables (static generator class)

**Effects of Changes**:
- Reward formula (level * 10 * (difficulty+1)) scales with difficulty—higher multipliers increase rewards but may unbalance economy; objective count randomness affects quest length and pacing.

### Time and Weather System

#### TimeManager.cs
**Purpose**: Provides a realistic simulation of time progression in the living world RPG, enabling scheduling of events, day/night cycles, and long-term world evolution. This system creates a sense of passage of time, allowing players to experience seasonal changes, daily routines, and time-sensitive quests, making the world feel dynamic and immersive. Without time management, the game would lack temporal depth, reducing replayability and the organic feel of a living world where events unfold over days, months, or years.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: Update() accumulates timer based on Time.deltaTime * timeScale (default 60f, 1 real second = 1 game minute), advancing minute/hour/day/month/year. Triggers OnHourChanged/OnDayChanged events for system integration. Calculates timeOfDay fraction (0-1) for lighting updates via LightingManager.Instance.UpdateLightingBasedOnTime(). Includes DaysInMonth() and IsLeapYear() for accurate calendar progression (e.g., February 28/29 days).
- Integration points: LightingManager for time-based lighting; WeatherManager scales weatherDuration by timeScale; WorldEventManager or quests can subscribe to time events for scheduling.
- Performance considerations: Update() runs every frame, but operations are lightweight (arithmetic only); memory usage minimal (few int/float variables).
- Optimization opportunities: Cache timeOfDay to avoid recalculating; use coroutines for less frequent updates if timeScale is high; integrate with Unity's Time.timeScale for pausing.

**Unity Editor Modifications**:
- `timeScale` (default: 60f) - Multiplier for game time speed (higher values accelerate time)

**Effects of Changes**:
- Increasing timeScale speeds up gameplay pacing, potentially making daily activities feel rushed; affects weather change frequency and event scheduling, requiring balancing to maintain immersion.

#### WeatherManager.cs
**Purpose**: Introduces environmental variety and atmospheric effects through random weather transitions, enhancing the living world's realism and influencing gameplay decisions. This system adds unpredictability, where weather can affect visibility, movement, or combat, encouraging adaptive strategies and making exploration more engaging. Without weather, the world would feel static and less immersive, lacking the dynamic elements that tie into player choices and world events.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: Start() initializes random weather; Update() increments weatherTimer by Time.deltaTime * TimeManager.Instance.timeScale, triggering SetRandomWeather() when weatherDuration (default 3600f game seconds) is reached. SetRandomWeather() selects random WeatherType from enum (Clear, Rain, Storm, Snow, Fog) and invokes OnWeatherChanged event.
- Integration points: TimeManager scales duration; systems like Battle.cs or Movement can subscribe to OnWeatherChanged for modifiers (e.g., reduced visibility in Fog); WorldEventManager for weather-triggered events.
- Performance considerations: Update() is frame-based but simple (timer increment); enum selection is O(1); memory usage low (enum and float).
- Optimization opportunities: Precompute random weather sequences for predictability; cache current weather to avoid redundant events; integrate weather effects with particle systems for visual feedback.

**Unity Editor Modifications**:
- `weatherDuration` (default: 3600f) - Interval in game seconds between weather changes

**Effects of Changes**:
- Shorter weatherDuration increases frequency, adding dynamism but potentially overwhelming players; longer durations create stability, reducing environmental variety and strategic depth.

#### LightingManager.cs
**Purpose**: Manages visual atmosphere through time-based lighting adjustments, simulating day/night cycles to enhance immersion and mood. This system provides visual feedback for time progression, affecting player perception of safety, exploration, and aesthetics in the living world. Without dynamic lighting, the game would lack visual cues for time, making the world feel flat and less engaging.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: UpdateLightingBasedOnTime(float timeOfDay) called by TimeManager, checks if timeOfDay (0-1, where 0.25-0.75 is day) to call SetDayLighting() or SetNightLighting(). SetDayLighting() sets directionalLight intensity to dayIntensity (1f) and color to dayColor (white); SetNightLighting() to nightIntensity (0.3f) and nightColor (blue-tinted).
- Integration points: Called by TimeManager on hour changes; affects rendering and player visibility; could integrate with post-processing for advanced effects.
- Performance considerations: Method calls are infrequent (hourly), lightweight (property sets); memory usage minimal (Light reference and floats/colors).
- Optimization opportunities: Use Unity's Timeline or Animation for smoother transitions; cache light references; integrate with weather for combined effects (e.g., dimmer light in Storm).

**Unity Editor Modifications**:
- `directionalLight` - Reference to the scene's main directional light
- `dayIntensity` (default: 1f) - Intensity during day
- `nightIntensity` (default: 0.3f) - Intensity during night
- `dayColor` - Color during day
- `nightColor` - Color during night

**Effects of Changes**:
- Adjusting intensities affects visibility and atmosphere—lower nightIntensity increases stealth opportunities but may frustrate navigation; color changes influence mood, with warmer day colors feeling inviting and cooler night colors evoking danger.

### Audio System

#### AudioManager.cs
**Purpose**: Enhances player immersion and provides auditory feedback through dynamic audio management, creating an engaging atmosphere in the living world RPG. This system delivers contextual sound cues for actions, events, and environments, making gameplay more responsive and emotionally impactful. By layering audio types (music, ambient, effects), it supports varied experiences, from tense battle soundtracks to serene exploration ambiances, fostering deeper player connection. Without audio feedback, the game would feel flat and less interactive, reducing the sense of presence in a dynamic world where sounds signal victories, dangers, or discoveries.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance and DontDestroyOnLoad for persistence across scenes. Manages three AudioSource components: backgroundMusicSource (looping for continuous music), ambientSoundSource (looping for environmental sounds), and soundEffectsSource (one-shot for transient effects). Data flow: Awake() sets up sources if not assigned, then Update() continuously adjusts volumes based on masterVolume * specificVolume (e.g., musicVolume for background). Play methods (e.g., PlayBackgroundMusic, PlaySoundEffect) assign clips and trigger playback, with volume scaling applied. Volume layering allows independent control: masterVolume affects all, while musicVolume, ambientVolume, and effectsVolume fine-tune categories.
- Integration points: Called by Battle.cs for attack/hit/death sounds, QuestManager for completion audio, and UI systems for click/menu sounds. Audio clips are assigned in inspector for game events (e.g., victorySound, uiClickSound).
- Performance considerations: Lightweight operations (volume updates per frame), minimal CPU impact as Unity handles AudioSource playback. Memory usage scales with clip sizes but is low for short effects; persistent sources avoid instantiation overhead.
- Optimization opportunities: Implement audio pooling for frequent one-shot effects to reduce AudioSource creation/destruction; preload clips to avoid load times; use Unity's AudioMixer for advanced grouping and effects.

**Unity Editor Modifications**:
- `backgroundMusicSource` - AudioSource for looping background music
- `ambientSoundSource` - AudioSource for looping ambient sounds
- `soundEffectsSource` - AudioSource for one-shot effects
- `masterVolume` (0-1) - Overall volume multiplier
- `musicVolume` (0-1) - Background music volume
- `ambientVolume` (0-1) - Ambient sound volume
- `effectsVolume` (0-1) - Sound effects volume
- Audio clip assignments (e.g., `attackSound`, `victorySound`, `uiClickSound`) for various game events

**Effects of Changes**:
- Volume adjustments directly influence immersion—low masterVolume may reduce engagement, while high effectsVolume enhances feedback but could overwhelm. Clip quality affects memory and realism; poor audio may detract from atmosphere, while high-fidelity clips improve polish but increase load times.

### Save/Load System

#### SaveLoadManager.cs
**Purpose**: Ensures game state persistence, allowing players to save progress and resume gameplay across sessions, fostering long-term engagement and continuity in the living world RPG. This system is vital for player investment, as it preserves character progression, inventory, equipment, and world state, preventing loss of hours of play due to interruptions. Without save/load functionality, the game would feel ephemeral, reducing replayability and player retention in a complex RPG where decisions have lasting impacts.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance and preventing duplicates via Destroy(). Static methods (SaveGame, LoadGame) delegate to private instance methods for thread-safe operations. Data flow: SaveGameImpl() serializes GameData to JSON using JsonUtility.ToJson() with pretty-printing, writes to file via File.WriteAllText() at SaveFilePath (Application.persistentDataPath + "/savegame.json"). LoadGameImpl() checks file existence, reads JSON with File.ReadAllText(), deserializes via JsonUtility.FromJson<GameData>(), and returns data or null if missing. GameData class encapsulates serializable fields like player stats (health, mana, gold, level, experience), inventory/equipment references, and position (Vector3).
- Integration points: Invoked by GameManager for keyboard-triggered save/load (e.g., Ctrl+S to save, Ctrl+L to load); data populated from WorldHandler.Player and related systems; loaded data restores player state, inventory, and position.
- Performance considerations: JSON serialization/deserialization is lightweight for small data sets but scales poorly with complex objects (e.g., large inventories); file I/O is synchronous, potentially blocking main thread on large saves. Memory usage minimal, as operations are on-demand.
- Optimization opportunities: Switch to binary serialization (e.g., BinaryFormatter) for faster I/O and smaller files; implement asynchronous saving/loading with coroutines to avoid frame drops; compress JSON or use custom serializers for large data; add checksums for data integrity.

**Unity Editor Modifications**:
- SaveFilePath property (computed as Application.persistentDataPath + "/savegame.json") - Configurable via code for custom paths, but no direct inspector variable.

**Effects of Changes**:
- Save frequency (e.g., manual vs. auto-save) affects data loss risk—infrequent saves may frustrate players on crashes, while frequent ones impact performance. File format (JSON) ensures human-readability for debugging but limits compatibility with other formats; changing to binary improves speed but reduces readability. Data fields in GameData directly influence what is preserved, with missing fields (e.g., no party data) potentially breaking continuity if expanded.

### UI System

### GameManager.cs
**Purpose**: Serves as the central input handler and UI manager for the living world RPG, enabling player control over core game functions like saving/loading progress, inventory inspection, and UI toggling. This system ensures seamless player interaction with the game's persistent state and interface, allowing quick access to essential features without disrupting gameplay. Without this manager, players would lack intuitive controls for managing their game state, leading to frustration and reduced engagement in a complex RPG where saving progress is critical.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance, allowing global access for input handling and UI management.
- Data flow: Update() checks for key presses (F5 for save, F9 for load, I for inventory log, Escape for panel toggle), delegating to SaveGame()/LoadGame() or LogInventory(). SaveGame() collects player data (name, stats, inventory, equipment, position) from WorldHandler.Player, creates GameData, and calls SaveLoadManager.SaveGame(). LoadGame() loads GameData, restores player state to WorldHandler.Player, and repositions the player transform (using assigned playerTransform or finding by tag "Player"), resetting Rigidbody velocity for smooth movement. LogInventory() iterates through WorldHandler.Player.Inventory.items and Equipment.equippedItems, logging to console for debugging.
- Integration points: Relies on WorldHandler for player data access, SaveLoadManager for serialization, and Unity's Input system for key detection. Position saving/loading uses Transform.position, with fallbacks for robustness.
- Performance considerations: Update() runs every frame but only processes on key down (lightweight), no ongoing computations; save/load operations are on-demand, potentially blocking if large data (e.g., extensive inventory), but typically fast for JSON serialization.
- Optimization opportunities: Cache player references to avoid repeated WorldHandler.Player lookups; implement asynchronous saving to prevent frame drops; add input remapping for accessibility.

**Unity Editor Modifications**:
- `saveLoadPanel` - GameObject reference for the save/load UI panel (toggled on Escape)
- `playerTransform` - Transform reference for player position saving/loading (optional, falls back to GameObject.FindGameObjectWithTag("Player"))

**Effects of Changes**:
- Modifying key codes (e.g., F5 to another key) changes player shortcuts, affecting usability—non-standard keys may confuse users familiar with common save/load bindings. Panel references ensure UI toggling works; missing refs disable features, impacting accessibility. Position saving enhances immersion by preserving location, but errors (e.g., no player found) could lead to lost progress if not handled gracefully.

### World Events System

#### WorldEventManager.cs
**Purpose**: Introduces dynamic, emergent events to simulate a living world, where random occurrences like wars, sieges, quests, and growth drive narrative progression and player challenges. This system creates unpredictability and replayability by triggering faction conflicts, settlement changes, and player opportunities, making the world feel organic and responsive to time rather than scripted. Without this manager, the world would be static, lacking the tension and variety from events that affect settlements, factions, and player quests, reducing immersion in a strategy RPG. It enables emergent storytelling by linking events to world state, encouraging adaptive player strategies and fostering a sense of consequence in a procedurally generated environment.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance and preventing duplicates via Destroy(), allowing global access for event management. Data flow: Start() subscribes to TimeManager.OnDayChanged for daily processing. ProcessDailyEvents() decrements active event durations, resolves completed events (e.g., ending sieges by resetting flags), and triggers new events with 10% daily chance via Random.value. TriggerRandomEvent() selects random WorldEventType from enum (War, Quest, Siege, Growth), random factionId from WorldHandler.Factions, and location via GetRandomSettlementLocation(), then executes type-specific logic: War updates FactionDiplomacy for hostilities; Quest generates and starts a quest via QuestManager; Siege sets settlement siege flags and applies damage; Growth boosts villages via ProcessGrowthEvent(). Events are stored in List<WorldEvent> with properties for type, description, duration (in days), factionId, and location (Vector3Int). ResolveEvent() handles end-of-event cleanup, such as ending wars or sieges.
- Integration points: Subscribes to TimeManager for time-based triggers; affects settlements (e.g., Castle.IsCastleUnderSiege, Town.IsTownUnderSiege, Village.ProcessSiegeEvent/GrowthEvent), factions (via FactionDiplomacy for war declarations), quests (generates via QuestGenerator and starts via QuestManager), and parties (implicitly through world changes like sieges). GetRandomSettlementLocation() aggregates faction settlements (castles, towns, villages) for random selection, ensuring events target valid locations.
- Performance considerations: ProcessDailyEvents() scales O(n) with active events (typically low, e.g., 1-5), called daily (efficient); random selections and enum lookups are O(1); memory usage for List<WorldEvent> is minimal as events are short-lived. Settlement iteration in GetRandomSettlementLocation() can scale with faction size but is infrequent.
- Optimization opportunities: Cache faction settlement lists to avoid repeated aggregation; implement event pooling for WorldEvent instances to reduce GC; limit active events count to prevent performance dips; use event-driven resolution instead of polling for better responsiveness.

**Unity Editor Modifications**:
- No direct inspector variables (manager class with internal logic and no serialized fields)

**Effects of Changes**:
- Event probabilities (e.g., 10% daily chance in ProcessDailyEvents()) control world dynamism—higher values increase emergent activity and replayability but may overwhelm players with frequent disruptions; lower values create stability, reducing tension and variety. Event durations affect pacing, with longer durations building sustained challenges (e.g., prolonged sieges) but delaying resolutions, impacting player engagement. Type-specific effects (e.g., siege damage via ProcessSiegeEvent() or war declarations) directly influence faction balance, settlement viability, and quest availability, requiring careful tuning to avoid overpowered or unbalanced outcomes; for instance, frequent growth events can accelerate village expansion, altering economic and military scaling.

### Faction and Diplomacy System

#### Factions.cs
**Purpose**: Defines political factions as data containers for territories and settlements, enabling strategic gameplay through ownership and influence. This class is essential for representing the political landscape, allowing players to navigate alliances, conflicts, and territorial control, which adds depth to the RPG's world simulation. Without defined factions, the game would lack political intrigue, reducing strategic decision-making and emergent narratives.

**Implementation**:
- Simple data class with public properties for Name, Center (Vector2), Id, and lists of Castles, Towns, Villages. Uses auto-properties with private backing fields for encapsulation.
- Data flow: FactionMaker populates the lists during assignment, and WorldHandler syncs the global Factions list. Center is set to the average position of owned settlements for influence calculations.
- Integration points: Referenced by FactionDiplomacy for relationship tracking and WorldEventManager for faction-based events.
- Performance considerations: Lists grow with settlement counts, but operations are lightweight (e.g., adding/removing settlements is O(1)). Memory usage scales linearly with faction and settlement sizes.
- Optimization opportunities: Use HashSets for settlement lists if frequent lookups are needed; cache center calculations to avoid recomputing on every access.

**Unity Editor Modifications**:
- Faction properties (name, center, ID)

**Effects of Changes**:
- Faction count affects political complexity
- Territory size impacts power balance

#### FactionMaker.cs
**Purpose**: Creates and assigns settlements to factions using a flood-fill algorithm, establishing political territories that drive strategic depth and emergent conflicts. This system transforms procedural settlements into meaningful political entities, enabling diplomacy, wars, and alliances that make the world feel dynamic and player-influential. Without faction assignment, settlements would be isolated, reducing the RPG's strategic layers and narrative potential. It lays the groundwork for faction-based quests, trade restrictions, and territorial conquests, enhancing long-term engagement.

**Implementation**:
- Employs a singleton MonoBehaviour pattern with Awake() ensuring a single instance for global faction management.
- Data flow: AssignFactions() combines all settlements (castles, towns, villages) into a unified dictionary, then selects random seed positions for each faction, assigning centers and initiating a BFS (breadth-first search) flood-fill expansion.
- Flood-fill algorithm: Uses a queue to expand from seeds, assigning adjacent unassigned settlements to the same faction, ensuring contiguous territories without "border gore" (disconnected enclaves). Leftover settlements are assigned randomly to prevent unclaimed areas.
- Integration points: Populates Faction objects with settlement lists (Castles, Towns, Villages) and syncs with WorldHandler.Factions for global access. Triggers FactionDiplomacy initialization post-assignment.
- Performance considerations: Algorithm scales with settlement count (O(n) for BFS traversal), efficient for typical worlds (e.g., 100+ settlements) but could slow with excessive factions or dense maps. Memory usage for queues and dictionaries is minimal.
- Optimization opportunities: Precompute settlement adjacency graphs for faster lookups; limit expansion depth to create more varied territory shapes; use multi-threading for large-scale assignments.

**Unity Editor Modifications**:
- `numberOfFactions` (default: 5) - Number of factions to create

**Effects of Changes**:
- Faction count affects world division
- Assignment algorithm impacts territory shapes

#### FactionDiplomacy.cs
**Purpose**: Simulates political relationships between factions, enabling dynamic alliances, wars, and hostilities that create emergent conflicts and strategic opportunities. This system adds realism and replayability by allowing factions to form pacts or declare war based on procedural logic, influencing player choices in diplomacy, trade, and conquest. Without diplomacy management, factions would be static, reducing the game's political depth and narrative variety.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance; uses a Dictionary<(int, int), DiplomacyStatus> for symmetric relationship matrix (e.g., (A,B) and (B,A) share status).
- Data flow: InitializeDiplomacy() sets all pairs to Neutral, then randomly assigns 20% alliances and 10% hostilities for initial variety. Methods like DeclareWar() and FormAlliance() update the matrix and log changes.
- Integration points: Called by WorldGenerator post-faction creation; queried by WorldEventManager for war events (e.g., random faction wars).
- Performance considerations: Dictionary lookups are O(1), efficient for small faction counts (e.g., 5-10 factions); initialization scales with faction pairs (O(n²)) but is one-time.
- Optimization opportunities: Use a 2D array for matrix if faction IDs are contiguous; cache allied/at-war lists to avoid repeated queries; add event-driven updates for real-time diplomacy changes.

**Unity Editor Modifications**:
- Diplomacy initialization parameters

**Effects of Changes**:
- Relationship states affect AI behavior
- Diplomacy options impact strategy depth

### NPC and AI System

#### NPC.cs
**Purpose**: Defines non-player characters as extensions of the Characters class, enabling dynamic NPCs with activities, faction ties, and procedural generation. This system adds life to settlements by simulating daily routines, faction interactions, and potential quests, making the world feel populated and immersive. Without NPCs, settlements would be static, reducing the RPG's social and narrative depth. It supports emergent storytelling through NPC-driven events and player interactions.

**Implementation**:
- Inherits from Characters.cs, extending with NPC-specific properties (npcName, factionId, gridPosition) and activity state (CurrentActivity enum: Sleeping, Working, Wandering, Eating). Data flow: Constructor initializes with random stats (health 100-200, mana 50-100, level 1-10, gold 1000-2000), generates name via NameGenerator, and sets default activity to Wandering. Initialize() method sets name, faction, and position for placement.
- Integration points: Managed by NPCManager for activity updates; factionId links to FactionDiplomacy for allegiance; gridPosition used for settlement placement and pathfinding.
- Performance considerations: Constructor randomization is O(1), efficient for spawning; memory usage low per NPC (inherits from Characters).
- Optimization opportunities: Pool NPC instances to reduce instantiation; lazy-load names if generation is slow; cache faction data to avoid lookups.

**Unity Editor Modifications**:
- No direct inspector variables (data class with constructor parameters)

**Effects of Changes**:
- Random stat ranges affect NPC strength and viability—wider ranges increase variety but may unbalance encounters; activity states influence world simulation, with more states adding realism but requiring more logic.

#### NPCManager.cs
**Purpose**: Oversees NPC lifecycle and activity scheduling, synchronizing NPC behaviors with time progression to create a dynamic, living world. This system ensures NPCs follow realistic daily routines, enhancing immersion and providing opportunities for player interaction. Without management, NPCs would be inert, diminishing the sense of a populated environment and reducing emergent gameplay.

**Implementation**:
- Singleton MonoBehaviour with Awake() ensuring one instance. Data flow: Start() subscribes to TimeManager.OnHourChanged for hourly updates. UpdateNPCActivities() iterates npcs list, setting CurrentActivity based on hour (e.g., Sleeping 22-6, Working 8-18, Eating 12-14, Wandering otherwise).
- Integration points: Subscribes to TimeManager for scheduling; updates NPC.CurrentActivity for behavior changes; npcs list populated by settlement spawners or world generation.
- Performance considerations: UpdateNPCActivities() scales O(n) with NPC count, called hourly (efficient for small populations, e.g., 10-50 per settlement); memory for list grows linearly.
- Optimization opportunities: Batch updates or use event-driven changes; limit NPC counts per settlement; cache hour checks to avoid repeated TimeManager calls.

**Unity Editor Modifications**:
- No direct inspector variables (manager class)

**Effects of Changes**:
- Hourly update frequency controls dynamism—more frequent changes increase activity but may impact performance; NPC count directly affects world density and processing load.

### Graphics and Optimization

#### GraphicsOptimizer.cs
**Purpose**: Manages graphics settings to balance visual quality and performance, allowing players to adjust rendering options for optimal gameplay on various hardware. This system is crucial for accessibility and performance optimization in a living world RPG, where complex scenes (settlements, parties, effects) can strain resources, ensuring smooth frame rates without sacrificing immersion. Without this optimizer, the game might run poorly on lower-end devices, leading to frustration and reduced player retention. It enables dynamic quality adjustments, supporting replayability across different setups and encouraging long-term engagement.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance, allowing global access for settings application. Data flow: Awake() calls ApplySettings() to configure Unity's QualitySettings and Application properties. ApplySettings() sets Application.targetFrameRate, QualitySettings.vSyncCount (1 for enabled, 0 for disabled), QualitySettings.shadows (All or Disable), QualitySettings.shadowResolution, and QualitySettings.pixelLightCount. Preset methods (SetLowQuality(), SetMediumQuality(), SetHighQuality()) adjust variables and reapply settings, providing quick quality switches (e.g., Low disables shadows, sets low resolution, reduces lights and FPS to 30).
- Integration points: Called on startup; could be integrated with UI menus for runtime adjustments; affects rendering pipeline globally, impacting all scenes and objects.
- Performance considerations: Settings changes are lightweight (property sets), applied once on Awake() or on preset calls; no ongoing updates, minimal CPU impact. Memory usage negligible as it modifies global settings.
- Optimization opportunities: Add runtime UI toggles for dynamic changes without restart; implement adaptive quality based on frame rate monitoring; cache settings for quick restoration.

**Unity Editor Modifications**:
- `targetFrameRate` (default: 60) - Target FPS limit; higher values improve smoothness but may not be achievable on low-end hardware
- `enableVSync` - Vertical sync toggle; enabled prevents screen tearing but may cap FPS below target
- `enableShadows` - Shadow rendering toggle; disabling improves performance but reduces visual depth
- `shadowResolution` - Shadow quality (Low, Medium, High); higher resolutions enhance realism but increase GPU load
- `pixelLightCount` - Dynamic light limit; lower counts boost performance in lit scenes but may darken areas

**Effects of Changes**:
- Frame rate settings directly influence smoothness and responsiveness—higher targetFrameRate improves gameplay feel but may cause overheating on mobile devices; lower rates reduce battery drain but can make actions feel sluggish. Quality presets (e.g., Low vs. High) trade visual fidelity for performance, with High enabling shadows and higher resolutions for immersive lighting but potentially dropping FPS in dense scenes; Low prioritizes speed, suitable for older hardware but diminishing atmosphere. VSync prevents tearing but can introduce input lag, requiring balancing for competitive elements.

#### VisualEffectManager.cs
**Purpose**: Handles particle effects and visual feedback to enhance immersion and communicate game events, providing visual cues for spells, combat, and actions in a living world RPG. This system adds polish and feedback, making magic, battles, and interactions feel impactful and engaging. Without visual effects, the game would lack visual dynamism, reducing player satisfaction and the sense of a vibrant, responsive world. It supports emergent storytelling by visually emphasizing key moments, encouraging replayability through varied, memorable experiences.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance, allowing global access for effect playback. Data flow: Play methods (e.g., PlayFireSpellEffect()) instantiate ParticleSystem prefabs at specified Vector3 positions with Quaternion.identity, then call Play() to start emission. Effects are categorized: Spell effects (fire, ice, magic) for abilities; Combat effects (hit, blood, explosion) for battles. Instantiation creates temporary objects that self-destruct after playback.
- Integration points: Called by Battle.cs for hit/blood effects, Skill.cs for spell effects, and other systems for explosions; positions provided by calling scripts (e.g., troop locations).
- Performance considerations: Instantiation on-demand avoids persistent overhead, but frequent calls (e.g., rapid combat) could create many objects, potentially impacting frame rate if not pooled. Particle systems are GPU-intensive; memory usage scales with active effects.
- Optimization opportunities: Implement object pooling for ParticleSystems to reduce instantiation costs; add effect duration controls; integrate with GraphicsOptimizer for quality scaling (e.g., disable on low settings).

**Unity Editor Modifications**:
- `fireSpellEffect` - ParticleSystem prefab for fire spells; high particle count enhances drama but increases GPU load
- `iceSpellEffect` - ParticleSystem prefab for ice spells; blue-themed particles for thematic consistency
- `magicSpellEffect` - ParticleSystem prefab for generic magic; versatile for various abilities
- `hitEffect` - ParticleSystem prefab for combat impacts; short duration for quick feedback
- `bloodEffect` - ParticleSystem prefab for damage; gore level adjustable for audience sensitivity
- `explosionEffect` - ParticleSystem prefab for large impacts; high emission for spectacle

**Effects of Changes**:
- Effect quality (particle count, duration) affects visual appeal and immersion—higher quality creates more engaging spectacles but may reduce FPS in effect-heavy scenes (e.g., large battles); lower quality maintains performance but diminishes polish. Effect frequency impacts performance and pacing, with more effects enhancing feedback but potentially overwhelming players if overused; sparse effects risk feeling underwhelming. Prefab assignments allow customization, enabling modding for different themes or performance needs.

### Pathfinding and Interactions

#### PathfindingManager.cs
**Purpose**: Provides efficient pathfinding for AI and player movement in a tile-based world, using the A* algorithm to compute optimal paths while avoiding obstacles like settlements. This system enables dynamic navigation for parties, NPCs, and player characters, ensuring realistic movement in a living world where terrain and settlements influence routes. Without pathfinding, entities would move unpredictably or get stuck, breaking immersion and gameplay flow. It supports multi-agent scenarios by allowing concurrent path requests, fostering emergent behaviors in faction conflicts and exploration.

**Implementation**:
- Utilizes a singleton MonoBehaviour pattern with Awake() ensuring one instance, allowing global access for path queries. Data flow: Awake() sets gridSize from WorldGenerator, initializes a 2D Node grid (walkable/obstacle based on settlement positions), and builds the grid via InitGrid(). UpdateGrid() refreshes obstacles when world changes. FindPath() implements A* with open/closed sets using a custom Heap for priority queue, iterating until target is reached or max iterations hit (safety limit). Neighbors are cardinal directions only; distance uses Manhattan heuristic. RetracePath() reconstructs the path from end to start.
- Integration points: Called by MultiAgentPathfinder for AI party movement, NPC_Controller for NPC navigation, and potentially player movement; uses WorldHandler for obstacle data (settlements as non-walkable).
- Performance considerations: Grid initialization scales O(width * height), efficient for 100x100 grids; pathfinding O(b^d) worst-case but optimized with heap and limits; memory for grid array grows quadratically with size.
- Optimization opportunities: Implement multi-threading for concurrent path requests; cache paths for repeated queries; use spatial partitioning for larger grids; add diagonal movement for smoother paths.

**Unity Editor Modifications**:
- `nodeSize` (default: 1f) - World units per grid cell, matching tilemap scale for accurate positioning

**Effects of Changes**:
- Increasing nodeSize reduces grid resolution, speeding up pathfinding but decreasing accuracy (e.g., paths may clip corners); smaller values improve precision but increase computation for large grids. Grid size (derived from WorldGenerator) directly impacts memory and performance—larger worlds require more resources, potentially causing lag in dense areas.

#### PlayerInteraction.cs
**Purpose**: Enables context-sensitive player interactions with world objects, such as settlements or NPCs, by detecting nearby interactable entities and triggering actions on input. This system facilitates exploration and engagement in the living world, allowing players to access menus, quests, or events without complex controls. Without interaction management, players would lack meaningful ways to engage with the world, reducing depth and replayability. It supports modularity through the IInteractable interface, enabling easy addition of new interactive elements.

**Implementation**:
- MonoBehaviour attached to player object. Data flow: Update() checks for interactKey press, uses Physics2D.OverlapCircleAll() to find colliders in interactRadius on interactableLayer, then calls Interact() on the first IInteractable found (prevents multiple simultaneous interactions). OnDrawGizmosSelected() visualizes the radius for debugging.
- Integration points: Relies on IInteractable implementations (e.g., settlement scripts); layer-based detection ensures only relevant objects are checked; could integrate with UI for feedback.
- Performance considerations: OverlapCircleAll() is lightweight for small radii, called only on key press; no ongoing updates, minimal impact.
- Optimization opportunities: Use spatial queries (e.g., quadtree) for larger worlds; add interaction cooldowns to prevent spam; implement priority sorting for multiple interactables.

**Unity Editor Modifications**:
- `interactKey` (default: KeyCode.E) - Key to trigger interaction
- `interactRadius` (default: 1f) - Radius in world units to detect interactables
- `interactableLayer` - LayerMask for filtering objects (e.g., assign 'Interactable' layer to settlements)

**Effects of Changes**:
- Larger interactRadius increases convenience by allowing distant interactions but may lead to accidental triggers in crowded areas; smaller radii require precise positioning, potentially frustrating players. Changing interactKey affects accessibility—common keys like E are intuitive, but conflicts with other inputs could confuse users. Layer settings ensure only intended objects are interactable, but misconfigurations could break interactions or include unintended colliders.

## Data Structures and Enums

### TerrainType
Defines terrain types affecting movement and combat:
- Plain, Forest, Mountain, Desert, Swamp, River, City, Castle, Water, Grass, Hills, Mountains

**Effects**: Terrain modifiers in Battle.cs affect combat outcomes

### TroopType
Defines unit types with different strengths:
- Infantry, Cavalry, Archer, Engineer

**Effects**: Type determines base stats and terrain bonuses

### EnumTroopRanks
Defines rank progression for troops:
- Infantry: Recruit, Soldier, Veteran, Elite, Champion
- Cavalry: Squire, Knight, Cavalier, Lancer, Paladin
- Archer: Novice, Marksman, Sharpshooter, Ranger, MasterArcher
- Engineer: Apprentice, Craftsman, Inventor, Artificer, Genius

**Effects**: Ranks influence stats, UI display, and recruitment costs

### SkillType
Defines character classes for skills:
- Mage, Warrior, Rogue

**Effects**: Determines available skills and effects

### PartyType
Defines party roles and behaviors:
- Trader, ResourceConvoy, Villagers, Bandits, Military, Adventurers, Mercenaries, Player

**Effects**: Influences AI behavior and interactions

### ResourceType
Defines basic resources:
- Wood, Stone, Iron, Gold, Grain, Meat, Fish, Fruit, Vegetable, Food, Water

**Effects**: Used in crafting, consumption, and economy

### WeaponTypes
Defines weapon categories:
- Sword, Axe, Spear, Bow, Crossbow, Dagger, Mace, Staff, Wand, Shield, Gun, GreatSword, Halberd, Flail, Scythe

**Effects**: Determines equipment stats and combat roles

### BuildingType
Defines settlement improvements:
- Barracks, Market, Wall, Tower, Farm, Mine

**Effects**: Provides bonuses to settlements

### FormationType
Defines tactical formations for parties:
- Line, Square, Wedge, Circle, Skirmish

**Effects**: Bonuses/penalties to attack, defense, and speed in battles

### DiplomacyStatus
Defines relationships between factions:
- Neutral, Allied, AtWar

**Effects**: Influences AI behavior, trade, and event triggering

### QuestStatus
Defines quest states:
- NotStarted, InProgress, Completed, Failed

**Effects**: Tracks quest progression and availability

### QuestDifficulty
Defines quest challenge levels:
- Easy, Normal, Hard

**Effects**: Scales rewards and objectives

### WeatherType
Defines weather conditions:
- Clear, Rain, Storm, Snow, Fog

**Effects**: Influences atmosphere and potentially gameplay

### SpecialAbility (Data Structure)
Defines unique abilities for troops:
- Properties: Name, Description, Type (Offensive/Defensive/Support), Cooldown, Effect (delegate)

**Effects**: Adds tactical variety and replayability in battles

### Activity
Defines NPC daily activities:
- Sleeping, Working, Wandering, Eating

**Effects**: Influences NPC behavior and world simulation realism

### WorldEventType
Defines types of random world events:
- War, Quest, Siege, Growth

**Effects**: Drives emergent narratives and faction dynamics

### AbilityType
Defines categories for troop special abilities:
- Offensive, Defensive, Support, Passive

**Effects**: Balances ability usage in combat strategies

### EquipmentSlot
Defines equipment attachment points:
- Head, Body, Weapon, Shield, Accessory

**Effects**: Limits equipment variety and stat stacking

### ItemType
Defines item categories:
- Consumable, Weapon, Armor, Accessory

**Effects**: Influences inventory management and usage

### AdvancedResourcesTypes
Defines advanced crafting resources:
- Steel, ReinforcedWood, StoneBriks, Food

**Effects**: Enables complex economy and building upgrades

### TroopRank
Defines rank progression for troops by type:
- Infantry: Recruit, Footman, Veteran, Sergeant, Champion
- Cavalry: Rider, Horseman, Lancer, Knight, Cavalier
- Archer: Bowman, Archer, Sharpshooter, Ranger, Marksman
- Engineer: Tinkerer, Mechanic, Engineer, SiegeMaster, GrandEngineer

**Effects**: Provides progression incentives and stat scaling

### QuestObjective (Data Structure)
Defines individual quest tasks:
- Properties: description, completed (bool)

**Effects**: Structures quest complexity and completion tracking

### EquippedItem (Data Structure)
Defines equipped gear:
- Properties: slot (EquipmentSlot), item (Item)

**Effects**: Manages character loadouts and stat bonuses

### WorldEvent (Data Structure)
Defines active world events:
- Properties: Type (WorldEventType), Description, Duration (days), FactionId, Location (Vector3Int)

**Effects**: Tracks event progression and world state changes

## Modding and Customization

### Unity Editor Integration
- Most managers expose variables for easy tweaking
- Prefab-based system allows visual customization
- ScriptableObjects can be used for data-driven content

### Balance Considerations
- Stat values affect power scaling
- Resource costs impact economy pacing
- Event probabilities control world dynamism

### Performance Optimization
- Use GraphicsOptimizer for quality settings
- Adjust TimeManager scale for gameplay speed
- Limit NPC and party counts for performance

## Development Notes

### Dependencies
- Unity 2021+ (based on project structure)
- TextMesh Pro (referenced in packages)
- Unity UI system

### Known Limitations
- Some systems have placeholder implementations (formations, advanced AI)
- Battle system uses simplified mechanics
- No multiplayer support

### Future Enhancements
- Complete formation system implementation
- Advanced AI decision making
- Multiplayer integration
- Procedural content generation expansion

This documentation provides a comprehensive overview of the Living World RPG codebase, its systems, and customization options. The modular architecture allows for extensive modification and expansion.
