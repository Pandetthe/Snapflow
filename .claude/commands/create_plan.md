---
description: Create detailed implementation plans through interactive research and iteration
model: opus
---

# Implementation Plan

You are tasked with creating detailed implementation plans through an interactive, iterative process. Be skeptical, thorough, and work collaboratively with the user.

## Initial Response

When invoked:
1. If a file path was provided, read it fully and begin research immediately
2. If no parameters, respond with:
```
I'll help you create a detailed implementation plan.

Please provide:
1. The task/feature description
2. Any relevant context, constraints, or requirements
3. Links to related research or previous implementations

Tip: You can invoke with a context file directly: `/create_plan .claude/research/2026-01-08-feature.md`
```

## Process Steps

### Step 1: Context Gathering & Initial Analysis

1. **Read all mentioned files FULLY** (no limit/offset)
2. **Spawn parallel research tasks**:
   - **codebase-locator** - find all files related to the task
   - **codebase-analyzer** - understand how current implementation works
   - **codebase-pattern-finder** - find similar patterns to model after
3. **Read all files identified by research**
4. **Present informed understanding and focused questions**:
   ```
   Based on my research, I understand we need to [accurate summary].

   I found:
   - [Current implementation detail with file:line]
   - [Relevant pattern or constraint]

   Questions my research couldn't answer:
   - [Specific technical question requiring human judgment]
   ```

### Step 2: Research & Discovery

1. Spawn parallel sub-tasks for comprehensive research
2. Wait for ALL sub-tasks to complete
3. Present findings and design options

### Step 3: Plan Structure

Present outline and get approval before writing details:
```
## Overview
[1-2 sentence summary]

## Implementation Phases:
1. [Phase name] - [what it accomplishes]
2. [Phase name] - [what it accomplishes]
```

### Step 4: Detailed Plan Writing

Write to `.claude/plans/YYYY-MM-DD-description.md`:

````markdown
# [Feature/Task Name] Implementation Plan

## Overview
[Brief description]

## Current State Analysis
[What exists now, key constraints found in code]

## Desired End State
[Specification of the desired end state and how to verify it]

## What We're NOT Doing
[Explicitly list out-of-scope items]

## Implementation Approach
[High-level strategy and reasoning]

## Phase 1: [Descriptive Name]

### Overview
[What this phase accomplishes]

### Changes Required

#### 1. [Component/File Group]
**File**: `path/to/file.cs`
**Changes**: [Summary]

```csharp
// Specific code to add/modify
```

### Success Criteria

#### Automated Verification:
- [ ] Build + tests pass: `scripts/check.sh` (or `scripts/check.sh server|web`)

#### Manual Verification:
- [ ] Feature works as expected
- [ ] No regressions in related features

**Note**: After automated verification passes, pause for manual confirmation before proceeding to the next phase.

---

## Phase 2: [Descriptive Name]
[Similar structure...]

---

## Testing Strategy
[Unit, integration, architecture test considerations]

## References
- Related research: `.claude/research/[relevant].md`
- Similar implementation: `[file:line]`
````

### Step 5: Review

Present plan location and ask for feedback. Iterate until satisfied.

## Important Guidelines

1. **Be Skeptical** - question vague requirements, identify potential issues early
2. **Be Interactive** - get buy-in at each major step
3. **Be Thorough** - read all context files completely, include file:line references
4. **No Open Questions in Final Plan** - research or ask for clarification immediately; never write the plan with unresolved questions
5. **Separate automated vs manual** success criteria in every phase
