---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - terrain
  - noise
  - world-generation
---

# Terrain Height Pipeline

## Definition

Ordered set of height and density modifiers used to decide whether each voxel is stone or air before cave carving and surface material replacement.

## Why It Matters

This pipeline controls macro terrain shape: oceans, continents, mountains, valleys, cliffs, overhangs, and traversable surface drama.

## Details

Planned stages:

1. Sample biome from [[concepts/Biome-First World Generation|biome map]].
2. Read per-biome base height.
3. Treat voxels above base height as air and below it as default stone.
4. Apply configurable 2D Perlin height noise.
5. Apply continentalness: 2D Perlin value plus threshold/spline table that remaps continentalness to base height.
6. Apply erosion: likely same "height map plus spline thresholds" pattern, but exact Minecraft-like behavior is unresolved.
7. Apply Peaks and Valleys / weirdness-like noise, likely without the same threshold table.
8. Apply cliffs and overhangs through 3D noise.
9. Run [[concepts/Cave Generation|cave generation]].
10. Run [[concepts/Surface Block Replacement|surface block replacement]].

Noise parameters mentioned by the source:

- intensity
- contrast
- size
- octave count

## Related

- [[concepts/Biome-First World Generation|Biome-First World Generation]]
- [[concepts/Cave Generation|Cave Generation]]
- [[concepts/Surface Block Replacement|Surface Block Replacement]]
- [[synthesis/World Generation Pipeline|World Generation Pipeline]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- The design starts as a 2D heightfield, then adds 3D cliffs, overhangs, and caves. Implementation likely needs a density function rather than only a final surface height.
- Erosion and Peaks and Valleys are desired to feel like Minecraft, but the exact math is still explicitly uncertain.

## Open Questions

- What curve/spline format should represent continentalness and erosion thresholds?
- Does the generator evaluate full voxel density, or derive a height first and apply 3D modifiers later?
- What exact meaning should "steepness" use for surface replacement: local height gradient, normal angle, or density gradient?
