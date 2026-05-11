---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - chunks
  - voxels
  - performance
---

# Chunked Voxel World

## Definition

Chunking scheme that divides the world horizontally into fixed `16*512*16` voxel chunks while keeping the vertical axis inside each chunk.

## Why It Matters

Chunks localize generation, modification, meshing, saving, loading, and network synchronization. They avoid rebuilding the whole world after one block edit.

## Details

- Chunk dimensions: `16` voxels X, `512` voxels Y, `16` voxels Z.
- Voxels per chunk: `131072`.
- Max world size: `256*256` chunks.
- Max horizontal size: `4096*4096` voxels.
- Max dense voxel count: `4096*512*4096`, or `8589934592` voxels.
- Vertical chunking is intentionally avoided for generator speed in the current design.
- The source proposes octree storage, likely to avoid dense memory cost for mostly uniform terrain regions.

## Related

- [[concepts/Voxel World|Voxel World]]
- [[concepts/Horizontal World Wrapping|Horizontal World Wrapping]]
- [[entities/World Generator System|World Generator System]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- One full-height chunk is simple for generation, but large for meshing, collision, saving, and dirty-region updates.
- Octree storage can compress emptiness and solid regions, but may complicate fast block edits and GPU mesh extraction.

## Open Questions

- What is the active loaded chunk radius around players?
- Are chunks generated whole, sectioned internally, or streamed by vertical bands for mesh/collision updates?
- What format stores modified chunks after player edits?
