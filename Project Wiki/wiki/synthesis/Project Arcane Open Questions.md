---
type: synthesis
status: draft
created: 2026-05-11
updated: 2026-05-12
sources:
  - "[[Project Arcane MVP Concept]]"
  - "[[World Generator]]"
tags:
  - questions
  - synthesis
  - project-arcane
---

# Project Arcane Open Questions

## MVP Scope

- Which systems are mandatory for the first playable MVP?
- Which magic schools ship first?
- Which survival systems must be fully simulated versus simplified?
- Which automation features are MVP and which are later?

## Onboarding

- What does player know about magic at start?
- How is hidden information revealed without frustration?
- What is the first "wow, magic changed rules" moment?

## Research

- What are exact mundane research mini-game rules?
- How are failed hypotheses useful?
- How are resource properties discovered, stored, and shown?

## Magic Economy

- What are baseline costs for mana, aura energy, and aspects?
- How fast does aura recover?
- What are exact thresholds for corruption stages?
- Can corruption be fully reversed?

## Co-op

- How does progression share across players?
- Can players specialize without blocking solo progression?
- How are discovered spells/recipes shared?

## PvE Hunters

- What exact player threshold spawns hunters?
- Do hunters truly simulate player-like rules?
- How are base raids telegraphed and balanced?

## Building And Logistics

- How are rooms detected?
- How do workstation bonuses communicate?
- When do roads, outposts, transport, and portals enter progression?

## World Generation

- What exact storage model backs [[concepts/Chunked Voxel World|full-height voxel chunks]]: dense arrays, palette compression, octree, sparse deltas, or hybrid?
- How is [[concepts/Horizontal World Wrapping|horizontal wrapping]] implemented across rendering, physics, AI, networking, saves, and interpolation?
- What final math defines [[concepts/Terrain Height Pipeline|erosion, Peaks and Valleys, cliffs, and overhangs]]?
- Should terrain generation be a heightfield plus 3D modifiers, or a single density function?
- How are [[concepts/Cave Generation|cheese and spaghetti caves]] parameterized and combined?
- How does [[concepts/Surface Block Replacement|surface replacement]] avoid turning cave walls or overhang undersides into soil/grass?
