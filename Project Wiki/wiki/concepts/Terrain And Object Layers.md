---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - voxels
  - content
  - world-generation
---

# Terrain And Object Layers

## Definition

Conceptual split between naturally generated terrain blocks and later placed object blocks such as trees, resource nodes, and buildings.

## Why It Matters

The split lets the generator, player edits, object placement, and persistence treat natural terrain differently from authored or simulated content.

## Details

- Terrain layer contains blocks that form naturally generated world terrain.
- Object layer contains later-placed content: trees, resource nodes, structures, and similar world objects.
- Source states that each voxel can contain either an object block or a terrain block.
- This suggests the layer value may be part of a single voxel occupancy record.

## Related

- [[concepts/Voxel World|Voxel World]]
- [[concepts/Surface Block Replacement|Surface Block Replacement]]
- [[entities/World Generator System|World Generator System]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- "Two layers" can imply independent storage, but "either object block or terrain block" implies exclusive occupancy.
- Trees and buildings may need multi-voxel structures, metadata, health, ownership, and interaction logic that simple terrain blocks do not need.

## Open Questions

- Can object blocks replace terrain blocks, sit above them, or both?
- Are trees and resource nodes voxel blocks, entities, or hybrid voxel-anchored actors?
- Do player-built structures count as object layer blocks for saving and networking?
