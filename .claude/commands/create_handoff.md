---
description: Create handoff document for transferring work to another session
---

# Create Handoff

Write a handoff document to transfer your work to another agent in a new session. Be thorough but concise - compact context without losing key details.

## Process

### 1. Filepath & Metadata

Run `scripts/spec_metadata.sh` to gather metadata, then create the file at:
`.claude/handoffs/YYYY-MM-DD_HH-MM-SS_description.md`

### 2. Write the Document

```markdown
---
date: [ISO datetime with timezone]
git_commit: [current commit hash]
branch: [current branch name]
repository: snapflow
topic: "[Feature/Task Name]"
tags: [implementation, relevant-component-names]
status: complete
last_updated: [YYYY-MM-DD]
type: handoff
---

# Handoff: [concise description]

## Task(s)
[Description of tasks and their status: completed / in progress / planned.
If working from a plan, call out which phase you're on and reference the plan file.]

## Critical References
[2-3 most important file paths - plans, research, specs. Leave blank if none.]

## Recent Changes
[Describe recent changes made - use `file.cs:line` syntax]

## Learnings
[Important things discovered - patterns, root causes, non-obvious facts someone picking up this work should know. Include explicit file paths.]

## Artifacts
[Exhaustive list of artifacts produced or updated - plans, research docs, modified files]

## Action Items & Next Steps
[What the next agent should accomplish]

## Other Notes
[Anything else useful - relevant code locations, edge cases, important context]
```

## Guidelines

- **More information, not less** - this is the minimum; always add more if useful
- **Be thorough and precise** - include both top-level objectives and lower-level details
- **Avoid large code blocks** - prefer `file.cs:line` references over inline diffs
- Reference the plan document if you were working from one

Once written, respond with the path so the user can resume in a new session by pointing it at the handoff file.
