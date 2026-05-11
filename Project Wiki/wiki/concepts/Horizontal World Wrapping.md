---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - world-generation
  - topology
  - rendering
---

# Horizontal World Wrapping

## Definition

World topology where horizontal X/Z boundaries connect to the opposite side of the map, while vertical bounds remain fixed.

## Why It Matters

Wrapping lets a finite world feel continuous. It also creates a strong technical requirement: movement, visibility, physics, AI, multiplayer replication, and rendering must agree at map edges.

## Details

- As player approaches a horizontal edge, chunks from the opposite edge should render beyond it.
- Crossing an edge moves any object or entity to the opposite boundary.
- Seamlessness is required for both visual continuity and physical movement.
- The source suggests shader assistance as possible rendering technique, but marks it uncertain.

## Related

- [[concepts/Chunked Voxel World|Chunked Voxel World]]
- [[concepts/Voxel World|Voxel World]]
- [[entities/World Generator System|World Generator System]]

## Sources

- [[World Generator]]

## Tensions / Contradictions

- Shader-only wrapping can solve visuals, but cannot by itself solve physics queries, collision, AI pathing, object persistence, or networking.
- Teleporting entities at boundaries is simple, but can create discontinuities in velocity, interpolation, camera smoothing, and multiplayer prediction.

## Open Questions

- Should simulation use canonical wrapped coordinates or unbounded logical coordinates with modulo sampling?
- How should line-of-sight, projectiles, pathfinding, and audio work across the seam?
- How are chunks duplicated or mirrored near the visual boundary without duplicating authoritative objects?
