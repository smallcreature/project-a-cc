---
type: source
status: draft
created: 2026-05-12
updated: 2026-05-12
source_path: "../../raw/2026-05-12 - World Generator.md"
tags: [source, world-generation, voxels, project-arcane]
---

# World Generator

## Summary

Source defines the [[entities/World Generator System|World Generator System]] for [[entities/Project Arcane|Project Arcane]] as a large but bounded 1m [[concepts/Voxel World|voxel world]] that players can modify by adding and removing blocks.

The world is stored as [[concepts/Chunked Voxel World|chunks]] of `16*512*16` voxels, up to `256*256` horizontal chunks. Vertical space is fixed at 512 voxels and is not split into vertical chunks. Horizontal X/Z axes wrap around the map, so the world behaves like a seamless torus while vertical boundaries stay fixed.

Generation is [[concepts/Biome-First World Generation|biome-first]]: most block decisions come from a 3D biome map composed from layered noise. The [[concepts/Terrain Height Pipeline|terrain height pipeline]] starts from per-biome base heights, then applies height noise, continentalness, erosion, Peaks and Valleys, cliffs and overhangs, cave carving, and surface block replacement.

The source emphasizes generation speed, compact voxel data, traversal order, and possible octree storage. It also separates [[concepts/Terrain And Object Layers|terrain and object layers]] so natural terrain and placed content can be reasoned about differently.

## Key Claims

- Each voxel represents `1m` of world volume and can be modified by the player.
- Chunking exists so local edits do not require rebuilding the whole world.
- The max world footprint is `256*256` chunks, or `4096*4096` voxels horizontally at `16` voxels per chunk.
- Horizontal wrapping must be seamless for both rendering and physical movement.
- Each voxel can contain either a terrain block or an object block.
- Biomes drive most terrain decisions through a 3D biome map.
- Surface material replacement happens after terrain and cave carving.
- Grass replacement is generation-only, not a continuing runtime rule.

## Entities

- [[entities/Project Arcane|Project Arcane]]
- [[entities/World Generator System|World Generator System]]

## Concepts

- [[concepts/Voxel World|Voxel World]]
- [[concepts/Chunked Voxel World|Chunked Voxel World]]
- [[concepts/Horizontal World Wrapping|Horizontal World Wrapping]]
- [[concepts/Terrain And Object Layers|Terrain And Object Layers]]
- [[concepts/Biome-First World Generation|Biome-First World Generation]]
- [[concepts/Terrain Height Pipeline|Terrain Height Pipeline]]
- [[concepts/Cave Generation|Cave Generation]]
- [[concepts/Surface Block Replacement|Surface Block Replacement]]

## Useful Quotes

- "Мир представляет собой воксельную сетку, где каждый воксель обозначает объем в 1m."
- "Подход к генерации при этом - biome-first..."
- "Главное в таком зацикливании - бесшовность..."

## Links

- Raw: `Project Wiki/raw/2026-05-12 - World Generator.md`
- Pipeline map: [[synthesis/World Generation Pipeline|World Generation Pipeline]]
- External reference mentioned by source: [The World Generation of Minecraft](https://www.alanzucconi.com/2022/06/05/minecraft-world-generation/) by Alan Zucconi, Jun 5 2022.
