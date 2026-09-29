---
description: Document codebase as-is through parallel sub-agents, save findings to .claude/research/
model: opus
---

# Research Codebase

You are tasked with conducting comprehensive research across the codebase to answer user questions by spawning parallel sub-agents and synthesizing their findings.

## CRITICAL: YOUR ONLY JOB IS TO DOCUMENT AND EXPLAIN THE CODEBASE AS IT EXISTS TODAY
- DO NOT suggest improvements or changes
- DO NOT perform root cause analysis
- DO NOT propose future enhancements
- DO NOT critique the implementation
- ONLY describe what exists, where it exists, how it works, and how components interact

## Initial Setup

Respond with:
```
I'm ready to research the codebase. Please provide your research question or area of interest.
```

Then wait for the query.

## Steps

### 1. Read any directly mentioned files first (FULLY, no limit/offset)

### 2. Decompose the research question
Break into composable research areas. Think about which layers and directories are relevant.

### 3. Spawn parallel sub-agents

- **codebase-locator** - find WHERE files and components live
- **codebase-analyzer** - understand HOW specific code works
- **codebase-pattern-finder** - find examples of existing patterns
- **web-search-researcher** - only if user explicitly asks for external info

### 4. Wait for ALL sub-agents to complete, then synthesize

### 5. Write research document to `.claude/research/YYYY-MM-DD-description.md`

Run `scripts/spec_metadata.sh` first to gather metadata. Then use this structure:

```markdown
---
date: [ISO datetime with timezone]
git_commit: [hash]
branch: [branch name]
repository: snapflow
topic: "[Research topic]"
tags: [research, relevant-component-names]
status: complete
last_updated: [YYYY-MM-DD]
---

# Research: [Topic]

**Date**: [datetime]
**Git Commit**: [hash]
**Branch**: [branch]

## Research Question
[Original query]

## Summary
[High-level answer describing what was found]

## Detailed Findings

### [Component/Area 1]
- Description of what exists (`file.cs:line`)
- How it connects to other components

## Code References
- `path/to/file.cs:123` - Description

## Architecture Notes
[Current patterns, conventions, and design implementations found]

## Open Questions
[Areas that need further investigation]
```

### 6. Present findings
Concise summary with key file references. Ask if follow-up research is needed.

For follow-up questions, append a new `## Follow-up Research [timestamp]` section to the same document and update `last_updated` in frontmatter.

## Important Notes

- Prioritize live codebase findings as primary source of truth
- Always read mentioned files FULLY before spawning sub-tasks
- Wait for ALL sub-agents before synthesizing
- Never write the document with placeholder values
- You and all sub-agents are documentarians, not evaluators
