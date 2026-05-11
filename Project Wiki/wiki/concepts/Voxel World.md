---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - voxels
  - world-generation
---

# Voxel World

## Definition

World representation where each voxel is a `1m` volume that can be empty air or hold one block.

## Why It Matters

Voxels make terrain editable at runtime: players can remove blocks, place blocks, mine terrain, and alter the world without replacing the whole level.

## Details

- Air is absence of a block.
- A filled voxel stores block identity and semantic layer data.
- The source distinguishes terrain blocks from object blocks through [[concepts/Terrain And Object Layers|terrain and object layers]].
- Runtime edits imply chunk-level mesh rebuilds, persistence, collision updates, and replication decisions.

## Related

- [[concepts/Chunked Voxel World|Chunked Voxel World]]
- [[concepts/Horizontal World Wrapping|Horizontal World Wrapping]]
- [[entities/World Generator System|World Generator System]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- The source does not yet define whether liquids, plants, buildings, and resource nodes are all voxel blocks or if some become separate entities anchored to voxels.

## Open Questions

- What minimum block record is required at runtime: block id, layer, biome, health, metadata, light, moisture?
- Can a voxel store both a terrain block and a thin object/decal, or is occupancy strictly exclusive?
