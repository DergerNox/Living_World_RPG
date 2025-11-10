# Living World RPG – Comprehensive Refactoring Plan

This plan scopes the systematic refactoring work I’ll drive, with enough detail for you to track progress. Each phase addresses SOLID compliance, modularity, and procedural-generation robustness without pursuing churn for its own sake. Tasks are ordered to stabilize project foundations before layering on higher-level gameplay refactors.

---

## Phase 0 – Groundwork (Project-Wide Hygiene)
1. **Inventory Current State**
   - Freeze architectural documentation (`Living_World_RPG_Documentation.md`) as the authoritative reference.
   - Capture before/after class diagrams (existing UML file) to visualize coupling hot spots.
2. **Tooling & Standards**
   - Introduce EditorConfig + consistent C# analyzers (IDEs already hint issues but no shared settings).
   - Add CI lint/test hooks if feasible (even minimal `dotnet format`/`dotnet test` targets).
3. **Dependency Review**
   - Identify runtime singletons vs. static classes; list candidates that need inversion-of-control later.

_Exit Criteria_: everyone shares the same style/tool baselines; documentation + diagrams reflect reality; no code touched yet.

---

## Phase 1 – Core Architecture Alignment (SOLID First)
### 1.1 Single Responsibility / Interface Segmentation
- Extract interfaces for high-variance behaviors:
  - `IWorldGenerator`, `ISettlementRepository`, `IBattleResolver`, `ISkillEffect`, `ISaveProvider`.
- Move logging, audio triggers, and random generation to dedicated services to avoid bloated managers.

### 1.2 Dependency Inversion
- Introduce lightweight service locator / dependency container suited for Unity (ScriptableObject-based config or static registry).
- Inject services into MonoBehaviours via serialized references + bootstrapper.

### 1.3 Data Ownership
- Split data models (pure C# classes) from presenter MonoBehaviours.
- Ensure serialization-ready DTOs exist for Save/Load, with mapping helpers.

_Exit Criteria_: Major systems depend on abstractions rather than concretes; responsibilities per class are clear; data layer separated from Unity-specific code.

---

## Phase 2 – Modularity & Subsystem Boundaries
### 2.1 World Simulation Module
- Encapsulate world state (factions, settlements, parties) behind repositories with query APIs (spatial lookups, filtering).
- Convert global static lists in `WorldHandler` to injectable services; keep caches internally.

### 2.2 Systems as Feature Modules
- For each subsystem (Battle, Quest, Economy, Time/Weather, AI), define:
  - Public facade (`IBattleService` etc.)
  - Internal components (data, logic, presenters)
  - Event contracts they emit/consume.
- Refactor cross-system calls into event bus or mediator to reduce bidirectional references.

### 2.3 UI/Presentation Separation
- Move UI scripts out of core logic folders.
- Define DTOs/view-models bridging services → UI to avoid UI querying game data directly.

_Exit Criteria_: Codebase can be reasoned about in modules with explicit APIs, minimal static coupling, and UI isolated from logic.

---

## Phase 3 – Procedural Generation Emphasis
### 3.1 World Generation Pipeline
- Rebuild `WorldGenerator` as a pipeline:
  1. Terrain noise + biome assignment
  2. Resource & climate layers
  3. Settlement placement with constraints (distance, biome suitability)
  4. Faction seeding + diplomacy matrix
- Each step -> pluggable strategy implementing `IWorldGenStage`.

### 3.2 Data-Driven Generation Inputs
- Externalize parameters via ScriptableObjects (biome definitions, settlement templates, faction archetypes).
- Add seed management + deterministic runs for debugging/tests.

### 3.3 Procedural Content Hooks
- Provide extension points for quests, encounters, and events to consume generation outputs (e.g., register callbacks when new settlements spawn).

_Exit Criteria_: Procedural generation configurable, staged, and testable; downstream systems read structured data produced by the pipeline.

---

## Phase 4 – Skill & Combat Systems (High-Impact Gameplay Refactor)
### 4.1 Apply Existing Skill Refactor Plan
- Implement interfaces, ScriptableObject-driven `SkillData`, factories, cooldown/status systems per `Skill_System_Refactoring_Plan.md`.
- Remove obsolete `SkillBook` and hard-coded entries once replacement stable.

### 4.2 Battle Resolver Modernization
- Convert `Battle` to strategy pattern per troop behavior; support modifiers from skills, formations, environment.
- Introduce simulation contexts to allow AI previews and automated tests.

### 4.3 AI/Party Integration
- Ensure AI decisions consume skill/battle abstractions, not raw classes.
- Add hooks to procedural generation for AI party templates and behaviors.

_Exit Criteria_: Skills are data-driven and modular; Battle system consumes those abstractions; AI interacts via defined contracts.

---

## Phase 5 – Persistence, Time/Weather, and Economy Polishing
### 5.1 Save/Load Refactor
- Replace ad-hoc serialization with versioned save schema + migration support.
- Ensure each module exposes `GetSnapshot()/LoadSnapshot()` so saves remain modular.

### 5.2 Time & Weather Modularization
- Convert managers into services broadcasting events (tick, day-night, weather change), not direct method calls.
- Allow systems (agriculture, NPC schedules, VFX) to subscribe without tight coupling.

### 5.3 Economy & Trade Balancing
- Use ScriptableObjects for commodities/markets.
- Introduce simulation tests to ensure no runaway inflation or negative inventories after time skips.

_Exit Criteria_: Persistence is robust and modular; temporal systems drive reactive gameplay; economy data lives outside code.

---

## Phase 6 – Testing & Validation
1. **Unit Tests**
   - World-gen stages (deterministic with seeds)
   - Skill effects & cooldown logic
   - Battle resolution edge cases (terrain, formations, status effects)
2. **Integration Tests**
   - Save/load round-trips
   - Procedural generation feeding quests/events
3. **Performance Benchmarks**
   - Large world generation
   - 100+ troop battles
   - Time/weather ticks over extended sessions

_Exit Criteria_: Automated coverage for critical systems, plus benchmark baselines to catch regressions.

---

## Phase 7 – Documentation & Handoff
- Update `Living_World_RPG_Documentation.md` per phase changes, add module diagrams.
- Provide migration notes for any API changes (public methods, serialized fields).
- Ensure TODOs reflect only remaining actionable items post-refactor.

---

## Execution Guidelines
- Tackle phases sequentially; don’t start a higher phase until prerequisites stabilize.
- Prefer small, reviewable PRs per subsystem.
- Maintain backward compatibility (adapters) until new modules fully replace old ones.
- Use feature flags/preprocessor directives when ripping out legacy systems to keep main branch playable.

This plan keeps refactoring deliberate, targets high-value areas (SOLID, modularity, procedural generation), and avoids change for change’s sake. I’ll reference it while working so we stay aligned. Let me know if you want any phase reordered or if additional focus areas should be called out. 
