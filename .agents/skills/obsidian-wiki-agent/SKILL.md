---
name: obsidian-wiki-agent
description: >
  Transform raw technical text, notes, articles, specs, transcripts, and source documents into
  a persistent Obsidian markdown wiki. Use when the user asks to ingest raw text into Project Wiki,
  build linked wiki pages, create Obsidian graph-friendly maps of concepts/entities, maintain
  index.md/log.md, query the wiki, or lint an LLM-maintained knowledge base.
---

# Obsidian Wiki Agent

Act as dedicated wiki maintainer. User curates sources and asks questions; agent writes and maintains wiki.

## Default Paths

- Raw sources: `Project Wiki/raw/`
- Generated wiki: `Project Wiki/wiki/`
- Schema: `Project Wiki/AGENTS.md`
- Index: `Project Wiki/wiki/index.md`
- Log: `Project Wiki/wiki/log.md`

Always read `Project Wiki/AGENTS.md`, `Project Wiki/wiki/index.md`, and recent `Project Wiki/wiki/log.md` entries before changing wiki pages.

## Core Rules

- Treat `raw/` as immutable source truth. Do not rewrite raw sources unless user explicitly asks.
- Own `wiki/`: create, update, merge, split, and cross-link pages there.
- Use Obsidian wiki links: `[[Page Name]]` or `[[path/page|label]]`.
- Preserve uncertainty. Mark weak claims as `status: draft` or `confidence: low`.
- Track contradictions in page sections named `## Tensions / Contradictions`.
- Cite source summary pages or raw source paths for non-obvious claims.
- Prefer many small durable pages over one giant summary.
- Keep filenames stable once linked. If renaming, update backlinks.

## Operations

### Ingest

1. Identify source file or pasted text. If pasted text has no file, create a raw source file only if user wants persistence.
2. Read schema, index, log, and relevant existing wiki pages.
3. Create or update one source summary in `wiki/sources/`.
4. Extract entities, concepts, systems, workflows, decisions, terms, constraints, and open questions.
5. Update or create pages in `wiki/entities/`, `wiki/concepts/`, and `wiki/synthesis/`.
6. Add bidirectional links where useful.
7. Update `wiki/index.md`.
8. Append one chronological entry to `wiki/log.md`.
9. Report changed pages and unresolved questions.

### Query

1. Read index first.
2. Read relevant pages and source summaries.
3. Answer with wiki links and citations.
4. If answer creates reusable synthesis, offer or create a new page in `wiki/synthesis/` when user asked for persistence.

### Lint

Check for broken links, orphan pages, duplicate concepts, stale claims, contradictions, missing entity/concept pages, uncited claims, and vague page names. Update pages only when fixes are obvious; otherwise write a lint report page in `wiki/synthesis/`.

## Page Patterns

Use `references/wiki-patterns.md` when creating or refactoring page structure.
