---
description: Iterate on existing implementation plans with research and updates
model: opus
---

# Iterate Implementation Plan

You are tasked with updating existing implementation plans based on user feedback.

## Initial Response

When invoked:

- **If NO plan file provided**: Ask which plan to update. Tip: `ls -lt .claude/plans/ | head`
- **If plan file provided but NO feedback**: Ask what changes to make
- **If BOTH provided**: Proceed immediately to Step 1

## Process Steps

### Step 1: Read and Understand Current Plan

Read the existing plan file COMPLETELY (no limit/offset). Understand the current structure, phases, and scope.

### Step 2: Research If Needed

Only spawn research tasks if the changes require new technical understanding:

- **codebase-locator** - find relevant files
- **codebase-analyzer** - understand implementation details
- **codebase-pattern-finder** - find similar patterns

Be specific about which layer/directory to look in.

### Step 3: Confirm Before Changing

```
Based on your feedback, I understand you want to:
- [Change 1 with specific detail]
- [Change 2 with specific detail]

My research found:
- [Relevant code pattern or constraint]

I plan to update the plan by:
1. [Specific modification]
2. [Another modification]

Does this align with your intent?
```

### Step 4: Update the Plan

- Use Edit for surgical changes - preserve existing structure unless explicitly changing it
- Maintain the distinction between automated vs manual success criteria
- Keep file:line references accurate
- If adding a new phase, ensure it follows the existing pattern
- If modifying scope, update "What We're NOT Doing" section

### Step 5: Review

Present changes made and ask if further adjustments are needed.

## Important Guidelines

1. **Be Skeptical** - question change requests that seem problematic
2. **Be Surgical** - make precise edits, not wholesale rewrites
3. **No Open Questions** - research or get clarification before updating
4. **Always maintain** automated vs manual success criteria distinction
