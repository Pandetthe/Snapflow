---
description: Implement technical plans from .claude/plans/ with verification
---

# Implement Plan

You are tasked with implementing an approved technical plan from `.claude/plans/`. These plans contain phases with specific changes and success criteria.

## Getting Started

When given a plan path:
- Read the plan completely and check for existing checkmarks (`- [x]`)
- Read all files mentioned in the plan
- **Read files fully** - never use limit/offset, you need complete context
- Create a todo list to track progress
- Start implementing if you understand what needs to be done

If no plan path provided, ask for one.

## Implementation Philosophy

Plans are carefully designed, but reality can be messy. Your job is to:
- Follow the plan's intent while adapting to what you find
- Implement each phase fully before moving to the next
- Verify your work makes sense in the broader codebase context
- Update checkboxes in the plan as you complete sections

If you encounter a mismatch with the plan:
- STOP and think deeply about why
- Present the issue clearly:
  ```
  Issue in Phase [N]:
  Expected: [what the plan says]
  Found: [actual situation]
  Why this matters: [explanation]

  How should I proceed?
  ```

## Verification

After implementing a phase:
- Run all automated success criteria:
  ```bash
  scripts/check.sh
  ```
- Fix any issues before proceeding
- Update checkboxes in the plan using Edit
- **Pause for human verification**:
  ```
  Phase [N] Complete - Ready for Manual Verification

  Automated verification passed:
  - [checks that passed]

  Please perform the manual verification steps from the plan:
  - [manual items]

  Let me know when done so I can proceed to Phase [N+1].
  ```

Do not check off manual items until the user confirms. If instructed to execute multiple phases consecutively, skip pausing until the last phase.

## If You Get Stuck

- Make sure you've read and understood all the relevant code
- Consider if the codebase has evolved since the plan was written
- Present the mismatch clearly and ask for guidance

## Resuming Work

If the plan has existing checkmarks:
- Trust that completed work is done
- Pick up from the first unchecked item
- Verify previous work only if something seems off
