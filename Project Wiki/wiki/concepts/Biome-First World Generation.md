---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - biomes
  - world-generation
  - noise
---

# Biome-First World Generation

## Definition

Generation approach where most block and terrain decisions originate from a biome map rather than from a biome-independent heightfield.

## Why It Matters

Biome-first generation makes biome identity a primary driver of height, materials, grass, soil, and later content. This supports distinct regions with different terrain and resource behavior.

## Details

- Biome map is described as a 3D texture combining multiple noises.
- Default biome fills the full texture first.
- Additional biomes are overlaid through 3D Perlin noise.
- Each biome has a configurable color on the biome map.
- Each biome defines base height.
- Each biome should define unique soil and grass block types.
- Biome data feeds [[concepts/Terrain Height Pipeline|terrain height]] and [[concepts/Surface Block Replacement|surface material replacement]].

## Related

- [[concepts/Terrain Height Pipeline|Terrain Height Pipeline]]
- [[concepts/Cave Generation|Cave Generation]]
- [[synthesis/World Generation Pipeline|World Generation Pipeline]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- The source calls the biome map a 3D texture, but several early terrain stages are 2D height modifiers. The implementation needs a clear sampling contract: which steps use X/Z only and which use X/Y/Z.

## Open Questions

- How are biome borders blended for terrain height and material transitions?
- Are underground biomes planned, or is the 3D biome map mainly future-proofing?
- Which biome parameters are authorable in Unity assets?
