# Wiki Patterns

## Naming

- Source summaries: `sources/YYYY-MM-DD - Source Title.md`
- Entity pages: `entities/Entity Name.md`
- Concept pages: `concepts/Concept Name.md`
- Synthesis pages: `synthesis/Question Or Thesis.md`
- Use human-readable names. Avoid IDs unless the source already uses them.

## Frontmatter

Source summary:

```yaml
---
type: source
status: draft
created: YYYY-MM-DD
updated: YYYY-MM-DD
source_path: "../raw/example.md"
tags: [source]
---
```

Entity/concept/synthesis:

```yaml
---
type: concept
status: draft
created: YYYY-MM-DD
updated: YYYY-MM-DD
sources: []
tags: []
---
```

## Source Page Shape

```md
# Source Title

## Summary

## Key Claims

## Entities

## Concepts

## Useful Quotes

## Links
```

Keep quotes short. Prefer paraphrase plus source citation.

## Entity / Concept Shape

```md
# Page Name

## Definition

## Why It Matters

## Details

## Related

## Sources

## Tensions / Contradictions

## Open Questions
```

## Index Entry

```md
- [[concepts/Concept Name|Concept Name]] - one-line purpose or summary. Sources: N.
```

## Log Entry

```md
## [YYYY-MM-DD] ingest | Source Title

- Source: `Project Wiki/raw/source.md`
- Updated: [[sources/YYYY-MM-DD - Source Title]], [[concepts/Concept Name]]
- Notes: short durable note
```
