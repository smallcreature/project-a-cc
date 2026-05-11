---
type: synthesis
status: draft
created: 2026-05-11
updated: 2026-05-12
sources:
  - "[[Project Arcane MVP Concept]]"
  - "[[World Generator]]"
tags:
  - map
  - synthesis
  - project-arcane
---

# Project Arcane System Map

## High-Level Chain

[[concepts/Survival Systems|Survival]] creates constraints -> [[concepts/Gathering|Gathering]], [[concepts/Farming|Farming]], [[concepts/Hunting|Hunting]], [[concepts/Cooking|Cooking]], and [[concepts/Storage And Stockpiles|Storage]] stabilize life -> [[concepts/Immersive Research|Research]] unlocks recipes and spells -> [[concepts/Magic System|Magic]] grants power and [[concepts/Magical Automation|automation]] -> stronger player reaches harder [[concepts/Expeditions And Adventures|expeditions/adventures]] and [[concepts/Bosses And Points Of Interest|bosses/POIs]] -> rare resources unlock deeper research.

## World Chain

[[entities/World Generator System|World generator]] creates [[concepts/Voxel World|editable voxel world]] -> [[concepts/Chunked Voxel World|chunks]] localize edits and streaming -> [[concepts/Horizontal World Wrapping|horizontal wrapping]] makes bounded world feel continuous -> [[concepts/Biome-First World Generation|biomes]] drive [[concepts/Terrain Height Pipeline|terrain shape]] -> [[concepts/Cave Generation|caves]] and [[concepts/Surface Block Replacement|surface replacement]] create exploration and readable biome surfaces.

Relevant page: [[synthesis/World Generation Pipeline|World Generation Pipeline]].

## Magic Chain

[[concepts/Aura And Magical Energies|Aura energies]] + [[concepts/Aspects|aspects]] + mana -> [[concepts/Spellcasting|spells]], [[concepts/Alchemy|alchemy]], [[concepts/Magical Crafting|magical crafting]], [[concepts/Magical Buildings|magical buildings]] -> automation and power -> [[concepts/Aberration And Corruption|aberration/corruption]] -> new threats and rare resources.

## Co-op Chain

Deep systems -> [[concepts/Specializations|specialized roles]] -> faster parallel progression -> shared infrastructure -> harder [[concepts/Bosses And Points Of Interest|bosses/dungeons]]/[[concepts/PvE Hunters|hunters]] -> stronger group engagement.

Relevant page: [[concepts/Deep Co-op|Deep Co-op]].

## Risk / Reward Chain

Safe play -> cleanse corruption -> stable base and aura.

Risky play -> study corruption -> stronger magic and rare resources -> more danger and world instability.

Relevant page: [[concepts/Aberration And Corruption|Aberration And Corruption]].

## Current Gaps

- Building materials and stepped construction are placeholders.
- Mining, mundane research, crafting, and leveling need full systems.
- PvE hunter simulation depth is undefined.
- MVP cut needs explicit scope.
- World generation algorithms need exact implementation choices for storage, wrapping, erosion, Peaks and Valleys, cliffs, overhangs, caves, and surface steepness.
