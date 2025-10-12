# Character System

## Classes
- `Characters`
- `Player`
- `NPC`

## Description
- The `Characters` class is the base class representing any character in the game. It includes properties such as Name, Health, Mana, Level, Experience, Gold, and a SkillBook.
- The `Player` class inherits from `Characters` and represents the player-controlled character with additional initialization such as starting gold and party leadership.
- The `NPC` class inherits from `Characters` and represents non-player characters with randomized stats and names.

## Completeness
80% - Core stats and leveling are implemented, but skill integration and advanced mechanics (e.g., skill usage) are not fully developed.

## Potential Improvements
- Fully integrate skills with usage mechanics.
- Add more character stats (e.g., attributes like strength, agility).
- Implement AI behaviors for NPCs.
- Add equipment/inventory systems.
