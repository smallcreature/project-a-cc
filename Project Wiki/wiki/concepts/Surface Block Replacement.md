---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - terrain
  - biomes
  - world-generation
---

# Surface Block Replacement

## Definition

Post-generation pass that replaces some default stone blocks with biome-specific soil and grass blocks.

## Why It Matters

This step converts raw terrain mass into readable surface biomes and prevents every generated surface from looking like exposed stone.

## Details

- Each biome defines its own soil block and grass block.
- Stone near the terrain surface becomes soil based on Minecraft-like rules that still need verification.
- Initial rule hypothesis: if a stone block is at or above base height and touches air, it can become soil.
- Several blocks below that surface soil block also become soil.
- Soil depth varies within a configurable range.
- High steepness prevents stone from becoming soil, preserving rocky cliffs.
- Grass generation rule: if a generated soil block is above base height and touches air, replace it with grass.
- Grass replacement happens only during initial world generation.

## Related

- [[concepts/Biome-First World Generation|Biome-First World Generation]]
- [[concepts/Terrain Height Pipeline|Terrain Height Pipeline]]
- [[concepts/Terrain And Object Layers|Terrain And Object Layers]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- A simple "touches air" rule could turn cave walls or overhang undersides into soil unless constrained to top-surface exposure.
- "Above base height" may conflict with later modifiers that change final surface height away from the original biome base height.

## Open Questions

- Should surface replacement use base height, final height, or density gradient?
- How exactly is steepness computed and thresholded?
- Should player-created soil later turn into grass, or is grass strictly generation-only as source states?
