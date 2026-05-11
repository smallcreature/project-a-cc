# Wiki Agent Schema

This folder is an Obsidian vault maintained by an LLM wiki agent.

## Layers

- `raw/` - immutable source documents. Human-owned. Agent reads, normally does not edit.
- `wiki/` - generated markdown wiki. Agent-owned. Human reads and steers.
- `wiki/index.md` - content catalog and navigation.
- `wiki/log.md` - append-only chronological record.

## Generated Wiki Layout

- `wiki/sources/` - one summary page per raw source.
- `wiki/entities/` - people, tools, projects, modules, services, named systems.
- `wiki/concepts/` - ideas, mechanisms, patterns, terms, constraints.
- `wiki/synthesis/` - comparisons, answers, theses, maps, lint reports.
- `wiki/_templates/` - reusable page templates.

## Link Rules

- Use Obsidian links: `[[Page Name]]` or `[[path/page|label]]`.
- Add links from source summaries to entity/concept pages.
- Add backlinks from entity/concept pages to source summaries.
- Keep filenames stable. If renaming, update all backlinks.

## Source Rules

- `raw/` is source of truth.
- Do not rewrite `raw/` unless asked.
- When ingesting pasted text that should persist, create a dated markdown file in `raw/`.
- Use source summaries in `wiki/sources/` for citations and navigation.

## Page Frontmatter

Use YAML frontmatter:

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

Allowed `type`: `source`, `entity`, `concept`, `synthesis`, `index`, `log`.
Allowed `status`: `draft`, `reviewed`, `stale`.

## Workflows

### Ingest

1. Read `wiki/index.md` and recent `wiki/log.md`.
2. Read the raw source.
3. Write or update one `wiki/sources/` summary.
4. Update relevant `entities/`, `concepts/`, and `synthesis/` pages.
5. Update `wiki/index.md`.
6. Append to `wiki/log.md`.
7. Report changed pages and open questions.

### Query

1. Search/read `wiki/index.md`.
2. Read relevant wiki pages.
3. Answer with wiki links and source citations.
4. File durable analyses into `wiki/synthesis/` when requested.

### Lint

Check for broken links, orphan pages, duplicate concepts, stale claims, contradictions, uncited claims, and missing concept/entity pages.
