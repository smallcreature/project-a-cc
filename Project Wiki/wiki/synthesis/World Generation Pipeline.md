---
type: synthesis
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - synthesis
  - world-generation
  - project-arcane
---

# World Generation Pipeline

## Pipeline

[[concepts/Biome-First World Generation|Biome map]] -> per-biome base height -> 2D height noise -> continentalness spline -> erosion spline -> Peaks and Valleys / weirdness -> 3D cliffs and overhangs -> [[concepts/Cave Generation|cave carving]] -> [[concepts/Surface Block Replacement|soil and grass replacement]] -> [[concepts/Terrain And Object Layers|object layer placement]].

## World Model

[[concepts/Voxel World|Voxel world]] -> [[concepts/Chunked Voxel World|full-height chunks]] -> [[concepts/Horizontal World Wrapping|horizontal torus topology]] -> player edits -> chunk-local rebuild/persistence.

## Implementation Pressure

- Dense max world is too large to store naively, so generation must be deterministic and storage must be sparse, compressed, or delta-based.
- Full-height chunks simplify biome and terrain generation, but dirty mesh/collision updates likely need smaller internal regions.
- Wrapping must be solved below rendering because objects and entities physically cross world boundaries.
- Terrain and cave math should converge on a density model if cliffs, overhangs, and caves become first-class.

## Design Dependencies

- [[concepts/Biome-First World Generation|Biomes]] define region identity and material palette.
- [[concepts/Terrain Height Pipeline|Terrain height and density]] define traversal and visual drama.
- [[concepts/Cave Generation|Caves]] create exploration/mining space.
- [[concepts/Surface Block Replacement|Surface replacement]] makes biome surfaces legible.

## Open Questions

- Is MVP world generation CPU-only, compute-shader assisted, or hybrid?
- Should generation output full block data immediately, or lazily materialize blocks only when chunks are edited or meshed?
- What exact algorithms should be copied, adapted, or avoided from Minecraft-like generation?
- What is the minimum viable biome set for first playable terrain?
