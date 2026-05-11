---
type: concept
status: draft
created: 2026-05-12
updated: 2026-05-12
sources:
  - "[[World Generator]]"
tags:
  - caves
  - noise
  - world-generation
---

# Cave Generation

## Definition

Voxel carving stage that replaces generated stone with air using paired 3D noise systems for large cavities and narrow tunnels.

## Why It Matters

Caves create exploration, mining routes, underground resources, traversal risk, and future biome/content hooks.

## Details

- Cave generation runs after cliffs and overhangs.
- Cheese caves create rounded 3D cavities inside configured height limits.
- Spaghetti caves create narrow tunnels from selected "gray" bands of 3D noise.
- Spaghetti settings should control tunnel thickness and length.
- Cheese caves and spaghetti caves should work as a pair.
- Each cave type needs configurable vertical limits.
- The source asks to verify exact Minecraft-like implementation details before finalizing.
- External reference checked: Alan Zucconi's Minecraft article describes Minecraft 1.18 cave types as thresholded 3D Perlin noise, with different parameters producing large caves, long tunnels, or finer passage networks.

## Related

- [[concepts/Terrain Height Pipeline|Terrain Height Pipeline]]
- [[concepts/Biome-First World Generation|Biome-First World Generation]]
- [[synthesis/World Generation Pipeline|World Generation Pipeline]]

## Sources

- [[World Generator]]
- External reference mentioned by source: [The World Generation of Minecraft](https://www.alanzucconi.com/2022/06/05/minecraft-world-generation/).

## Tensions / Contradictions

- The source says cheese caves and spaghetti caves work exclusively in a pair, but does not define whether one can carve without the other in a given region.
- "Gray areas" for spaghetti caves needs a numerical threshold/band definition.

## Open Questions

- Are cave networks deterministic by seed and chunk coordinate only?
- Should caves affect biome sampling, resource distribution, or surface collapse?
- Are noodle caves, pillars, aquifers, or lava levels in scope?
