---
type: entity
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - system
  - world-generation
  - project-arcane
---

# World Generator System

## Definition

Project-owned system that generates, stores, streams, renders, and edits the bounded wrapping voxel world for [[entities/Project Arcane|Project Arcane]].

## Why It Matters

This system is the base layer for terrain, mining, building, resource placement, exploration, biome identity, and later world-state changes from magic or corruption.

## Details

- World unit: [[concepts/Voxel World|1m voxel]].
- Storage unit: [[concepts/Chunked Voxel World|chunk]] of `16*512*16` voxels.
- Max footprint: `256*256` chunks, equal to `4096*4096` horizontal voxels.
- Vertical range: fixed `512` voxels, no vertical chunking in the current design.
- Topology: bounded world with [[concepts/Horizontal World Wrapping|horizontal wrapping]].
- Content model: [[concepts/Terrain And Object Layers|terrain and object layers]] with mutually exclusive voxel occupancy.
- Generation approach: [[concepts/Biome-First World Generation|biome-first]] terrain and block selection.
- Performance direction: compact voxel records, traversal order optimization, and possible octree storage.

## Related

- [[synthesis/World Generation Pipeline|World Generation Pipeline]]
- [[concepts/Terrain Height Pipeline|Terrain Height Pipeline]]
- [[concepts/Cave Generation|Cave Generation]]
- [[concepts/Surface Block Replacement|Surface Block Replacement]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- Source says the world has two layers, but also says each voxel can contain either terrain or object block. This implies one occupancy channel with semantic type, not two independent block stacks.
- Rendering wrap might be shader-assisted, but entity movement, physics, saves, AI, and networking still need canonical world coordinates.
- Octree is proposed, but chunk-local editing and meshing may also need dense or palette-compressed chunk sections.

## Open Questions

- What is the canonical coordinate system for wrapped X/Z positions?
- Are chunk coordinates stored canonical `[0..255]` or can simulation use unwrapped coordinates and normalize at boundaries?
- How are player edits persisted: full chunk data, sparse deltas over deterministic generation, or hybrid?
- Does multiplayer authority live on chunks, entities, or a global world service?
